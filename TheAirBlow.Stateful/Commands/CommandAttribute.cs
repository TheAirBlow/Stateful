using System.Reflection;
using TheAirBlow.Stateful.Conditions;
using TheAirBlow.Stateful.Exceptions;

namespace TheAirBlow.Stateful.Commands;

/// <summary>
/// Marks method as a telegram bot command
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : HandlerAttribute {
    /// <summary>
    /// Command name
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Should arguments get automatically mapped
    /// </summary>
    public bool MapArguments { get; }

    /// <summary>
    /// Creates a new command attribute
    /// </summary>
    /// <param name="name">Name</param>
    /// <param name="map">Map arguments</param>
    public CommandAttribute(string name, bool map = true) {
        Name = name; MapArguments = map;
    }

    /// <summary>
    /// Checks if the condition matches for specified update handler
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <returns>True if matches</returns>
    public override bool Match(UpdateHandler handler) {
        var command = Command.Parse(handler);
        return command != null && command.Name == Name;
    }
    
    /// <summary>
    /// Returns arguments to pass to specified method
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="method">Method info</param>
    /// <returns>Arguments</returns>
    public override object[]? GetArguments(UpdateHandler handler, MethodBase method) {
        if (!MapArguments) return null;
        var command = Command.Parse(handler)!;
        try {
            return command.Map(method);
        } catch (Exception e) {
            // TODO: this is fucking aids, redo this later
            var info = new CommandInfo(method);
            handler.Stateful.Options.CommandErrorHandler?.Invoke(handler, e, info).GetAwaiter().GetResult();
            throw new SilentException();
        }
    }
}