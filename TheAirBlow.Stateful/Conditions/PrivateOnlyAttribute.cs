namespace TheAirBlow.Stateful.Conditions;

/// <summary>
/// Marks handler class or method as private chat only
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class PrivateOnlyAttribute : HandlerAttribute {
    /// <summary>
    /// Are only private 
    /// </summary>
    public bool PrivateOnly { get; set; }
    
    /// <summary>
    /// Creates a new private only attribute
    /// </summary>
    /// <param name="privateOnly">Private only</param>
    public PrivateOnlyAttribute(bool privateOnly = true)
        => PrivateOnly = privateOnly;
    
    /// <summary>
    /// Checks if the condition matches for specified update handler
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <returns>True if matches</returns>
    public override bool Match(UpdateHandler handler)
        => PrivateOnly ? handler.Update.IsPrivateChat() : !handler.Update.IsPrivateChat();
}