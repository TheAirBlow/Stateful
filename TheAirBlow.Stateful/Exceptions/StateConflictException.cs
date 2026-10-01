namespace TheAirBlow.Stateful.Exceptions;

/// <summary>
/// Thrown by a state handler when a message state was modified by someone else
/// since it was loaded. Load the state again and retry the change.
/// </summary>
public class StateConflictException : Exception {
    /// <summary>
    /// Creates a new state conflict exception
    /// </summary>
    /// <param name="state">State that failed to save</param>
    public StateConflictException(MessageState state)
        : base($"State of message {state.MessageId} in chat {state.ChatId} was modified concurrently (version {state.Version})") { }
}
