using System.Globalization;

namespace TheAirBlow.Stateful.Mappers;

/// <summary>
/// Mapper for core CLR types
/// </summary>
public class CoreTypeMapper : CustomTypeMapper {
    /// <summary>
    /// An array of types this mapper can map
    /// </summary>
    public override Type[] Types => [ 
        typeof(bool), typeof(byte), typeof(sbyte), typeof(decimal),
        typeof(double), typeof(float), typeof(int), typeof(uint),
        typeof(long), typeof(ulong), typeof(short), typeof(ushort),
        typeof(char), typeof(Guid), typeof(DateTime), typeof(DateTimeOffset),
        typeof(TimeSpan)
    ];

    /// <summary>
    /// Maps string to target type
    /// </summary>
    /// <param name="target">Target type</param>
    /// <param name="value">String value</param>
    /// <returns>Parsed type</returns>
    public override object Map(Type target, string value) {
        var culture = CultureInfo.InvariantCulture;
        if (target == typeof(Guid)) return Guid.Parse(value);
        if (target == typeof(DateTimeOffset)) return DateTimeOffset.Parse(value, culture);
        if (target == typeof(TimeSpan)) return TimeSpan.Parse(value, culture);
        switch (Type.GetTypeCode(target)) {
            case TypeCode.Boolean:
                return value.ToLowerInvariant() switch {
                    "true" or "1" => true,
                    "false" or "0" => false,
                    _ => throw new FormatException($"\"{value}\" is not a valid boolean")
                };
            case TypeCode.Char:
                return char.Parse(value);
            case TypeCode.DateTime:
                return DateTime.Parse(value, culture);
            case TypeCode.Byte:
                return byte.Parse(value, culture);
            case TypeCode.SByte:
                return sbyte.Parse(value, culture);
            case TypeCode.Decimal:
                return decimal.Parse(value, culture);
            case TypeCode.Double:
                return double.Parse(value, culture);
            case TypeCode.Single:
                return float.Parse(value, culture);
            case TypeCode.Int16:
                return short.Parse(value, culture);
            case TypeCode.Int32:
                return int.Parse(value, culture);
            case TypeCode.Int64:
                return long.Parse(value, culture);
            case TypeCode.UInt16:
                return ushort.Parse(value, culture);
            case TypeCode.UInt32:
                return uint.Parse(value, culture);
            case TypeCode.UInt64:
                return ulong.Parse(value, culture);
            default:
                throw new ArgumentException($"{target.FullName} is not supported", nameof(target));
        }
    }
}
