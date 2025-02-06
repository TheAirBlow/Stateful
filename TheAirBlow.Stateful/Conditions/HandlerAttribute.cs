using System.Reflection;
using JetBrains.Annotations;

namespace TheAirBlow.Stateful.Conditions; 

/// <summary>
/// Marks the method as an update handler and checks if specified condition matches
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public abstract class HandlerAttribute : Attribute {
    /// <summary>
    /// Checks if the condition matches for specified update handler.
    /// This is a wrapper for <see cref="Match"/> by default.
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <returns>True if matches</returns>
    public virtual Task<bool> MatchAsync(UpdateHandler handler) => Task.FromResult(Match(handler));
    
    /// <summary>
    /// Checks if the condition matches for specified update handler.
    /// Do not override <see cref="MatchAsync"/> for it to work.
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <returns>True if matches</returns>
    public virtual bool Match(UpdateHandler handler) => true;

    /// <summary>
    /// Returns arguments to pass to specified method
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="method">Method info</param>
    /// <returns>Arguments, none if not necessary</returns>
    public virtual object[]? GetArguments(UpdateHandler handler, MethodBase method) => null;
}