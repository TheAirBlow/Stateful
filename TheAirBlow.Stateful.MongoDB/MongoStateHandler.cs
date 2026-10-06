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
    /// How many of the latest message states are always kept per chat, even if they are past <see cref="Expiry"/>.
    /// </summary>
    public int MinStatesPerChat { get; set; }

    /// <summary>
    /// How many of the latest message states are kept per chat at most, 0 for no limit.
    /// Takes priority over <see cref="MinStatesPerChat"/>.
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

        await DropIndex("expiry");
        if (Expiry == null) {
            await DropIndex("expires_at");
            return;
        }

        try {
            await Collection.Indexes.CreateOneAsync(new CreateIndexModel<MessageState>(
                keys.Ascending(x => x.ExpiresAt),
                new CreateIndexOptions { ExpireAfter = TimeSpan.Zero, Name = "expires_at" }));
        } catch (MongoCommandException e) when (e.CodeName == "IndexOptionsConflict") {
            await DropIndex("expires_at");
            await EnsureIndexesAsync();
        }
    }

    /// <summary>
    /// Drops an index
    /// </summary>
    /// <param name="name">Index name</param>
    private async Task DropIndex(string name) {
        try {
            await Collection.Indexes.DropOneAsync(name);
        } catch (MongoCommandException e) when (e.CodeName is "IndexNotFound" or "NamespaceNotFound") {
            // Nothing to drop
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

        if (update.CallbackQuery != null)
            return newState;

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
        state.ExpiresAt = await IsProtected(state) ? null : state.LastUpdated + Expiry;
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
    /// Is the state among the latest <see cref="MinStatesPerChat"/> of its chat, so it must not expire
    /// </summary>
    /// <param name="state">Message state</param>
    /// <returns>True if protected</returns>
    private async Task<bool> IsProtected(MessageState state) {
        if (Expiry == null || MinStatesPerChat <= 0) return Expiry == null;
        var oldestKept = await Nth(state.ChatId, MinStatesPerChat);
        return oldestKept == null || state.MessageId >= oldestKept;
    }

    /// <summary>
    /// Deletes everything but the latest <see cref="MaxStatesPerChat"/> states of a chat, and lets the state
    /// that just left the latest <see cref="MinStatesPerChat"/> ones expire
    /// </summary>
    /// <param name="chatId">Chat ID</param>
    private async Task Trim(long chatId) {
        try {
            var filter = Builders<MessageState>.Filter;
            var chat = filter.Eq(x => x.ChatId, chatId);

            if (MaxStatesPerChat > 0) {
                var oldest = await Nth(chatId, MaxStatesPerChat + 1);
                if (oldest != null) await Collection.DeleteManyAsync(chat & filter.Lte(x => x.MessageId, oldest.Value));
            }

            if (Expiry == null || MinStatesPerChat <= 0) return;
            var leaving = await Nth(chatId, MinStatesPerChat + 1);
            if (leaving == null) return;
            await Collection.UpdateManyAsync(
                chat & filter.Lte(x => x.MessageId, leaving.Value) & filter.Eq(x => x.ExpiresAt, null),
                Builders<MessageState>.Update.Pipeline(
                    new EmptyPipelineDefinition<MessageState>().AppendStage<MessageState, MessageState, MessageState>(
                        new BsonDocument("$set", new BsonDocument("ExpiresAt",
                            new BsonDocument("$add", new BsonArray { "$LastUpdated", (long)Expiry.Value.TotalMilliseconds })))))
                );
        } catch {
            // The state itself is stored already, the next cleanup trims again
        }
    }

    /// <summary>
    /// Returns the ID of the n-th latest message state of a chat
    /// </summary>
    private async Task<long?> Nth(long chatId, int n) {
        var found = await Collection.Find(x => x.ChatId == chatId).SortByDescending(x => x.MessageId)
            .Skip(n - 1).Limit(1).Project(x => x.MessageId).ToListAsync();
        return found.Count > 0 ? found[0] : null;
    }
}
