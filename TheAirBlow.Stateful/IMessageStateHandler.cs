using Telegram.Bot.Types;

namespace TheAirBlow.Stateful;

/// <summary>
/// Message state database handler
/// </summary>
public interface IMessageStateHandler {
    /// <summary>
    /// Returns message state for message, a new unsaved one if none is stored
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>Message State</returns>
    public Task<MessageState> GetState(Message message);
    
    /// <summary>
    /// Returns message state for update, a new unsaved one if none is stored.
    /// Clicked messages without state are <see cref="MessageState.Expired"/>, other updates inherit from the previous message.
    /// </summary>
    /// <param name="update">Update</param>
    /// <returns>Message State</returns>
    public Task<MessageState> GetState(Update update);

    /// <summary>
    /// Stores message state in the database, creating it if necessary.
    /// Throws <see cref="Exceptions.StateConflictException"/> if it was modified since it was loaded.
    /// </summary>
    /// <param name="state">Message State</param>
    public Task Update(MessageState state);
}