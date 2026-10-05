using JetBrains.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Telegram.Bot.Types;
using TheAirBlow.Stateful.Exceptions;

namespace TheAirBlow.Stateful.MongoDB;

/// <summary>
/// MongoDB message state handler
/// </summary>
[PublicAPI]
public class MongoStateHandler : IMessageStateHandler {
    /// <summary>
    /// MongoDB collection to use
    /// </summary>
    public IMongoCollection<MessageState> Collection { get; set; }

    /// <summary>
    /// Registers necessary MongoDB conventions
    /// </summary>
    public static void RegisterConvention()
        => ConventionRegistry.Register("TheAirBlow.Stateful.MongoDB", 
            new ConventionPack {
                new NoIdMemberConvention(),
                new IgnoreExtraElementsConvention(true)
            }, t => t.FullName!.StartsWith("TheAirBlow.Stateful"));
    
    /// <summary>
    /// How long a message state is kept after it was last updated, null to keep it forever.
    /// </summary>
    public TimeSpan? Expiry { get; set; }

    /// <summary>
    /// How many of the latest message states are kept per chat.
    /// </summary>
    public int MaxStatesPerChat { get; set; }

    /// <summary>
    /// Creates a new MongoDB message state handler
    /// </summary>
    /// <param name="collection">Collection</param>
    /// <param name="expiry">How long to keep states, null to keep them forever</param>
    public MongoStateHandler(IMongoCollection<MessageState> collection, TimeSpan? expiry = null) {
        Collection = collection;
        Expiry = expiry;
    }

    /// <summary>
    /// Creates the indexes this handler needs.
    /// </summary>
    public async Task EnsureIndexesAsync() {
        var keys = Builders<MessageState>.IndexKeys;
        await Collection.Indexes.CreateOneAsync(new CreateIndexModel<MessageState>(
            keys.Ascending(x => x.ChatId).Descending(x => x.MessageId),
            new CreateIndexOptions { Unique = true, Name = "chat_message" }));
        if (Expiry == null) return;

        try {
            await Collection.Indexes.CreateOneAsync(new CreateIndexModel<MessageState>(
                keys.Ascending(x => x.LastUpdated),
                new CreateIndexOptions { ExpireAfter = Expiry, Name = "expiry" }));
        } catch (MongoCommandException e) when (e.CodeName == "IndexOptionsConflict") {
            await Collection.Database.RunCommandAsync<BsonDocument>(new BsonDocument {
                { "collMod", Collection.CollectionNamespace.CollectionName },
                { "index", new BsonDocument {
                    { "name", "expiry" },
                    { "expireAfterSeconds", (long)Expiry.Value.TotalSeconds }
                } }
            });
        }
    }

    /// <summary>
    /// Returns the stored state of a message
    /// </summary>
    /// <param name="chatId">Chat ID</param>
    /// <param name="messageId">Message ID</param>
    /// <returns>Message state, null if none</returns>
    private async Task<MessageState?> Find(long chatId, long messageId)
        => await Collection.Find(x => x.ChatId == chatId && x.MessageId == messageId).FirstOrDefaultAsync();

    /// <summary>
    /// Returns message state for message, a new unsaved one if none is stored
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>Message State</returns>
    public async Task<MessageState> GetState(Message message) {
        var state = await Find(message.Chat.Id, message.Id);
        if (state != null) { state.Expired = false; return state; }
        return new MessageState {
            LastUpdated = DateTime.UtcNow,
            MessageId = message.Id,
            ChatId = message.Chat.Id
        };
    }

    /// <summary>
    /// Returns message state for update, a new unsaved one if none is stored.
    /// Clicked messages are <see cref="MessageState.Expired"/>, other updates inherit from the previous message.
    /// </summary>
    /// <param name="update">Update</param>
    /// <returns>Message State</returns>
    public async Task<MessageState> GetState(Update update) {
        var chatId = update.GetChatId();
        var messageId = update.GetMessageId();
        if (chatId == null || messageId == null)
            return new MessageState { LastUpdated = DateTime.UtcNow };

        var state = await Find(chatId.Value, messageId.Value);
        if (state != null) { state.Expired = false; return state; }

        var newState = new MessageState {
            LastUpdated = DateTime.UtcNow, 
            MessageId = messageId.Value,
            ChatId = chatId.Value
        };

        if (update.CallbackQuery != null) {
            newState.Expired = true;
            return newState;
        }

        var prevState = await Collection.Find(x => x.ChatId == chatId && x.MessageId < messageId)
            .SortByDescending(x => x.MessageId).FirstOrDefaultAsync();
        if (prevState != null) {
            newState.State = new Dictionary<string, string>(prevState.State);
            newState.HandlerId = prevState.HandlerId;
        }

        return newState;
    }

    /// <summary>
    /// Stores message state in the database, creating it if necessary.
    /// </summary>
    /// <param name="state">Message State</param>
    /// <exception cref="StateConflictException">The stored state was modified since it was loaded</exception>
    public async Task Update(MessageState state) {
        var expected = state.Version;
        var filter = Builders<MessageState>.Filter;
        var match = filter.Eq(x => x.Version, expected);
        if (expected == 0) match |= filter.Exists(x => x.Version, false);

        state.Version = expected + 1;
        state.Expired = false;
        state.LastUpdated = DateTime.UtcNow;
        try {
            var result = await Collection.ReplaceOneAsync(
                filter.Eq(x => x.ChatId, state.ChatId) & filter.Eq(x => x.MessageId, state.MessageId) & match,
                state, new ReplaceOptions { IsUpsert = true });
            if (result is ReplaceOneResult.Acknowledged { UpsertedId: not null })
                _ = Trim(state.ChatId);
        } catch (MongoWriteException e) when (e.WriteError.Category == ServerErrorCategory.DuplicateKey) {
            state.Version = expected;
            throw new StateConflictException(state);
        }
    }

    /// <summary>
    /// Deletes everything but the latest <see cref="MaxStatesPerChat"/> states of a chat
    /// </summary>
    /// <param name="chatId">Chat ID</param>
    private async Task Trim(long chatId) {
        if (MaxStatesPerChat <= 0 || Random.Shared.Next(Math.Max(1, MaxStatesPerChat / 10)) != 0) return;
        try {
            var oldest = await Collection.Find(x => x.ChatId == chatId).SortByDescending(x => x.MessageId)
                .Skip(MaxStatesPerChat).Project(x => x.MessageId).Limit(1).ToListAsync();
            if (oldest.Count > 0)
                await Collection.DeleteManyAsync(x => x.ChatId == chatId && x.MessageId <= oldest[0]);
        } catch {
            // The state itself is stored already, the next cleanup trims again
        }
    }
}
