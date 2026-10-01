namespace TheAirBlow.Stateful.Mappers;

/// <summary>
/// Custom type mapper implementation for target type
/// </summary>
public abstract class CustomTypeMapper {
    /// <summary>
    /// An array of types this mapper can map
    /// </summary>
    public abstract Type[] Types { get; }

    /// <summary>
    /// Maps string to target type
    /// </summary>
    /// <param name="target"></param>
    /// <param name="value">String value</param>
    /// <returns>Parsed type</returns>
    public abstract object Map(Type target, string value);
}