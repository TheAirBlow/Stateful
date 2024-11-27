namespace Stateful.Attributes;

/// <summary>
/// Marks method as the default update handler of a class
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class DefaultHandlerAttribute : Attribute;