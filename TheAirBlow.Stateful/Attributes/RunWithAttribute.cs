namespace TheAirBlow.Stateful.Attributes;

/// <summary>
/// Overrides threading type (methods) or changes the default (classes)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RunWithAttribute : Attribute {
    /// <summary>
    /// Threading value
    /// </summary>
    public Threading Threading { get; set; }

    /// <summary>
    /// Creates a new private only attribute
    /// </summary>
    public RunWithAttribute(Threading threading)
        => Threading = threading;
}