using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using TheAirBlow.Stateful.Exceptions;

namespace TheAirBlow.Stateful;

/// <summary>
/// Stateful logic implementation
/// </summary>
public partial class StatefulHandler {
    /// <summary>
    /// Sequential lanes, keyed by lane.
    /// </summary>
    private readonly Dictionary<Lane, Queue<QueueItem>> _lanes = new();

    /// <summary>
    /// Execution lane of an update
    /// </summary>
    /// <param name="Kind">Threading type, never <see cref="Threading.PerUser"/>, <see cref="Threading.PerChat"/> or <see cref="Threading.PerMessage"/> without a key</param>
    /// <param name="Key">User or chat ID, zero if not keyed</param>
    /// <param name="Message">Message ID, zero if not keyed by message</param>
    private readonly record struct Lane(Threading Kind, long Key = 0, long Message = 0);

    /// <summary>
    /// Queue item
    /// </summary>
    /// <param name="Work">Work to run</param>
    /// <param name="Bot">Telegram bot client</param>
    /// <param name="Token">Cancellation token</param>
    /// <param name="Handler">Update handler the work runs on, null if there is none yet</param>
    private readonly record struct QueueItem(Func<Task> Work, TelegramBotClient Bot, CancellationToken Token, UpdateHandler? Handler);

    /// <summary>
    /// Resolves the lane an update has to run on.
    /// </summary>
    /// <param name="update">Update</param>
    /// <param name="threading">Threading type</param>
    /// <returns>Lane</returns>
    private static Lane ResolveLane(Update update, Threading threading) {
        if (threading == Threading.PerMessage) {
            var messageChatId = update.GetChatId();
            var messageId = update.GetMessageId();
            if (messageChatId.HasValue && messageId.HasValue)
                return new Lane(Threading.PerMessage, messageChatId.Value, messageId.Value);
            threading = Threading.PerUser;
        }

        if (threading == Threading.PerUser) {
            var userId = update.GetUserId();
            if (userId.HasValue) return new Lane(Threading.PerUser, userId.Value);
            threading = Threading.PerChat;
        }

        if (threading != Threading.PerChat) return new Lane(threading);
        var chatId = update.GetChatId();
        return chatId.HasValue ? new Lane(Threading.PerChat, chatId.Value) : new Lane(Threading.PerUpdate);
    }

    /// <summary>
    /// Creates the exception for an update that was dropped because its lane is full
    /// </summary>
    /// <param name="update">Update</param>
    /// <param name="lane">Lane</param>
    /// <returns>Exception</returns>
    private InvalidOperationException Dropped(Update update, Lane lane)
        => new($"Dropped update {update.Id}: more than {Options.MaxQueuedUpdates} updates are waiting for {lane.Kind} {lane.Key}"
            + (lane.Kind == Threading.PerMessage ? $" message {lane.Message}" : ""));

    /// <summary>
    /// Runs work, reporting any exception to the error handler. Never throws.
    /// </summary>
    /// <param name="bot">Telegram bot client</param>
    /// <param name="token">Cancellation token</param>
    /// <param name="work">Work to run</param>
    /// <param name="handler">Returns the update handler the work runs on, if there is one by now</param>
    private async Task RunSafe(TelegramBotClient bot, CancellationToken token, Func<Task> work, Func<UpdateHandler?>? handler = null) {
        try {
            await work();
        } catch (SilentException) {
            // Ignore
        } catch (Exception e) {
            await ReportError(bot, e, handler?.Invoke(), token);
        }
    }

    /// <summary>
    /// Passes an exception to the error handler, which is not allowed to throw
    /// </summary>
    /// <param name="bot">Telegram bot client</param>
    /// <param name="exception">Exception</param>
    /// <param name="handler">Update handler, null if there is none</param>
    /// <param name="token">Cancellation token</param>
    /// <param name="source">Error source</param>
    private async Task ReportError(ITelegramBotClient bot, Exception exception, UpdateHandler? handler,
        CancellationToken token, HandleErrorSource source = HandleErrorSource.HandleUpdateError) {
        if (Options.ErrorHandler == null) return;
        try {
            await Options.ErrorHandler(bot, exception, source, handler, token);
        } catch {
            // Nothing left to do if the error handler itself fails
        }
    }

    /// <summary>
    /// Schedules work on a lane.
    /// </summary>
    /// <param name="lane">Lane</param>
    /// <param name="bot">Telegram bot client</param>
    /// <param name="token">Cancellation token</param>
    /// <param name="work">Work to run</param>
    /// <param name="handler">Update handler the work runs on, null if there is none yet</param>
    /// <returns>False if the lane is full and the work was dropped</returns>
    private bool Enqueue(Lane lane, TelegramBotClient bot, CancellationToken token, Func<Task> work, UpdateHandler? handler = null) {
        var item = new QueueItem(work, bot, token, handler);
        if (lane.Kind is not (Threading.PerUser or Threading.PerChat or Threading.PerMessage)) {
            _ = Task.Run(() => RunSafe(bot, token, work, () => handler), CancellationToken.None);
            return true;
        }

        lock (_lanes) {
            if (_lanes.TryGetValue(lane, out var queue)) {
                if (Options.MaxQueuedUpdates > 0 && queue.Count >= Options.MaxQueuedUpdates) return false;
                queue.Enqueue(item);
                return true;
            }

            _lanes.Add(lane, new Queue<QueueItem>());
        }

        _ = Task.Run(() => Drain(lane, item), CancellationToken.None);
        return true;
    }

    /// <summary>
    /// Runs the first item of a lane and everything that gets queued behind it.
    /// </summary>
    /// <param name="lane">Lane</param>
    /// <param name="item">First item</param>
    private async Task Drain(Lane lane, QueueItem item) {
        while (true) {
            try {
                if (!item.Token.IsCancellationRequested)
                    await RunSafe(item.Bot, item.Token, item.Work, () => item.Handler);
            } catch {
                // ignore
            }

            lock (_lanes) {
                var queue = _lanes[lane];
                if (queue.Count == 0) {
                    _lanes.Remove(lane);
                    return;
                }

                item = queue.Dequeue();
            }
        }
    }
}
