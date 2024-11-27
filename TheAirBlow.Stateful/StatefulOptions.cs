using Telegram.Bot;
using Telegram.Bot.Polling;
using TheAirBlow.Stateful.Attributes;

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
    /// Message state handler. Disables states completely if set to null.
    /// </summary>
    public IMessageStateHandler? StateHandler { get; set; }
    
    /// <summary>
    /// Error handler to use
    /// </summary>
    public HandleErrorDelegate? ErrorHandler { get; set; }
    
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