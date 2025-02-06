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
            var value = handler.State.GetState<string>(Key);
            return value != null && Matches(value);
        } catch {
            return false;
        }
    }
}