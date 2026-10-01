using Telegram.Bot;
using Telegram.Bot.Polling;
using TheAirBlow.Stateful.Attributes;
using TheAirBlow.Stateful.Commands;
using TheAirBlow.Stateful.Conditions;

namespace TheAirBlow.Stateful;

/// <summary>
/// Stateful bot options
/// </summary>
public class StatefulOptions {
    /// <summary>
    /// An array of <see cref="HandlerAttribute"/> for filtering updates globally before any handlers.
    /// </summary>
    public HandlerAttribute[] Filters { get; set; } = [];

    /// <summary>
    /// Makes the bot automatically answer any callback queries before the handler runs. Enabled by default.<br/><br/>
    /// You can make an exemption by adding <see cref="AnswersQueryAttribute"/> to the handler method.
    /// </summary>
    public bool AnswerCallbackQueries { get; set; } = true;

    /// <summary>
    /// Default threading option for update handlers. Set to <see cref="Threading.PerUser"/> by default.
    /// Can be overridden by adding <see cref="RunWithAttribute"/> to a method or class.
    /// </summary>
    public Threading DefaultThreading { get; set; } = Threading.PerUser;
    
    /// <summary>
    /// Prefix of the callback data that Stateful's own buttons (such as the paginator's page buttons) use.
    /// </summary>
    public string InternalPrefix { get; set; } = "stinternal-";

    /// <summary>
    /// How many updates may queue up per user or chat before new ones are dropped and sent to <see cref="ErrorHandler"/>. 0 for no limit.
    /// </summary>
    public int MaxQueuedUpdates { get; set; } = 100;
    
    /// <summary>
    /// Message state handler. Disables states completely if set to null.
    /// </summary>
    public IMessageStateHandler? StateHandler { get; set; }
    
    /// <summary>
    /// Error handler to use
    /// </summary>
    public HandleErrorDelegate? ErrorHandler { get; set; }
    
    /// <summary>
    /// Command error handler to use
    /// </summary>
    public HandleCommandErrorDelegate? CommandErrorHandler { get; set; }

    /// <summary>
    /// Is entire bot private chat only
    /// </summary>
    internal bool PrivateOnly => Filters.Any(x => x is PrivateOnlyAttribute { PrivateOnly: true });
    
    /// <summary>
    /// Handle command error delegate
    /// </summary>
    public delegate Task HandleCommandErrorDelegate(
        UpdateHandler handler,
        Exception exception,
        CommandInfo command
    );
    
    /// <summary>
    /// Handle error delegate
    /// </summary>
    public delegate Task HandleErrorDelegate(
        ITelegramBotClient botClient,
        Exception exception,
        HandleErrorSource errorSource,
        CancellationToken cancellationToken
    );
}

/// <summary>
/// Threading types
/// </summary>
public enum Threading {
    /// <summary>
    /// Every update is processed independently, in no particular order.
    /// </summary>
    PerUpdate,
        
    /// <summary>
    /// Updates from the same user are processed one after another, different users in parallel.
    /// <br/><br/>
    /// This is the default option. Fallbacks to <see cref="PerChat"/> if user ID is not available.
    /// </summary>
    PerUser,
        
    /// <summary>
    /// Updates from the same chat are processed one after another, different chats in parallel.
    /// </summary>
    PerChat,
    
    /// <summary>
    /// Everything is processed on the receiver's thread, stalling polling. You should never have to use this.
    /// </summary>
    Disabled
}