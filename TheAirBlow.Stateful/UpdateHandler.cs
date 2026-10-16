using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TheAirBlow.Stateful.Commands;
using TheAirBlow.Stateful.Conditions;
using TheAirBlow.Stateful.Exceptions;
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
    /// Command parsed from the update, valid if <see cref="CommandParsed"/> is set
    /// </summary>
    internal Command? ParsedCommand { get; set; }
    
    /// <summary>
    /// Was the command parsed already
    /// </summary>
    internal bool CommandParsed { get; set; }

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
    /// Stops this method and lets the next applicable method take over, including methods of other handler classes.
    /// State changes made so far are kept, but are only saved if <see cref="SaveState"/> was called.
    /// </summary>
    [DoesNotReturn]
    public void Yield() => throw new YieldException();

    /// <summary>
    /// Saves state changes to the database.
    /// Required if you edit the current message instead of sending a new one.
    /// </summary>
    public async Task SaveState() {
        await Stateful.SaveState(State);
    }
    
    /// <summary>
    /// Generates a reply keyboard for this handler
    /// </summary>
    /// <param name="allHandlers">Include buttons of all handlers available for the current state (e.g. the active one when called from a global handler)</param>
    /// <returns>Generated reply keyboard</returns>
    public ReplyKeyboardMarkup GenerateReply(bool allHandlers = false)
        => Stateful.GenerateReply(this, allHandlers);
    
    /// <summary>
    /// Generates an inline keyboard for this handler
    /// </summary>
    /// <param name="allHandlers">Include buttons of all handlers available for the current state (e.g. the active one when called from a global handler)</param>
    /// <returns>Generated inline keyboard</returns>
    public InlineKeyboardMarkup GenerateInline(bool allHandlers = false)
        => Stateful.GenerateInline(this, allHandlers);

    /// <summary>
    /// Returns a list of available commands
    /// </summary>
    /// <param name="all">Include commands whose other conditions (such as private only) don't match the current update</param>
    /// <param name="allHandlers">Include commands of all handlers available for the current state</param>
    /// <returns>List of commands</returns>
    public CommandInfo[] GetCommands(bool all = false, bool allHandlers = false)
        => Stateful.GetCommands(this, all, allHandlers);
}