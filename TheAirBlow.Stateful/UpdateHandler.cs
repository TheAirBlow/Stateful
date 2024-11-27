using JetBrains.Annotations;
using Telegram.Bot;
using Telegram.Bot.Types;

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
    /// Saves state changes to the database
    /// </summary>
    public async Task SaveState() {
        if (Stateful.Options.StateHandler == null) return;
        await Stateful.Options.StateHandler.Update(State);
    }
}