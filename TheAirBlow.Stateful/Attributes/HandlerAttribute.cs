using JetBrains.Annotations;

namespace TheAirBlow.Stateful.Attributes; 

/// <summary>
/// Marks the method as an update handler and performs checks if necessary
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public abstract class HandlerAttribute : Attribute {
    /// <summary>
    /// Checks if the condition matches for specified update handler
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <returns>True if matches</returns>
    public virtual Task<bool> Match(UpdateHandler handler)
        => Task.FromResult(true);
}