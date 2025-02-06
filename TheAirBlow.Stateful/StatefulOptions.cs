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
    /// A new thread will be created for each update.
    /// </summary>
    PerUpdate,
        
    /// <summary>
    /// A new thread will be created for each user.
    /// If an update is received before the previous one is processed,
    /// it will be queued to be processed later on the same thread.
    /// <br/><br/>
    /// This is the default option. Fallbacks to <see cref="PerChat"/> if user ID is not available.
    /// </summary>
    PerUser,
        
    /// <summary>
    /// A new thread will be created for each chat.
    /// If an update is received before the previous one is processed,
    /// it will be queued to be processed later on the same thread.
    /// </summary>
    PerChat,
    
    /// <summary>
    /// Everything would be processed on the handler's thread.
    /// You should never have to use this.
    /// </summary>
    Disabled
}