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
    /// Sequential lanes, keyed by threading type and user or chat ID.
    /// </summary>
    private readonly Dictionary<(Threading, long), Queue<QueueItem>> _lanes = new();

    /// <summary>
    /// Execution lane of an update
    /// </summary>
    /// <param name="Kind">Threading type, never <see cref="Threading.PerUser"/> or <see cref="Threading.PerChat"/> without a key</param>
    /// <param name="Key">User or chat ID, zero if not keyed</param>
    private readonly record struct Lane(Threading Kind, long Key);

    /// <summary>
    /// Queue item
    /// </summary>
    /// <param name="Work">Work to run</param>
    /// <param name="Bot">Telegram bot client</param>
    /// <param name="Token">Cancellation token</param>
    private readonly record struct QueueItem(Func<Task> Work, TelegramBotClient Bot, CancellationToken Token);

    /// <summary>
    /// Resolves the lane an update has to run on.
    /// </summary>
    /// <param name="update">Update</param>
    /// <param name="threading">Threading type</param>
    /// <returns>Lane</returns>
    private static Lane ResolveLane(Update update, Threading threading) {
        if (threading == Threading.PerUser) {
            var userId = update.GetUserId();
            if (userId.HasValue) return new Lane(Threading.PerUser, userId.Value);
            threading = Threading.PerChat;
        }

        if (threading != Threading.PerChat) return new Lane(threading, 0);
        var chatId = update.GetChatId();
        return chatId.HasValue ? new Lane(Threading.PerChat, chatId.Value) : new Lane(Threading.PerUpdate, 0);
    }

    /// <summary>
    /// Runs work, reporting any exception to the error handler. Never throws.
    /// </summary>
    /// <param name="bot">Telegram bot client</param>
    /// <param name="token">Cancellation token</param>
    /// <param name="work">Work to run</param>
    private async Task RunSafe(TelegramBotClient bot, CancellationToken token, Func<Task> work) {
        try {
            await work();
        } catch (SilentException) {
            // Ignore
        } catch (Exception e) {
            await ReportError(bot, e, token);
        }
    }

    /// <summary>
    /// Passes an exception to the error handler, which is not allowed to throw
    /// </summary>
    /// <param name="bot">Telegram bot client</param>
    /// <param name="exception">Exception</param>
    /// <param name="token">Cancellation token</param>
    private async Task ReportError(TelegramBotClient bot, Exception exception, CancellationToken token) {
        if (Options.ErrorHandler == null) return;
        try {
            await Options.ErrorHandler(bot, exception, HandleErrorSource.HandleUpdateError, token);
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
    /// <returns>False if the lane is full and the work was dropped</returns>
    private bool Enqueue(Lane lane, TelegramBotClient bot, CancellationToken token, Func<Task> work) {
        var item = new QueueItem(work, bot, token);
        if (lane.Kind is not (Threading.PerUser or Threading.PerChat)) {
            _ = Task.Run(() => RunSafe(bot, token, work), CancellationToken.None);
            return true;
        }

        lock (_lanes) {
            if (_lanes.TryGetValue((lane.Kind, lane.Key), out var queue)) {
                if (Options.MaxQueuedUpdates > 0 && queue.Count >= Options.MaxQueuedUpdates) return false;
                queue.Enqueue(item);
                return true;
            }

            _lanes.Add((lane.Kind, lane.Key), new Queue<QueueItem>());
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
        var key = (lane.Kind, lane.Key);
        while (true) {
            try {
                if (!item.Token.IsCancellationRequested)
                    await RunSafe(item.Bot, item.Token, item.Work);
            } catch {
                // ignore
            }

            lock (_lanes) {
                var queue = _lanes[key];
                if (queue.Count == 0) {
                    _lanes.Remove(key);
                    return;
                }

                item = queue.Dequeue();
            }
        }
    }
}
