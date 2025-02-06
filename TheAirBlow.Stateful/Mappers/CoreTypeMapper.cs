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
        typeof(long), typeof(ulong), typeof(short), typeof(ushort)
    ];

    /// <summary>
    /// Maps string to target type
    /// </summary>
    /// <param name="target">Target type</param>
    /// <param name="value">String value</param>
    /// <returns>Parsed type</returns>
    public override object Map(Type target, string value) {
        switch (Type.GetTypeCode(target)) {
            case TypeCode.Boolean:
                return value is "true" or "1";
            case TypeCode.Byte:
                return byte.Parse(value);
            case TypeCode.SByte:
                return sbyte.Parse(value);
            case TypeCode.Decimal:
                return decimal.Parse(value);
            case TypeCode.Double:
                return double.Parse(value);
            case TypeCode.Single:
                return float.Parse(value);
            case TypeCode.Int16:
                return short.Parse(value);
            case TypeCode.Int32:
                return int.Parse(value);
            case TypeCode.Int64:
                return long.Parse(value);
            case TypeCode.UInt16:
                return ushort.Parse(value);
            case TypeCode.UInt32:
                return uint.Parse(value);
            case TypeCode.UInt64:
                return ulong.Parse(value);
            default:
                throw new ArgumentException($"{target.FullName} is not supported", nameof(target));
        }
    }
}