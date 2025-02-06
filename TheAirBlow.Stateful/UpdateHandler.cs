using JetBrains.Annotations;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TheAirBlow.Stateful.Commands;
using TheAirBlow.Stateful.Conditions;
using TheAirBlow.Stateful.Keyboards;

namespace TheAirBlow.Stateful; 

/// <summary>
/// Update handler class
/// </summary>
[PublicAPI]
public partial class UpdateHandler {
    /// <summary>
    /// Telegram bot client instance
    /// </summary>
    public TelegramBotClient Client { get; internal set; } = null!;
    
    /// <summary>
    /// Stateful bot instance
    /// </summary>
    public StatefulHandler Stateful { get; internal set; } = null!;
    
    /// <summary>
    /// Current message state
    /// </summary>
    public MessageState State { get; internal set; } = null!;
    
    /// <summary>
    /// Handled update instance
    /// </summary>
    public Update Update { get; internal set; } = null!;

    /// <summary>
    /// Telegram Chat ID
    /// </summary>
    public long? ChatId => Update.GetChatId();
    
    /// <summary>
    /// Telegram User ID
    /// </summary>
    public long? UserId => Update.GetUserId();
    
    /// <summary>
    /// Telegram Message ID
    /// </summary>
    public int? MessageId => Update.GetMessageId();

    /// <summary>
    /// Changes the current <see cref="UpdateHandler"/> and runs its default method if necessary.
    /// </summary>
    /// <param name="id">Handler ID</param>
    /// <param name="runDefault">Run default</param>
    public async Task ChangeHandler(string id, bool runDefault = false)
        => await Stateful.ChangeHandler(this, id, runDefault);

    /// <summary>
    /// Saves state changes to the database.
    /// Required if you edit the current message instead of sending a new one.
    /// </summary>
    public async Task SaveState() {
        if (Stateful.Options.StateHandler == null) return;
        await Stateful.Options.StateHandler.Update(State);
    }
    
    /// <summary>
    /// Generates a reply keyboard for this handler
    /// </summary>
    /// <returns>Generated reply keyboard</returns>
    public ReplyKeyboardMarkup GenerateReply() {
        var wrapper = Stateful.Handlers.FirstOrDefault(x => x.Handler == GetType());
        if (wrapper == null) throw new InvalidOperationException(
            "This method can only be used inside a registered handler");
        var list = new List<string>();
        
        foreach (var method in wrapper.Methods) {
            var msg = (MessageAttribute?)method.Conditions.FirstOrDefault(x => x is MessageAttribute);
            if (msg == null || msg.Hidden || msg.Selector == null || msg.Matcher != Data.Equals || msg.Type != MessageType.Text) continue;
            if (method.Conditions.Where(x => x is not MessageAttribute)
                .Any(x => !x.MatchAsync(this).GetAwaiter().GetResult())) continue;
            list.Add(msg.Selector);
        }
        return Keyboard.Reply(list.ToArray());
    }
    
    /// <summary>
    /// Generates a reply keyboard for this handler
    /// </summary>
    /// <returns>Generated reply keyboard</returns>
    public InlineKeyboardMarkup GenerateInline() {
        var wrapper = Stateful.Handlers.FirstOrDefault(x => x.Handler == GetType());
        if (wrapper == null) throw new InvalidOperationException(
            "This method can only be used inside a registered handler");
        
        var dict = new Dictionary<string, string>();
        foreach (var method in wrapper.Methods) {
            var call = (CallbackAttribute?)method.Conditions.FirstOrDefault(x => x is CallbackAttribute);
            if (call == null || call.Hidden || call.Selector == null || call.Matcher != Data.Equals) continue;
            if (method.Conditions.Where(x => x is not CallbackAttribute)
                .Any(x => !x.MatchAsync(this).GetAwaiter().GetResult())) continue;
            dict.Add(call.Name ?? call.Selector, call.Selector.TrimEnd('\n'));
        }
        return Keyboard.Inline(dict);
    }

    /// <summary>
    /// Returns a list of available commands
    /// </summary>
    /// <param name="all">Include restricted</param>
    /// <returns>List of commands</returns>
    public CommandInfo[] GetCommands(bool all = false) {
        var wrapper = Stateful.Handlers.FirstOrDefault(x => x.Handler == GetType());
        if (wrapper == null) throw new InvalidOperationException(
            "This method can only be used inside a registered handler");

        return wrapper.Methods.Where(x => x.Conditions.Any(j => j is CommandAttribute))
            .Select(x => new CommandInfo(x.Method)).ToArray();
    }
}