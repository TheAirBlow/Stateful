namespace TheAirBlow.Stateful.Exceptions;

/// <summary>
/// Thrown by <see cref="UpdateHandler.Yield"/> to let the next applicable method take over
/// </summary>
public class YieldException : Exception;
