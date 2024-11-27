namespace Stateful.Attributes;

/// <summary>
/// Disables answering callbacks by default
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AnswersQueryAttribute : Attribute;