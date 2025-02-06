namespace TheAirBlow.Stateful.Conditions; 

/// <summary>
/// Handler attribute that matches submenu value
/// </summary>
public class SubMenuAttribute : MatcherAttribute {
    /// <summary>
    /// Check if submenu value equals to
    /// </summary>
    /// <param name="selector">Value</param>
    protected SubMenuAttribute(string selector) {
        Matcher = Data.Equals; Selector = selector;
    }

    /// <summary>
    /// Checks if submenu value matches selector 
    /// </summary>
    /// <param name="matcher">Matcher</param>
    /// <param name="selector">Selector</param>
    protected SubMenuAttribute(Data matcher, string selector) {
        Matcher = matcher; Selector = selector;
    }

    /// <summary>
    /// Checks if the condition matches for specified update handler
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <returns>True if matches</returns>
    public override bool Match(UpdateHandler handler) {
        var value = handler.State.SubMenu;
        return value != null && Matches(value);
    }
}