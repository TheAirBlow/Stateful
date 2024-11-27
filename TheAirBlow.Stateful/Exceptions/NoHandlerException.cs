using Telegram.Bot.Types;

namespace TheAirBlow.Stateful.Exceptions;

/// <summary>
/// Exception that is thrown when Stateful fails to find a handler method
/// </summary>
public class NoHandlerException : Exception {
    /// <summary>
    /// The update that triggered the exception
    /// </summary>
    public Update Update { get; private set; }
    
    /// <summary>
    /// Exception message
    /// </summary>
    public override string Message 
        => $"No handler method for {Update.GetMessageId()} by {Update.GetUserId()}"
            + (Update.Message?.Text != null ? $", message: {Update.Message.Text}" : "")
            + (Update.CallbackQuery?.Data != null ? $", data: {Update.CallbackQuery?.Data}" : "");

    /// <summary>
    /// Creates a new handler exception
    /// </summary>
    /// <param name="update">Update</param>
    public NoHandlerException(Update update)
        => Update = update;
}