using System.Reflection;
using TheAirBlow.Stateful.Mappers;

namespace TheAirBlow.Stateful.Commands;

/// <summary>
/// Telegram text command
/// </summary>
public class Command {
    /// <summary>
    /// Command name
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Command parameters
    /// </summary>
    public string[] Parameters { get; }
    
    /// <summary>
    /// Creates a new command
    /// </summary>
    /// <param name="name">Name</param>
    /// <param name="parameters">Parameters</param>
    private Command(string name, string[] parameters) {
        Name = name; Parameters = parameters;
    }

    /// <summary>
    /// Maps command parameters to method arguments
    /// </summary>
    /// <param name="method">Method</param>
    /// <returns>Method arguments</returns>
    public object[] Map(MethodBase method) {
        var parameters = method.GetParameters();
        var nonOptional = parameters.Count(x => !x.IsOptional);
        if (parameters.Length - nonOptional > 1)
            throw new ArgumentException($"Found more that one optional argument in {method.DeclaringType?.FullName ?? "Anonymous"}.{method.Name}", nameof(method));
        if (nonOptional > Parameters.Length)
            throw new InvalidOperationException($"Expected at least {nonOptional} parameters, found {Parameters.Length}");
        
        var idx = 0; var args = new List<object>();
        for (var i = 0; i < parameters.Length; i++) {
            if (idx >= Parameters.Length) {
                args.Add(null!);
                break;
            }
            
            var param = parameters[i];
            if (i + 1 == parameters.Length && idx + 1 < Parameters.Length && param.ParameterType == typeof(string)) {
                try { args.Add(TypeMapper.Map(typeof(string), string.Join(' ', Parameters[idx..]))); }
                catch (Exception e) { throw new InvalidOperationException($"Failed to parse parameter at {idx}", e); }
                break;
            }
            
            try { args.Add(TypeMapper.Map(param.ParameterType, Parameters[idx])); }
            catch (Exception e) { throw new InvalidOperationException($"Failed to parse parameter at {idx}", e); }
            idx++;
        }

        return args.ToArray();
    }
    
    /// <summary>
    /// Parses command from update
    /// </summary>
    /// <param name="handler">Update handler</param>
    /// <returns>Command, null if none</returns>
    public static Command? Parse(UpdateHandler handler) {
        var text = handler.Update.Message?.Text;
        if (text == null || !text.StartsWith('/')) return null;
        var split = text.Split(' ');
        var cmd = split[0].Split('@');
        if (cmd.Length == 1 && handler.Update.IsPrivateChat())
            return new Command(cmd[0][1..], split[1..]);
        if (cmd.Length == 2) {
            if (cmd[1] != handler.Stateful.Bot!.Username) return null;
            return new Command(cmd[0][1..], split[1..]);
        }
        return null;
    }
}