using System.Reflection;

namespace TheAirBlow.Stateful.Commands;

/// <summary>
/// Command information
/// </summary>
public class CommandInfo {
    /// <summary>
    /// Array of parameters
    /// </summary>
    public Parameter[] Parameters { get; }
    
    /// <summary>
    /// Command name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Creates a new command wrapper
    /// </summary>
    /// <param name="method">Method</param>
    internal CommandInfo(MethodBase method) {
        Parameters = method.GetParameters().Select(p => new Parameter(p)).ToArray();
        Name = method.GetCustomAttribute<CommandAttribute>()!.Name;
    }
    
    /// <summary>
    /// Command parameter
    /// </summary>
    public class Parameter {
        /// <summary>
        /// Underlying type
        /// </summary>
        public Type Type { get; }
        
        /// <summary>
        /// Is this parameter required
        /// </summary>
        public bool Required { get; }
        
        /// <summary>
        /// Parameter description
        /// </summary>
        public string Name { get; }
    
        /// <summary>
        /// Parameter description
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Creates a new parameter
        /// </summary>
        /// <param name="param">Parameter</param>
        internal Parameter(ParameterInfo param) {
            var attr = param.GetCustomAttribute<ParameterAttribute>() ?? new ParameterAttribute(param.Name!);
            Type = param.ParameterType; Name = attr.Name; Description = attr.Description; Required = !param.IsOptional;
        }
    }
}