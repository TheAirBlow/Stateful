namespace TheAirBlow.Stateful.Commands;

/// <summary>
/// Command parameter attribute
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class ParameterAttribute : Attribute {
    /// <summary>
    /// Parameter description
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Parameter description
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Creates a new parameter attribute
    /// </summary>
    /// <param name="name">Name</param>
    /// <param name="description">Description</param>
    public ParameterAttribute(string name, string? description = null) {
        Name = name; Description = description;
    }
}