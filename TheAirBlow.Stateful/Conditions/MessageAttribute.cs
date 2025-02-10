using System.Reflection;
using Telegram.Bot.Types.Enums;

namespace TheAirBlow.Stateful.Conditions;

/// <summary>
/// Handler attribute that matches messages by text
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class MessageAttribute : MatcherAttribute {
    /// <summary>
    /// Message type
    /// </summary>
    public MessageType Type { get; }
    
    /// <summary>
    /// Hidden from auto generator
    /// </summary>
    public bool Hidden { get; }

    /// <summary>
    /// Checks if message is equals to selector
    /// </summary>
    /// <param name="selector">Selector</param>
    /// <param name="hidden">Hidden</param>
    public MessageAttribute(string? selector = null, bool hidden = false) {
        Type = MessageType.Text; Matcher = Data.Equals; Selector = selector; Hidden = hidden;
    }

    /// <summary>
    /// Checks if message matches selector
    /// </summary>
    /// <param name="matcher">Matcher type</param>
    /// <param name="selector">Selector</param>
    /// <param name="hidden">Hidden</param>
    public MessageAttribute(Data matcher, string? selector = null, bool hidden = false) {
        Type = MessageType.Text; Matcher = matcher; Selector = selector; Hidden = hidden;
    }
    
    /// <summary>
    /// Checks if message is of type
    /// </summary>
    /// <param name="type">Message type</param>
    /// <param name="hidden">Hidden</param>
    public MessageAttribute(MessageType type, bool hidden = false) {
        Type = type; Hidden = hidden;
    }

    /// <summary>
    /// Checks if the condition matches for specified update handler
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <returns>True if matches</returns>
    public override bool Match(UpdateHandler handler)
        => handler.Update.Type == UpdateType.Message && handler.Update.Message!.Type == Type &&
           (Type != MessageType.Text || Matches(handler.Update.Message?.Text));
    
    /// <summary>
    /// Returns arguments to pass to specified method
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="method">Method info</param>
    /// <returns>Arguments</returns>
    public override object[]? GetArguments(UpdateHandler handler, MethodBase method)
        => GetArguments(handler, method, handler.Update.Message?.Text);
}