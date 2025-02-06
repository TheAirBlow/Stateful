using System.Reflection;
using System.Text.RegularExpressions;
using Telegram.Bot.Types.Enums;
using TheAirBlow.Stateful.Mappers;

namespace TheAirBlow.Stateful.Conditions;

/// <summary>
/// Handler attribute that matches arbitrary value by selector
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public abstract class MatcherAttribute : HandlerAttribute {
    /// <summary>
    /// Matcher type
    /// </summary>
    public Data Matcher { get; protected init; }
    
    /// <summary>
    /// Selector value
    /// </summary>
    public string? Selector { get; protected init; }

    /// <summary>
    /// Checks if the condition matches for specified value
    /// </summary>
    /// <param name="value">String value</param>
    /// <returns>True if matches</returns>
    protected bool Matches(string value)
        => Selector == null || Matcher switch {
            Data.Equals => value == Selector.TrimEnd('\n'),
            Data.StartsWith => value.StartsWith(Selector),
            Data.EndsWith => value.EndsWith(Selector),
            Data.Contains => value.Contains(Selector),
            Data.Regex => Regex.IsMatch(value, Selector),
            Data.ParsedRegex => Regex.IsMatch(value, Selector),
            _ => false
        };
    
    /// <summary>
    /// Returns arguments to pass to specified method
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="method">Method info</param>
    /// <returns>Arguments</returns>
    public override object[]? GetArguments(UpdateHandler handler, MethodBase method) {
        if (Matcher != Data.ParsedRegex) return null;
        var match = Regex.Match(handler.Update.Message!.Text!, Selector!);
        if (match.Groups.Count - 1 != method.GetParameters().Length)
            throw new InvalidDataException($"Method {method.DeclaringType?.FullName ?? "Anonymous"}.{method.Name} was expected to have {match.Groups.Count - 1} arguments but found {method.GetParameters().Length} instead");
        return match.Groups.Values.Select(x => x.Value).Zip(method.GetParameters(), (a, b) => TypeMapper.Map(b.ParameterType, a)).ToArray();
    }
}

/// <summary>
/// Matcher type
/// </summary>
public enum Data {
    /// <summary>
    /// Checks if target data starts with selector
    /// </summary>
    StartsWith,
    
    /// <summary>
    /// Checks if target data ends with selector
    /// </summary>
    EndsWith,
    
    /// <summary>
    /// Checks if target data contains selector
    /// </summary>
    Contains,
    
    /// <summary>
    /// Checks if target data equals to selector
    /// </summary>
    Equals,
    
    /// <summary>
    /// Checks if target data matches selector regex
    /// </summary>
    Regex,
    
    /// <summary>
    /// Checks if target data matches selector regex.
    /// Will provide group values as arguments to your method.
    /// </summary>
    ParsedRegex
}