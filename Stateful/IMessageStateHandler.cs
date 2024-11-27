using Telegram.Bot.Types;

namespace Stateful;

/// <summary>
/// Message state database handler
/// </summary>
public interface IMessageStateHandler {
    /// <summary>
    /// Returns message state for message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>Message State</returns>
    public Task<MessageState> GetState(Message message);
    
    /// <summary>
    /// Returns message state for update
    /// </summary>
    /// <param name="update">Update</param>
    /// <returns>Message State</returns>
    public Task<MessageState> GetState(Update update);

    /// <summary>
    /// Updates message state in the database
    /// </summary>
    /// <param name="state">Message State</param>
    public Task Update(MessageState state);
}