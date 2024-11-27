using JetBrains.Annotations;
using MongoDB.Driver;
using Telegram.Bot.Types;

namespace Stateful.MongoDB;

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
    /// Creates a new MongoDB message state handler
    /// </summary>
    /// <param name="collection">Collection</param>
    public MongoStateHandler(IMongoCollection<MessageState> collection)
        => Collection = collection;

    /// <summary>
    /// Returns message state for message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>Message State</returns>
    public async Task<MessageState> GetState(Message message) {
        var filter = new ExpressionFilterDefinition<MessageState>(
            x => x.ChatId == message.Chat.Id && x.MessageId == message.Id);
        using var res = await Collection.FindAsync(filter,
            new FindOptions<MessageState> { Limit = 1 });
        await res.MoveNextAsync();
        var curState = res.Current.FirstOrDefault();
        if (curState != null) return curState;
        
        return new MessageState {
            LastUpdated = DateTime.UtcNow,
            MessageId = message.Chat.Id,
            ChatId = message.Id
        };
    }

    /// <summary>
    /// Returns message state for update
    /// </summary>
    /// <param name="update">Update</param>
    /// <returns>Message State</returns>
    public async Task<MessageState> GetState(Update update) {
        var chatId = update.GetChatId();
        var messageId = update.GetMessageId();
        
        var filter = new ExpressionFilterDefinition<MessageState>(
            x => x.ChatId == chatId && x.MessageId == messageId);
        using var res = await Collection.FindAsync(filter,
            new FindOptions<MessageState> { Limit = 1 });
        await res.MoveNextAsync();
        var curState = res.Current.FirstOrDefault();
        if (curState != null) return curState;
        
        var newState = new MessageState {
            LastUpdated = DateTime.UtcNow, 
            MessageId = messageId!.Value,
            ChatId = chatId!.Value
        };

        var filter2 = new ExpressionFilterDefinition<MessageState>(
            x => x.ChatId == chatId && x.MessageId <= messageId);
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
            filter.Eq(x => x.ChatId, state.ChatId), state);
    }
}