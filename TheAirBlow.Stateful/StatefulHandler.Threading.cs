using Telegram.Bot;
using Telegram.Bot.Polling;

namespace TheAirBlow.Stateful;

/// <summary>
/// Stateful logic implementation
/// </summary>
public partial class StatefulHandler {
    /// <summary>
    /// Per user update queue
    /// </summary>
    private Dictionary<long, Queue<QueueItem>> _userQueue = new();
    
    /// <summary>
    /// Per chat update queue
    /// </summary>
    private Dictionary<long, Queue<QueueItem>> _chatQueue = new();

    /// <summary>
    /// Per user method wrapper
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="item">Queue Item</param>
    private void Threading_UserWrapper(long id, QueueItem item) {
        Threading_MethodWrapper(item);
        while (true)
            lock (_userQueue) {
                var queue = _userQueue[id];
                if (queue.Count == 0) break;
                item = queue.Dequeue();
                Threading_MethodWrapper(item);
            }
        lock (_userQueue)
            _userQueue.Remove(id);
    }
    
    /// <summary>
    /// Per chat method wrapper
    /// </summary>
    /// <param name="id">Chat ID</param>
    /// <param name="item">Queue Item</param>
    private void Threading_ChatWrapper(long id, QueueItem item) {
        Threading_MethodWrapper(item);
        while (true)
            lock (_chatQueue) {
                var queue = _chatQueue[id];
                if (queue.Count == 0) break;
                item = queue.Dequeue();
                Threading_MethodWrapper(item);
            }
        lock (_chatQueue)
            _chatQueue.Remove(id);
    }

    /// <summary>
    /// Normal method wrapper
    /// </summary>
    /// <param name="item">Queue Item</param>
    private void Threading_MethodWrapper(QueueItem item) {
        try {
            item.Invoke();
        } catch (Exception e) {
            if (Options.ErrorHandler == null) return;
            Options.ErrorHandler(item.Bot, e, HandleErrorSource.HandleUpdateError, item.Token);
        }
    }

    /// <summary>
    /// Invokes a method in a threaded fashion
    /// </summary>
    /// <param name="bot">Telegram Bot</param>
    /// <param name="token">Cancellation token</param>
    /// <param name="wrapper">Method</param>
    /// <param name="handler">Handler</param>
    private void Threading_Invoke(TelegramBotClient bot, CancellationToken token, MethodWrapper wrapper, UpdateHandler handler) {
        var item = new QueueItem(bot, token, wrapper, handler);
        switch (wrapper.Threading) {
            case Threading.PerUpdate:
                new Thread(() => Threading_MethodWrapper(item)).Start();
                break;
            case Threading.PerUser:
                if (!handler.UserId.HasValue) goto goto_PerChat;
                var userId = handler.UserId.Value;
                lock (_userQueue) {
                    if (_userQueue.TryGetValue(userId, out var queue)) {
                        queue.Enqueue(item);
                        break;
                    }
                    
                    _userQueue.Add(userId, []);
                    new Thread(() => Threading_UserWrapper(userId, item)).Start();
                }
                break;
            case Threading.PerChat:
                goto_PerChat:
                var chatId = handler.ChatId!.Value;
                lock (_chatQueue) {
                    if (_chatQueue.TryGetValue(chatId, out var queue)) {
                        queue.Enqueue(item);
                        break;
                    }
                    
                    _chatQueue.Add(chatId, []);
                    new Thread(() => Threading_ChatWrapper(chatId, item)).Start();
                }
                break;
            case Threading.Disabled:
                item.Invoke();
                return;
        }
    }

    /// <summary>
    /// Queue item
    /// </summary>
    private class QueueItem {
        /// <summary>
        /// Telegram bot client
        /// </summary>
        public TelegramBotClient Bot { get; }
        
        /// <summary>
        /// Cancellation token
        /// </summary>
        public CancellationToken Token { get; }
        
        /// <summary>
        /// Method to invoke
        /// </summary>
        private MethodWrapper Method { get; }
        
        /// <summary>
        /// Handler instance
        /// </summary>
        private UpdateHandler Handler { get; }

        /// <summary>
        /// Creates a new queue item
        /// </summary>
        /// <param name="bot">Telegram Bot</param>
        /// <param name="token">Cancellation token</param>
        /// <param name="method">Method</param>
        /// <param name="handler">Handler</param>
        public QueueItem(TelegramBotClient bot, CancellationToken token, MethodWrapper method, UpdateHandler handler) {
            Bot = bot; Token = token; Method = method; Handler = handler;
        }

        /// <summary>
        /// Invokes queue item's method
        /// </summary>
        public void Invoke() => Method.Invoke(Handler).GetAwaiter().GetResult();
    }
}