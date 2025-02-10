using System.Reflection;
using Telegram.Bot.Types.Enums;

namespace TheAirBlow.Stateful.Conditions;

/// <summary>
/// Handler attribute that matches inline queries
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class InlineQueryAttribute : MatcherAttribute {
    /// <summary>
    /// Checks if inline query equals to
    /// </summary>
    /// <param name="selector">Data</param>
    public InlineQueryAttribute(string? selector = null) {
        Matcher = Data.Equals; Selector = selector;
    }
    
    /// <summary>
    /// Checks if inline query matches selector
    /// </summary>
    /// <param name="matcher">Matcher type</param>
    /// <param name="selector">Selector</param>
    public InlineQueryAttribute(Data matcher, string? selector = null) {
        Matcher = matcher; Selector = selector;
    }
    
    /// <summary>
    /// Checks if the condition matches for specified update handler
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <returns>True if matches</returns>
    public override bool Match(UpdateHandler handler)
        => handler.Update.Type == UpdateType.InlineQuery && Matches(handler.Update.InlineQuery?.Query);
    
    /// <summary>
    /// Returns arguments to pass to specified method
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="method">Method info</param>
    /// <returns>Arguments</returns>
    public override object[]? GetArguments(UpdateHandler handler, MethodBase method)
        => GetArguments(handler, method, handler.Update.InlineQuery?.Query);
}