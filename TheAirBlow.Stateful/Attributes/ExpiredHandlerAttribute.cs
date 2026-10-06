using JetBrains.Annotations;

namespace TheAirBlow.Stateful.Attributes;

/// <summary>
/// Marks method as the handler for callback queries from messages whose state no longer exists.
/// </summary>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public class ExpiredHandlerAttribute : Attribute;
