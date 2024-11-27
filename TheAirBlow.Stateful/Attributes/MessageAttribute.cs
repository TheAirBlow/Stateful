using Telegram.Bot.Types.Enums;

namespace TheAirBlow.Stateful.Attributes;

/// <summary>
/// Handler attribute that matches messages by text
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class MessageAttribute : HandlerAttribute {
    /// <summary>
    /// Message text
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// Hidden from auto generator
    /// </summary>
    public bool Hidden { get; }

    /// <summary>
    /// Does update message text equal to
    /// </summary>
    /// <param name="msg">Message</param>
    /// <param name="hidden">Hidden</param>
    public MessageAttribute(string? msg = null, bool hidden = false) {
        Message = msg; Hidden = hidden;
    }

    /// <summary>
    /// Checks if the condition matches for specified update handler
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <returns>True if matches</returns>
    public override Task<bool> Match(UpdateHandler handler) {
        if (handler.Update.Type != UpdateType.Message || handler.Update.Message!.Type != MessageType.Text)
            return Task.FromResult(false);
        return Task.FromResult(Message == null || handler.Update.Message!.Text == Message.TrimEnd('\n'));
    }
}