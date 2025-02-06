namespace TheAirBlow.Stateful.Mappers;

/// <summary>
/// Custom type mapper implementation for target type
/// </summary>
public abstract class CustomTypeMapper {
    /// <summary>
    /// An array of types this mapper can map
    /// </summary>
    public virtual Type[] Types => throw new NotImplementedException();

    /// <summary>
    /// Maps string to target type
    /// </summary>
    /// <param name="target"></param>
    /// <param name="value">String value</param>
    /// <returns>Parsed type</returns>
    public virtual object Map(Type target, string value)
        => throw new NotImplementedException();
}