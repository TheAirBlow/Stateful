using Telegram.Bot.Types.Enums;

namespace TheAirBlow.Stateful.Attributes;

/// <summary>
/// Handler attribute that matches messages with documents
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class DocumentAttribute : HandlerAttribute {
    /// <summary>
    /// Checks if the condition matches for specified update handler
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <returns>True if matches</returns>
    public override Task<bool> Match(UpdateHandler handler)
        => Task.FromResult(
            handler.Update.Type == UpdateType.Message &&
            handler.Update.Message!.Type == MessageType.Document);
}