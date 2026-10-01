using System.Reflection;
using System.Runtime.ExceptionServices;
using JetBrains.Annotations;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TheAirBlow.Stateful.Attributes;
using TheAirBlow.Stateful.Commands;
using TheAirBlow.Stateful.Conditions;
using TheAirBlow.Stateful.Exceptions;

namespace TheAirBlow.Stateful;

/// <summary>
/// Stateful logic implementation
/// </summary>
[PublicAPI]
public partial class StatefulHandler : IUpdateHandler {
    /// <summary>
    /// Binding flags to use for searching methods
    /// </summary>
    internal const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

    /// <summary>
    /// List of update handlers
    /// </summary>
    internal readonly List<HandlerWrapper> Handlers = [];

    /// <summary>
    /// Stateful options
    /// </summary>
    public StatefulOptions Options { get; }

    /// <summary>
    /// Bot user instance
    /// </summary>
    public User? Bot { get; private set; }

    /// <summary>
    /// Creates a new stateful client
    /// </summary>
    /// <param name="options">Stateful Options</param>
    public StatefulHandler(StatefulOptions? options = null) {
        Options = options ?? new StatefulOptions();
        MatcherAttribute.InternalPrefix = Options.InternalPrefix;
        try {
            Register<InternalHandler>();
        } finally {
            MatcherAttribute.InternalPrefix = null;
        }
    }
    
    /// <summary>
    /// Registers a handler. If unique ID is null, handler is considered global.
    /// </summary>
    /// <param name="id">Unique ID</param>
    /// <typeparam name="T">Type</typeparam>
    public void Register<T>(string? id = null) where T : UpdateHandler
        => Handlers.Add(new HandlerWrapper(Options, typeof(T), id));

