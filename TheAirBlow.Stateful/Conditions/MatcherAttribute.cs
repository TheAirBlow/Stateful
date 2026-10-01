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
    /// <see cref="StatefulOptions.InternalPrefix"/> while <see cref="InternalHandler"/> is being registered,
    /// null otherwise
    /// </summary>
    [ThreadStatic]
    internal static string? InternalPrefix;
    
    /// <summary>
    /// Selector value.
    /// </summary>
    public string? Selector {
        get;
        protected init => field = InternalPrefix == null ? value : value?.Replace("{internal}",
            Matcher is Data.Regex or Data.ParsedRegex ? Regex.Escape(InternalPrefix) : InternalPrefix);
    }

    /// <summary>
    /// Selector without the trailing newlines, used by <see cref="Data.Equals"/>
    /// </summary>
    private string? _equals;

    /// <summary>
    /// The only value this matches, null if it can match more than one
    /// </summary>
    internal string? Exact => Matcher == Data.Equals ? Selector?.TrimEnd('\n') : null;

    /// <summary>
    /// Returns the selector regex
    /// </summary>
    private Regex Pattern => field ??= new Regex(Selector!, RegexOptions.CultureInvariant);

    /// <summary>
    /// Checks if the condition matches for specified value
    /// </summary>
    /// <param name="value">String value</param>
    /// <returns>True if matches</returns>
    protected bool Matches(string? value)
        => Selector == null || (value != null && Matcher switch {
            Data.Equals => value == (_equals ??= Selector.TrimEnd('\n')),
            Data.StartsWith => value.StartsWith(Selector, StringComparison.Ordinal),
            Data.EndsWith => value.EndsWith(Selector, StringComparison.Ordinal),
            Data.Contains => value.Contains(Selector, StringComparison.Ordinal),
            Data.Regex => Pattern.IsMatch(value),
            Data.ParsedRegex => Pattern.IsMatch(value),
            _ => false
        });

    /// <summary>
    /// Returns arguments to pass to specified method
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="method">Method info</param>
    /// <param name="value">String value</param>
    /// <returns>Arguments</returns>
    protected object[]? GetArguments(UpdateHandler handler, MethodBase method, string? value) {
        if (value == null || Selector == null || Matcher != Data.ParsedRegex) return null;
        var match = Pattern.Match(value);
        if (match.Groups.Count - 1 != method.GetParameters().Length)
            throw new InvalidDataException($"Method {method.DeclaringType?.FullName ?? "Anonymous"}.{method.Name} was expected to have {match.Groups.Count - 1} arguments but found {method.GetParameters().Length} instead");
        return match.Groups.Values.Skip(1).Select(x => x.Value).Zip(method.GetParameters(), (a, b) => TypeMapper.Map(b.ParameterType, a)).ToArray();
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