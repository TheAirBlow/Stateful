using System.Reflection;
using JetBrains.Annotations;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TheAirBlow.Stateful.Attributes;
using TheAirBlow.Stateful.Exceptions;

namespace TheAirBlow.Stateful;

/// <summary>
/// Stateful logic implementation
/// </summary>
[PublicAPI]
public class StatefulHandler : IUpdateHandler {
    /// <summary>
    /// Binding flags to use for searching methods
    /// </summary>
    internal const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

    /// <summary>
    /// List of update handlers
    /// </summary>
    private readonly List<HandlerWrapper> _handlers = [];

    /// <summary>
    /// Stateful options
    /// </summary>
    public StatefulOptions Options { get; }

    /// <summary>
    /// Creates a new stateful client
    /// </summary>
    /// <param name="options">Stateful Options</param>
    public StatefulHandler(StatefulOptions? options = null) {
        Options = options ?? new StatefulOptions();
        Register<InternalHandler>();
    }
    
    /// <summary>
    /// Registers a handler. If unique ID is null, handler is considered global.
    /// </summary>
    /// <param name="id">Unique ID</param>
    /// <typeparam name="T">Type</typeparam>
    public void Register<T>(string? id = null) where T : UpdateHandler
        => _handlers.Add(new HandlerWrapper(typeof(T), id));

