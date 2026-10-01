namespace TheAirBlow.Stateful.Commands;

/// <summary>
/// Thrown when command arguments can't be mapped to the method's parameters
/// </summary>
internal class CommandArgumentException : Exception {
    /// <summary>
    /// Command the arguments were meant for
    /// </summary>
    public CommandInfo Command { get; }

    /// <summary>
    /// Creates a new command argument exception
    /// </summary>
    /// <param name="command">Command</param>
    /// <param name="inner">Mapping error</param>
    public CommandArgumentException(CommandInfo command, Exception inner) : base(inner.Message, inner)
        => Command = command;
}
