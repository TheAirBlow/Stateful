using System.Collections;
using System.Collections.Concurrent;

namespace TheAirBlow.Stateful.Mappers;

/// <summary>
/// Global type mapper
/// </summary>
public static class TypeMapper {
    /// <summary>
    /// Dictionary of types and their mapper
    /// </summary>
    private static readonly ConcurrentDictionary<Type, CustomTypeMapper> _mappers = new();

    /// <summary>
    /// Registers default type mappers
    /// </summary>
    static TypeMapper() => Register<CoreTypeMapper>();
    
    /// <summary>
    /// Registers a type mapper
    /// </summary>
    public static void Register<T>() {
        var mapper = (CoreTypeMapper)Activator.CreateInstance(typeof(T))!;
        foreach (var type in mapper.Types) {
            if (_mappers.TryGetValue(type, out var conflict))
                throw new ArgumentException($"Mapper conflicts with {conflict.GetType().FullName} because they both map {type.FullName}", nameof(mapper));
            _mappers[type] = mapper;
        }
    }
    
    /// <summary>
    /// Maps string value to target type
    /// </summary>
    /// <param name="type">Target type</param>
    /// <param name="value">String value</param>
    /// <returns>Mapped value</returns>
    public static object Map(Type type, string value) {
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (type == typeof(string)) return value;
        if (_mappers.TryGetValue(type, out var mapper)) return mapper.Map(type, value);
        throw new InvalidOperationException($"No mapper registered for type {type.FullName}");
    }
}