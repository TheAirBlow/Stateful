using JetBrains.Annotations;

namespace TheAirBlow.Stateful.Attributes;

/// <summary>
/// Marks method as the default update handler of a class
/// </summary>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public class DefaultHandlerAttribute : Attribute;