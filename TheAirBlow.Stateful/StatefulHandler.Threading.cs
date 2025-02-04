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
    private void UserWrapper(long id, QueueItem item) {
        item.Invoke();
        while (true)
            lock (_userQueue) {
                var queue = _userQueue[id];
                if (queue.Count == 0) break;
                item = queue.Dequeue();
                item.Invoke();
            }
        lock (_userQueue)
            _userQueue.Remove(id);
    }
    
    /// <summary>
    /// Per chat method wrapper
    /// </summary>
    /// <param name="id">Chat ID</param>
    /// <param name="item">Queue Item</param>
    private void ChatWrapper(long id, QueueItem item) {
        item.Invoke();
        while (true)
            lock (_chatQueue) {
                var queue = _chatQueue[id];
                if (queue.Count == 0) break;
                item = queue.Dequeue();
                item.Invoke();
            }
        lock (_chatQueue)
            _chatQueue.Remove(id);
    }

    /// <summary>
    /// Invokes a method in a threaded fashion
    /// </summary>
    /// <param name="wrapper">Method</param>
    /// <param name="handler">Handler</param>
    private void InvokeThreaded(MethodWrapper wrapper, UpdateHandler handler) {
        var item = new QueueItem(wrapper, handler);
        switch (wrapper.Threading) {
            case Threading.PerUpdate:
                new Thread(item.Invoke).Start();
                break;
            case Threading.PerUser:
                if (!handler.UserId.HasValue) goto goto_PerChat;
                var userId = handler.UserId.Value;
                lock (_userQueue) {
                    if (_userQueue.TryGetValue(userId, out var queue)) {
                        queue.Enqueue(item);
                        break;
                    }
                    
                    new Thread(() => UserWrapper(userId, item)).Start();
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
                    
                    new Thread(() => ChatWrapper(chatId, item)).Start();
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
        /// <param name="method">Method</param>
        /// <param name="handler">Handler</param>
        public QueueItem(MethodWrapper method, UpdateHandler handler) {
            Method = method; Handler = handler;
        }

        /// <summary>
        /// Invokes queue item's method
        /// </summary>
        public void Invoke() => Method.Invoke(Handler).GetAwaiter().GetResult();
    }
}