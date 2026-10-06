using System.Reflection;
using JetBrains.Annotations;
using Telegram.Bot.Types.Enums;

namespace TheAirBlow.Stateful.Conditions; 

/// <summary>
/// Marks the method as an update handler and checks if specified condition matches
/// </summary>
[PublicAPI, MeansImplicitUse]
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
    /// The only type of update this condition can match, null if any
    /// </summary>
    public virtual UpdateType? Updates => null;

    /// <summary>
    /// The only message text, callback data or query this condition can match, null if more than one
    /// </summary>
    public virtual string? ExactValue => null;

    /// <summary>
    /// Does this condition use <see cref="UpdateHandler.State"/>. Filters that don't are checked before it is loaded.
    /// </summary>
    public virtual bool RequiresState => true;

    /// <summary>
    /// Returns arguments to pass to specified method
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="method">Method info</param>
    /// <returns>Arguments, none if not necessary</returns>
    public virtual object[]? GetArguments(UpdateHandler handler, MethodBase method) => null;
}