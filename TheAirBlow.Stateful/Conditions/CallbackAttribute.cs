using System.Reflection;
using Telegram.Bot.Types.Enums;

namespace TheAirBlow.Stateful.Conditions;

/// <summary>
/// Handler attribute that checks for callback data
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class CallbackAttribute : MatcherAttribute {
    /// <summary>
    /// Name of the button
    /// </summary>
    public string? Name { get; }
    
    /// <summary>
    /// Hidden from auto generator
    /// </summary>
    public bool Hidden { get; }

    /// <summary>
    /// Checks if callback data equals to
    /// </summary>
    /// <param name="selector">Data</param>
    /// <param name="name">Name</param>
    /// <param name="hidden">Hidden</param>
    public CallbackAttribute(string? selector = null, string? name = null, bool hidden = false) {
        Matcher = Data.Equals; Name = name; Selector = selector; Hidden = hidden;
    }
    
    /// <summary>
    /// Checks if callback data matches selector
    /// </summary>
    /// <param name="matcher">Matcher type</param>
    /// <param name="selector">Selector</param>
    /// <param name="hidden">Hidden</param>
    public CallbackAttribute(Data matcher, string? selector = null, bool hidden = false) {
        Matcher = matcher; Selector = selector; Hidden = hidden;
    }

    /// <summary>
    /// Checks if the condition matches for specified update handler
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <returns>True if matches</returns>
    public override bool Match(UpdateHandler handler)
        => handler.Update.Type == UpdateType.CallbackQuery && Matches(handler.Update.CallbackQuery?.Data);
    
    /// <summary>
    /// Returns arguments to pass to specified method
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="method">Method info</param>
    /// <returns>Arguments</returns>
    public override object[]? GetArguments(UpdateHandler handler, MethodBase method)
        => GetArguments(handler, method, handler.Update.CallbackQuery?.Data);
}