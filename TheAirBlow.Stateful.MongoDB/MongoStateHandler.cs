using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Telegram.Bot.Types;

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
    /// Creates a new MongoDB message state handler
    /// </summary>
    /// <param name="collection">Collection</param>
    public MongoStateHandler(IMongoCollection<MessageState> collection) {
        Collection = collection;
    }

    /// <summary>
    /// Returns message state for message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>Message State</returns>
    public async Task<MessageState> GetState(Message message) {
        var chatId = message.Chat.Id;
        var userId = message.From?.Id;
        if (userId == null)
            return MessageState.None;
        
        var filter = new ExpressionFilterDefinition<MessageState>(
            x => x.UserId == userId && x.ChatId == chatId && x.MessageId == message.Id);
        using var res = await Collection.FindAsync(filter,
            new FindOptions<MessageState> { Limit = 1 });
        await res.MoveNextAsync();
        var curState = res.Current.FirstOrDefault();
        if (curState != null) return curState;
        var newState = new MessageState {
            LastUpdated = DateTime.UtcNow,
            MessageId = message.Id,
            UserId = userId.Value,
            ChatId = chatId
        };
        
        await Collection.InsertOneAsync(newState);
        return newState;
    }

    /// <summary>
    /// Returns message state for update
    /// </summary>
    /// <param name="update">Update</param>
    /// <returns>Message State</returns>
    public async Task<MessageState> GetState(Update update) {
        var userId = update.GetUserId();
        if (userId == null)
            return MessageState.None;
        
        var chatId = update.GetChatId();
        var messageId = update.GetMessageId();
        
        var filter = new ExpressionFilterDefinition<MessageState>(
            x => x.UserId == userId && x.ChatId == chatId && x.MessageId == messageId);
        using var res = await Collection.FindAsync(filter,
            new FindOptions<MessageState> { Limit = 1 });
        await res.MoveNextAsync();
        var curState = res.Current.FirstOrDefault();
        if (curState != null) return curState;
        
        var newState = new MessageState {
            LastUpdated = DateTime.UtcNow, 
            MessageId = messageId!.Value,
            UserId = userId.Value,
            ChatId = chatId!.Value
        };

        var filter2 = new ExpressionFilterDefinition<MessageState>(
            x => x.UserId == userId && x.ChatId == chatId && x.MessageId <= messageId);
        var sort = Builders<MessageState>.Sort.Descending(x => x.MessageId);
        using var res2 = await Collection.FindAsync(filter2,
            new FindOptions<MessageState> { Limit = 1, Sort = sort });
        await res2.MoveNextAsync();
        var prevState = res2.Current.FirstOrDefault();
        if (prevState != null) {
            newState.State = prevState.State.ToDictionary(x => x.Key, x => x.Value);
            newState.HandlerId = prevState.HandlerId;
        }
        
        await Collection.InsertOneAsync(newState);
        return newState;
    }

    /// <summary>
    /// Updates message state in the database
    /// </summary>
    /// <param name="state">Message State</param>
    public async Task Update(MessageState state) {
        var filter = Builders<MessageState>.Filter;
        await Collection.FindOneAndReplaceAsync(
            filter.Eq(x => x.MessageId, state.MessageId) & 
            filter.Eq(x => x.UserId, state.UserId) & 
            filter.Eq(x => x.ChatId, state.ChatId), state);
    }
}