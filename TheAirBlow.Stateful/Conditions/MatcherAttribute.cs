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
    /// Selector with the placeholder resolved for the prefix it was last resolved with
    /// </summary>
    private (string? Prefix, string Selector, Regex? Regex) _resolved;

    /// <summary>
    /// The only value this matches, null if it can match more than one
    /// </summary>
    internal string? Exact => Matcher == Data.Equals ? Selector?.TrimEnd('\n') : null;

    /// <summary>
    /// Returns the selector with <c>{internal}</c> replaced by the handler's internal prefix
    /// </summary>
    private (string Selector, Regex? Regex) Resolve(UpdateHandler handler) {
        var prefix = handler.Stateful.Options.InternalPrefix;
        var cached = _resolved;
        if (cached.Prefix == prefix) return (cached.Selector, cached.Regex);
        var isRegex = Matcher is Data.Regex or Data.ParsedRegex;
        var selector = Selector!.Replace("{internal}", isRegex ? Regex.Escape(prefix) : prefix);
        var regex = isRegex ? new Regex(selector, RegexOptions.CultureInvariant) : null;
        _resolved = (prefix, selector, regex);
        return (selector, regex);
    }

    /// <summary>
    /// Checks if the condition matches for specified value
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="value">String value</param>
    /// <returns>True if matches</returns>
    protected bool Matches(UpdateHandler handler, string? value) {
        if (Selector == null) return true;
        if (value == null) return false;
        var (selector, regex) = Resolve(handler);
        return Matcher switch {
            Data.Equals => value == selector.TrimEnd('\n'),
            Data.StartsWith => value.StartsWith(selector, StringComparison.Ordinal),
            Data.EndsWith => value.EndsWith(selector, StringComparison.Ordinal),
            Data.Contains => value.Contains(selector, StringComparison.Ordinal),
            Data.Regex or Data.ParsedRegex => regex!.IsMatch(value),
            _ => false
        };
    }

    /// <summary>
    /// Returns arguments to pass to specified method
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="method">Method info</param>
    /// <param name="value">String value</param>
    /// <returns>Arguments</returns>
    protected object[]? GetArguments(UpdateHandler handler, MethodBase method, string? value) {
        if (value == null || Selector == null || Matcher != Data.ParsedRegex) return null;
        var match = Resolve(handler).Regex!.Match(value);
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