    /// <summary>
    /// Handles an update asynchronously
    /// </summary>
    /// <param name="bot">Telegram bot client</param>
    /// <param name="update">Telegram update</param>
    /// <param name="token">Cancellation token</param>
    public Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token) {
        new Thread(async() => await HandleUpdate((TelegramBotClient)bot, update, token)).Start();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles an update asynchronously
    /// </summary>
    /// <param name="bot">Telegram bot client</param>
    /// <param name="update">Telegram update</param>
    /// <param name="token">Cancellation token</param>
    private async Task HandleUpdate(TelegramBotClient bot, Update update, CancellationToken token) {
        try {
            var handler = CreateHandler(bot, update);
            if (Options.StateHandler != null)
                handler.State = await Options.StateHandler.GetState(update);
            if (!Options.Filters.All(x => x.Match(handler).GetAwaiter().GetResult())) return;
            var method = GetMethod(handler);
            if (method == null) return;
            if (Options.AnswerCallbackQueries && !method.GetCustomAttributes(false).Any(x => x is AnswersQueryAttribute))
                await bot.AnswerCallbackQuery(update.CallbackQuery!.Id, cancellationToken: token);
            handler = CreateHandler(bot, update, handler.State, method.DeclaringType);
            await Invoke(method, handler);
        } catch (Exception e) {
            if (Options.ErrorHandler == null) return;
            await Options.ErrorHandler(bot, e, HandleErrorSource.HandleUpdateError, token);
        }
    }

    /// <summary>
    /// Handles an error asynchronously
    /// </summary>
    /// <param name="bot">Telegram bot client</param>
    /// <param name="exception">Exception</param>
    /// <param name="source">Error source</param>
    /// <param name="token">Cancellation token</param>
    public async Task HandleErrorAsync(ITelegramBotClient bot, Exception exception,
        HandleErrorSource source, CancellationToken token) {
        if (Options.ErrorHandler == null) return;
        await Options.ErrorHandler(bot, exception, source, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Invokes update handler's method
    /// </summary>
    /// <param name="method">Method</param>
    /// <param name="handler">Update Handler</param>
    private async Task Invoke(MethodBase method, UpdateHandler handler)
        => await method.Invoke(handler, []).AwaitIfTask();

    /// <summary>
    /// Creates an update handler
    /// </summary>
    /// <param name="bot">Bot</param>
    /// <param name="type">Type</param>
    /// <param name="state">State</param>
    /// <param name="update">Update</param>
    /// <returns>Update handler</returns>
    private UpdateHandler CreateHandler(TelegramBotClient bot, Update update, MessageState? state = null, Type? type = null) {
        var handler = (UpdateHandler)Activator.CreateInstance(type ?? typeof(UpdateHandler))!;
        handler.Client = bot; handler.Stateful = this;
        handler.State = state ?? new MessageState(); 
        handler.Update = update;
        return handler;
    }

    /// <summary>
    /// Returns handler method to call
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <returns>Handler Method</returns>
    public MethodBase? GetMethod(UpdateHandler handler) {
        var avail = _handlers
            .Where(x => handler.State.HandlerId == null || x.HandlerId == null || x.HandlerId == handler.State.HandlerId)
            .Where(x => {
                var attrs = x.Handler.GetCustomAttributes(false);
                var handlers = attrs.Where(j => j is HandlerAttribute);
                return !handlers.Any() || handlers.All(j => j is HandlerAttribute attr && attr.Match(handler).GetAwaiter().GetResult());
            });
        foreach (var i in avail) {
            var method = i.Methods.FirstOrDefault(x => {
                var attrs = x.GetCustomAttributes(false);
                if (attrs.Any(j => j is DefaultHandlerAttribute)) return false;
                var handlers = attrs.Where(j => j is HandlerAttribute);
                return handlers.Any() && handlers.All(j => j is HandlerAttribute attr && attr.Match(handler).GetAwaiter().GetResult());
            });
            
            if (method == null && handler.Update.Type != UpdateType.CallbackQuery) {
                var attrs = handler.GetType().GetCustomAttributes(false);
                if (attrs.All(x => x is not PrivateOnlyAttribute) || 
                    attrs.Any(x => x is PrivateOnlyAttribute { PrivateOnly: false })) return null;
                method ??= GetDefault(i, handler);
            }
            
            if (method != null)
                return method;
        }

        throw new NoHandlerException(handler.Update);
    }

    /// <summary>
    /// Returns default method in a handler class
    /// </summary>
    /// <param name="wrapper">Handler Wrapper</param>
    /// <param name="handler">Update Handler</param>
    /// <returns>Default Handler</returns>
    private static MethodInfo? GetDefault(HandlerWrapper wrapper, UpdateHandler handler)
        => wrapper.Methods.FirstOrDefault(x => {
            var attrs = x.GetCustomAttributes(false);
            return attrs.Any(j => j is DefaultHandlerAttribute) && attrs.Where(j => j is HandlerAttribute)
                .All(j => j is HandlerAttribute attr && attr.Match(handler).GetAwaiter().GetResult());
        });

    /// <summary>
    /// Runs default method of a handler
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="id">Handler ID</param>
    /// <param name="runDefault">Run default</param>
    internal async Task ChangeHandler(UpdateHandler handler, string? id, bool runDefault) {
        var wrapper = _handlers.FirstOrDefault(x => x.HandlerId == id);
        if (wrapper == null && id == null)
            wrapper ??= _handlers.FirstOrDefault(x => x.HandlerId != null);
        if (wrapper == null)
            throw new ArgumentOutOfRangeException(nameof(id),
                $"No handler with ID {id} was registered");
        if (Options.StateHandler != null) {
            handler.State = await Options.StateHandler.GetState(handler.Update);
            handler.State.SetHandler(id);
        }
        
        await handler.SaveState();
        if (runDefault) {
            var method = GetDefault(wrapper, handler);
            if (method == null) {
                var attrs = handler.GetType().GetCustomAttributes(false);
                if (attrs.All(x => x is not PrivateOnlyAttribute) || 
                    attrs.Any(x => x is PrivateOnlyAttribute { PrivateOnly: false })) return;
                throw new InvalidOperationException($"No default method found for {wrapper.HandlerId}");
            }
            
            handler = CreateHandler(handler.Client, handler.Update, handler.State, method.DeclaringType);
            await Invoke(method, handler);
        }
    }
    
    /// <summary>
    /// Update handler wrapper
    /// </summary>
    private class HandlerWrapper {
        /// <summary>
        /// Update handler type
        /// </summary>
        public Type Handler { get; }
        
        /// <summary>
        /// An array of available methods
        /// </summary>
        public MethodInfo[] Methods { get; }
        
        /// <summary>
        /// Unique handler identifier, null is global
        /// </summary>
        public string? HandlerId { get; set; }

        /// <summary>
        /// Creates a new update handler wrapper
        /// </summary>
        /// <param name="handler">Handler Type</param>
        /// <param name="id">Unique ID</param>
        public HandlerWrapper(Type handler, string? id) {
            Handler = handler; HandlerId = id;
            Methods = handler.GetMethods(Flags);
        }
    }
}