    /// <summary>
    /// Handles an update asynchronously.
    /// </summary>
    /// <param name="bot">Telegram bot client</param>
    /// <param name="update">Telegram update</param>
    /// <param name="token">Cancellation token</param>
    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token) {
        var client = (TelegramBotClient)bot;
        var lane = ResolveLane(update, Options.DefaultThreading);
        if (lane.Kind == Threading.Disabled) {
            await RunSafe(client, token, () => Process(client, update, token, lane));
            return;
        }

        if (!Enqueue(lane, client, token, () => Process(client, update, token, lane)))
            await ReportError(client, new InvalidOperationException(
                $"Dropped update {update.Id}: more than {Options.MaxQueuedUpdates} updates are waiting for {lane.Kind} {lane.Key}"), token);
    }

    /// <summary>
    /// Processes an incoming update.
    /// </summary>
    /// <param name="bot">Telegram bot client</param>
    /// <param name="update">Telegram update</param>
    /// <param name="token">Cancellation token</param>
    /// <param name="lane">Lane this runs on</param>
    private async Task Process(TelegramBotClient bot, Update update, CancellationToken token, Lane lane) {
        Bot ??= await bot.GetMe(cancellationToken: token);
        var handler = CreateHandler(bot, update);
        
        foreach (var filter in Options.Filters)
            if (!filter.RequiresState && !await filter.MatchAsync(handler)) return;
        if (Options.StateHandler != null && update.GetChatId() != null && update.GetMessageId() != null)
            handler.State = await Options.StateHandler.GetState(update);
        foreach (var filter in Options.Filters)
            if (filter.RequiresState && !await filter.MatchAsync(handler)) return;

        var method = GetMethod(handler);
        if (method == null) return;
        if (update.Type == UpdateType.CallbackQuery && Options.AnswerCallbackQueries && !method.AnswersQuery)
            try {
                await bot.AnswerCallbackQuery(update.CallbackQuery!.Id, cancellationToken: token);
            } catch (ApiRequestException) {
                // ignore
            }

        handler = CreateHandler(bot, update, handler.State, method.Method.DeclaringType);

        var target = ResolveLane(update, method.Threading);
        if (target == lane || target.Kind == Threading.Disabled) {
            await method.Invoke(handler);
            return;
        }

        if (!Enqueue(target, bot, token, () => method.Invoke(handler)))
            throw new InvalidOperationException(
                $"Dropped update {update.Id}: more than {Options.MaxQueuedUpdates} updates are waiting for {target.Kind} {target.Key}");
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
    private MethodWrapper? GetMethod(UpdateHandler handler) {
        var avail = Handlers
            .Where(x => handler.State.HandlerId == null || x.HandlerId == handler.State.HandlerId || x.HandlerId == null)
            .Where(x => x.Conditions.Match(handler));
        if (handler.State.Expired)
            foreach (var i in avail) {
                var expired = i.Methods.FirstOrDefault(x => x.IsExpired && x.Conditions.Match(handler, false));
                if (expired != null) return expired;
            }

        foreach (var i in avail) {
            var method = i.Methods.FirstOrDefault(x => !x.IsDefault && !x.IsExpired && x.Conditions.Match(handler, false));
            if (method == null && handler.Update.Type != UpdateType.CallbackQuery
                && (i.PrivateOnly || Options.PrivateOnly))
                method = GetDefault(i, handler);
            if (method != null)
                return method;
        }
        
        if (handler.Update.Type == UpdateType.CallbackQuery)
            throw new NoHandlerException(handler.Update);
        return null;
    }

    /// <summary>
    /// Returns default method in a handler class
    /// </summary>
    /// <param name="wrapper">Handler Wrapper</param>
    /// <param name="handler">Update Handler</param>
    /// <returns>Default Handler</returns>
    private static MethodWrapper? GetDefault(HandlerWrapper wrapper, UpdateHandler handler)
        => wrapper.Methods.FirstOrDefault(x => x.IsDefault && x.Conditions.Match(handler, false));

    /// <summary>
    /// Runs default method of a handler
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="id">Handler ID</param>
    /// <param name="runDefault">Run default</param>
    internal async Task ChangeHandler(UpdateHandler handler, string? id, bool runDefault) {
        var wrapper = Handlers.FirstOrDefault(x => x.HandlerId == id);
        if (wrapper == null)
            throw new ArgumentOutOfRangeException(nameof(id),
                $"No handler with ID {id} was registered");
        handler.State.SetHandler(id);
        await handler.SaveState();
        if (runDefault) {
            var method = GetDefault(wrapper, handler);
            if (method == null) {
                if (!wrapper.PrivateOnly) return;
                throw new InvalidOperationException($"No default method found for {wrapper.HandlerId}");
            }
            
            handler = CreateHandler(handler.Client, handler.Update, handler.State, method.Method.DeclaringType);
            await method.Invoke(handler);
        }
    }
    
    /// <summary>
    /// Update handler wrapper
    /// </summary>
    internal class HandlerWrapper {
        /// <summary>
        /// Update handler type
        /// </summary>
        public Type Handler { get; }
        
        /// <summary>
        /// An array of handler attributes (conditions)
        /// </summary>
        public HandlerAttribute[] Conditions { get; }
        
        /// <summary>
        /// An array of available method wrappers
        /// </summary>
        public MethodWrapper[] Methods { get; }
        
        /// <summary>
        /// Unique handler identifier, null is global
        /// </summary>
        public string? HandlerId { get; set; }
        
        /// <summary>
        /// Threading type
        /// </summary>
        public Threading Threading { get; }
        
        /// <summary>
        /// Is method handler private chat only
        /// </summary>
        public bool PrivateOnly { get; }

        /// <summary>
        /// Creates a new update handler wrapper
        /// </summary>
        /// <param name="options">Stateful Options</param>
        /// <param name="handler">Handler Type</param>
        /// <param name="id">Unique ID</param>
        public HandlerWrapper(StatefulOptions options, Type handler, string? id) {
            Handler = handler; HandlerId = id;
            var attributes = handler.GetCustomAttributes(false);
            Conditions = attributes.Where(x => x is HandlerAttribute).Cast<HandlerAttribute>().ToArray();
            PrivateOnly = attributes.Any(x => x is PrivateOnlyAttribute { PrivateOnly: true });
            Threading = options.DefaultThreading;
            var runWith = attributes.FirstOrDefault(x => x is RunWithAttribute);
            if (runWith != null) Threading = ((RunWithAttribute)runWith).Threading;
            Methods = handler.GetMethods(Flags)
                .Where(x => !x.IsSpecialName && x.DeclaringType != typeof(object))
                .Where(x => x.GetParameters().Length == 0 || x.GetCustomAttributes().Any(j => j is CommandAttribute or HandlerAttribute))
                .Where(x => x.GetCustomAttributes(false).Any(j => j is HandlerAttribute or DefaultHandlerAttribute or ExpiredHandlerAttribute))
                .OrderBy(x => x.MetadataToken)
                .Select(x => new MethodWrapper(this, x)).ToArray();
        }
    }

    /// <summary>
    /// Update method wrapper
    /// </summary>
    internal class MethodWrapper {
        /// <summary>
        /// Method information
        /// </summary>
        public MethodInfo Method { get; }
        
        /// <summary>
        /// An array of handler attributes (conditions)
        /// </summary>
        public HandlerAttribute[] Conditions { get; }
        
        /// <summary>
        /// Threading type
        /// </summary>
        public Threading Threading { get; }
        
        /// <summary>
        /// Does this method answer the query manually
        /// </summary>
        public bool AnswersQuery { get; }
        
        /// <summary>
        /// Is this the default handler
        /// </summary>
        public bool IsDefault { get; }
        
        /// <summary>
        /// Is this the handler for expired messages
        /// </summary>
        public bool IsExpired { get; }

        /// <summary>
        /// Creates a new method wrapper
        /// </summary>
        /// <param name="handler">Handler</param>
        /// <param name="method">Method</param>
        public MethodWrapper(HandlerWrapper handler, MethodInfo method) {
            Method = method; var attributes = method.GetCustomAttributes(false);
            Conditions = attributes.Where(x => x is HandlerAttribute).Cast<HandlerAttribute>().ToArray();
            AnswersQuery = attributes.Any(x => x is AnswersQueryAttribute);
            IsDefault = attributes.Any(x => x is DefaultHandlerAttribute);
            IsExpired = attributes.Any(x => x is ExpiredHandlerAttribute);
            Threading = handler.Threading;
            var runWith = attributes.FirstOrDefault(x => x is RunWithAttribute);
            if (runWith != null) Threading = ((RunWithAttribute)runWith).Threading;
        }

        /// <summary>
        /// Invokes this method
        /// </summary>
        /// <param name="handler">Update Handler</param>
        public async Task Invoke(UpdateHandler handler) {
            foreach (var cond in Conditions) {
                var args = cond.GetArguments(handler, Method);
                if (args == null) continue;
                if (args.Length != Method.GetParameters().Length)
                    throw new InvalidDataException($"Expected {Method.GetParameters().Length} arguments from {cond.GetType().FullName} but found {args.Length}");
                await InvokeMethod(handler, args);
                return;
            }
            
            await InvokeMethod(handler, []);
        }

        /// <summary>
        /// Invokes the method and unwraps <see cref="TargetInvocationException"/>
        /// </summary>
        /// <param name="handler">Update Handler</param>
        /// <param name="args">Arguments</param>
        private async Task InvokeMethod(UpdateHandler handler, object[] args) {
            try {
                await Method.Invoke(handler, args).AwaitIfTask();
            } catch (TargetInvocationException e) when (e.InnerException != null) {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            }
        }
    }
}
