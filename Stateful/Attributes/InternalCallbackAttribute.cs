namespace Stateful.Attributes;

/// <summary>
/// Internal handler attribute that checks for callback data
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
internal class InternalCallbackAttribute : HandlerAttribute {
    /// <summary>
    /// Callback data
    /// </summary>
    public string Data { get; }
    
    /// <summary>
    /// Checks if the condition matches for specified update handler
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <returns>True if matches</returns>
    public override Task<bool> Match(UpdateHandler handler)
        => Task.FromResult(handler.Update.CallbackQuery?.Data?.StartsWith($"stinternal-{Data}") ?? false);

    /// <summary>
    /// Does callback data equal to
    /// </summary>
    /// <param name="data">Data</param>
    public InternalCallbackAttribute(string data)
        => Data = data;
}