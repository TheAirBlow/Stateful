using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TheAirBlow.Stateful.Commands;
using TheAirBlow.Stateful.Conditions;
using TheAirBlow.Stateful.Keyboards;

namespace TheAirBlow.Stateful;

/// <summary>
/// Stateful logic implementation
/// </summary>
public partial class StatefulHandler {
    /// <summary>
    /// Returns the caller's handler, or all handlers that could handle the next update the same way dispatching picks them
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="allHandlers">Include all available handlers, not just the caller's</param>
    /// <returns>Handler wrappers</returns>
    private IEnumerable<HandlerWrapper> Available(UpdateHandler handler, bool allHandlers) {
        var own = Handlers.FirstOrDefault(x => x.Handler == handler.GetType());
        if (own == null) throw new InvalidOperationException("This method can only be used inside a registered handler");
        return allHandlers ? Handlers.Where(x => x.IsAvailable(handler.State) && x.Conditions.Match(handler)) : [own];
    }

    /// <summary>
    /// Generates a reply keyboard for the caller's handler, or all handlers available for the current state
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="allHandlers">Include all available handlers, not just the caller's</param>
    /// <returns>Generated reply keyboard</returns>
    internal ReplyKeyboardMarkup GenerateReply(UpdateHandler handler, bool allHandlers) {
        var list = new List<string>();
        foreach (var method in Available(handler, allHandlers).SelectMany(x => x.Methods)) {
            var msg = (MessageAttribute?)method.Conditions.FirstOrDefault(x => x is MessageAttribute);
            if (msg == null || msg.Hidden || msg.Selector == null || msg.Matcher != Data.Equals || msg.Type != MessageType.Text) continue;
            if (!method.Conditions.Where(x => x is not MessageAttribute).ToArray().Match(handler)) continue;
            if (!list.Contains(msg.Selector)) list.Add(msg.Selector);
        }
        return Keyboard.Reply(list.ToArray());
    }

    /// <summary>
    /// Generates an inline keyboard for the caller's handler, or all handlers available for the current state
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="allHandlers">Include all available handlers, not just the caller's</param>
    /// <returns>Generated inline keyboard</returns>
    internal InlineKeyboardMarkup GenerateInline(UpdateHandler handler, bool allHandlers) {
        var dict = new Dictionary<string, string>();
        foreach (var method in Available(handler, allHandlers).SelectMany(x => x.Methods)) {
            var call = (CallbackAttribute?)method.Conditions.FirstOrDefault(x => x is CallbackAttribute);
            if (call == null || call.Hidden || call.Selector == null || call.Matcher != Data.Equals) continue;
            if (!method.Conditions.Where(x => x is not CallbackAttribute).ToArray().Match(handler)) continue;
            var name = call.Name ?? call.Selector;
            if (!dict.TryAdd(name, call.Selector.TrimEnd('\n')))
                throw new InvalidOperationException($"Duplicate inline button name \"{name.TrimEnd('\n')}\" in {method.Method.DeclaringType?.FullName}");
        }
        return Keyboard.Inline(dict);
    }

    /// <summary>
    /// Returns a list of commands from the caller's handler, or all handlers available for the current state
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="all">Include commands whose other conditions (such as private only) don't match the current update</param>
    /// <param name="allHandlers">Include all available handlers, not just the caller's</param>
    /// <returns>List of commands</returns>
    internal CommandInfo[] GetCommands(UpdateHandler handler, bool all, bool allHandlers) {
        return Available(handler, allHandlers).SelectMany(x => x.Methods)
            .Where(x => x.Conditions.Any(j => j is CommandAttribute))
            .Where(x => all || x.Conditions.Where(j => j is not CommandAttribute).ToArray().Match(handler))
            .Select(x => new CommandInfo(x.Method)).ToArray();
    }
}
