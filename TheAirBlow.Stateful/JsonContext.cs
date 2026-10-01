using System.Text.Json.Serialization;
using TheAirBlow.Stateful.Keyboards;

namespace TheAirBlow.Stateful;

/// <summary>
/// JSON serialization of the values Stateful stores in message states
/// </summary>
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Keyboard.PaginatorData))]
internal partial class StatefulJsonContext : JsonSerializerContext;
