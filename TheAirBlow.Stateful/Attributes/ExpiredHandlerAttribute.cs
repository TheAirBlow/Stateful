namespace TheAirBlow.Stateful.Attributes;

/// <summary>
/// Marks method as the handler for callback queries from messages whose state no longer exists (expired).
/// Conditions can be added to limit which expired callbacks it handles. Only works if the
/// <see cref="IMessageStateHandler"/> reports expired messages by setting <see cref="MessageState.Expired"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ExpiredHandlerAttribute : Attribute;
