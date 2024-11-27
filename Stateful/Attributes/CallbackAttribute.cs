using Telegram.Bot.Types.Enums;

namespace Stateful.Attributes;

/// <summary>
/// Handler attribute that checks for callback data
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class CallbackAttribute : HandlerAttribute {
    /// <summary>
    /// Name of the button
    /// </summary>
    public string? Name { get; }
    
    /// <summary>
    /// Callback data
    /// </summary>
    public string? Data { get; }
    
    /// <summary>
    /// Hidden from auto generator
    /// </summary>
    public bool Hidden { get; }

    /// <summary>
    /// Does callback data equal to
    /// </summary>
    /// <param name="data">Data</param>
    /// <param name="name">Name</param>
    /// <param name="hidden">Hidden</param>
    public CallbackAttribute(string? data = null, string? name = null, bool hidden = false) {
        Name = name; Data = data; Hidden = hidden;
    }

    /// <summary>
    /// Checks if the condition matches for specified update handler
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <returns>True if matches</returns>
    public override Task<bool> Match(UpdateHandler handler)
        => handler.Update.Type == UpdateType.CallbackQuery
            ? Task.FromResult(Data == null || handler.Update.CallbackQuery!.Data == Data.TrimEnd('\n'))
            : Task.FromResult(false);
}