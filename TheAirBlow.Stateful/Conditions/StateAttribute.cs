using System.Reflection;

namespace TheAirBlow.Stateful.Conditions; 

/// <summary>
/// Handler attribute that matches state values
/// </summary>
public class StateAttribute : MatcherAttribute {
    /// <summary>
    /// State key
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Checks if state value by key equals to
    /// </summary>
    /// <param name="key">State Key</param>
    /// <param name="selector">Value</param>
    protected StateAttribute(string key, string selector) {
        Matcher = Data.Equals; Key = key; Selector = selector;
    }

    /// <summary>
    /// Checks if state value by key matches selector 
    /// </summary>
    /// <param name="matcher">Matcher</param>
    /// <param name="key">State Key</param>
    /// <param name="selector">Selector</param>
    protected StateAttribute(Data matcher, string key, string selector) {
        Matcher = matcher; Key = key; Selector = selector;
    }

    /// <summary>
    /// Checks if the condition matches for specified update handler
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <returns>True if matches</returns>
    public override bool Match(UpdateHandler handler) {
        try {
            return Matches(handler.State.GetState<string>(Key));
        } catch {
            return false;
        }
    }

    /// <summary>
    /// Returns arguments to pass to specified method
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="method">Method info</param>
    /// <returns>Arguments</returns>
    public override object[]? GetArguments(UpdateHandler handler, MethodBase method) {
        try {
            return GetArguments(handler, method, handler.State.GetState<string>(Key));
        } catch {
            return null;
        }
    }
}