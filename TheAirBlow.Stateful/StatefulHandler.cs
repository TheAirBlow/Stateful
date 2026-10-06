using System.Reflection;
using System.Reflection.Metadata;
using System.Diagnostics.CodeAnalysis;
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
    /// Members of handlers that are found with <see cref="Flags"/>, which trimming has to keep
    /// </summary>
    internal const DynamicallyAccessedMemberTypes Members
        = DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods;

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
        Register<InternalHandler>();
    }
    
    /// <summary>
    /// Saves state changes to the database.
    /// </summary>
    /// <exception cref="StateConflictException">State was modified since it was loaded</exception>
    /// <param name="state">Message state</param>
    public async Task SaveState(MessageState state) {
        if (Options.StateHandler == null) return;
        await Options.StateHandler.Update(state);
    }

    /// <summary>
    /// Stores state for a message that was just sent or edited.
    /// If the message is not the one <paramref name="source"/> belongs to,
    /// the stored state of the message gets the handler and values of <paramref name="source"/>.
    /// </summary>
    /// <param name="source">State to take values from</param>
    /// <param name="msg">Message</param>
    /// <returns>State that was stored, <paramref name="source"/> if there is no state handler</returns>
    public async Task<MessageState> PutState(MessageState source, Message msg) {
        var stateHandler = Options.StateHandler;
        if (stateHandler == null) return source;
        var state = source;
        if (source.ChatId != msg.Chat.Id || source.MessageId != msg.MessageId) {
            state = await stateHandler.GetState(msg);
            state.HandlerId = source.HandlerId;
            state.State = new Dictionary<string, string>(source.State);
            foreach (var key in source.PendingLocal)
                if (source.LocalState.TryGetValue(key, out var value))
                    state.LocalState[key] = value;
        }

        state.LastUpdated = DateTime.UtcNow;
        await SaveState(state);
        return state;
    }

    /// <summary>
    /// Registers a handler. If unique ID is null, handler is considered global.
    /// </summary>
    /// <param name="id">Unique ID</param>
    /// <typeparam name="T">Type</typeparam>
    public void Register<[DynamicallyAccessedMembers(Members)] T>(string? id = null) where T : UpdateHandler, new()
        => Handlers.Add(new HandlerWrapper(Options, typeof(T), static () => new T(), id));

    /// <summary>
    /// Handles an update asynchronously.
    /// </summary>
    /// <param name="bot">Telegram bot client</param>
    /// <param name="update">Telegram update</param>
    /// <param name="token">Cancellation token</param>
    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token) {
        if (bot is not TelegramBotClient client) {
            await ReportError(bot, new ArgumentException(
                $"Stateful needs a {nameof(TelegramBotClient)}, got {bot.GetType().FullName}", nameof(bot)), null, token);
            return;
        }

        var lane = ResolveLane(update, Options.DefaultThreading);
        if (lane.Kind == Threading.Disabled) {
            await Process(client, update, token, lane);
            return;
        }

        if (!Enqueue(lane, client, token, () => Process(client, update, token, lane)))
            await ReportError(client, Dropped(update, lane), null, token);
    }

    /// <summary>
    /// Processes an incoming update, reporting any exception to the error handler. Never throws.
    /// </summary>
    /// <param name="bot">Telegram bot client</param>
    /// <param name="update">Telegram update</param>
    /// <param name="token">Cancellation token</param>
    /// <param name="lane">Lane this runs on</param>
    private Task Process(TelegramBotClient bot, Update update, CancellationToken token, Lane lane) {
        UpdateHandler? current = null;
        return RunSafe(bot, token, () => Handle(bot, update, token, lane, x => current = x), () => current);
    }

    /// <summary>
    /// Finds the method that handles an update and runs it on its lane.
    /// </summary>
    /// <param name="bot">Telegram bot client</param>
    /// <param name="update">Telegram update</param>
    /// <param name="token">Cancellation token</param>
    /// <param name="lane">Lane this runs on</param>
    /// <param name="track">Called with every update handler created, so errors can be reported with it</param>
    private async Task Handle(TelegramBotClient bot, Update update, CancellationToken token, Lane lane, Action<UpdateHandler> track) {
        Bot ??= await bot.GetMe(cancellationToken: token);
        var matcher = CreateHandler(bot, update);
        track(matcher);
        
        foreach (var filter in Options.Filters)
            if (!filter.RequiresState && !await filter.MatchAsync(matcher)) return;
        if (Options.StateHandler != null && update.GetChatId() != null && update.GetMessageId() != null)
            matcher.State = await Options.StateHandler.GetState(update);
        if (Options.AssumeExpired && Options.StateHandler != null && update.CallbackQuery != null && matcher.State.Version == 0)
            matcher.State.Expired = true;
        foreach (var filter in Options.Filters)
            if (filter.RequiresState && !await filter.MatchAsync(matcher)) return;

        await Dispatch(bot, update, token, lane, matcher, [], track);
    }

    /// <summary>
    /// Finds the next method that handles an update and runs it on its lane.
    /// </summary>
    /// <param name="bot">Telegram bot client</param>
    /// <param name="update">Update</param>
    /// <param name="token">Cancellation token</param>
    /// <param name="lane">Lane this runs on</param>
    /// <param name="matcher">Update handler used for matching, holds the state</param>
    /// <param name="skipped">Methods that yielded already</param>
    /// <param name="track">Called with every update handler created, so errors can be reported with it</param>
    private async Task Dispatch(TelegramBotClient bot, Update update, CancellationToken token, Lane lane,
        UpdateHandler matcher, List<MethodWrapper> skipped, Action<UpdateHandler> track) {
        var method = await GetMethod(matcher, skipped);
        if (method == null) return;
        if (update.Type == UpdateType.CallbackQuery && Options.AnswerCallbackQueries && !method.AnswersQuery && skipped.All(x => x.AnswersQuery))
            try {
                await bot.AnswerCallbackQuery(update.CallbackQuery!.Id, cancellationToken: token);
            } catch (ApiRequestException) {
                // ignore
            }

        var handler = CreateHandler(bot, update, matcher.State, method.Create);
        handler.ParsedCommand = matcher.ParsedCommand;
        handler.CommandParsed = matcher.CommandParsed;
        track(handler);

        async Task Run() {
            try {
                await method.Invoke(handler);
            } catch (YieldException) {
                matcher.State = handler.State;
                await Dispatch(bot, update, token, ResolveLane(update, method.Threading), matcher, [..skipped, method], track);
            }
        }

        var target = ResolveLane(update, method.Threading);
        if (target == lane || target.Kind == Threading.Disabled) {
            await Run();
            return;
        }

        if (!Enqueue(target, bot, token, Run, handler))
            throw Dropped(update, target);
    }

    /// <summary>
    /// Handles an error asynchronously
    /// </summary>
    /// <param name="bot">Telegram bot client</param>
    /// <param name="exception">Exception</param>
    /// <param name="source">Error source</param>
    /// <param name="token">Cancellation token</param>
    public Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, HandleErrorSource source, CancellationToken token)
        => ReportError(bot, exception, null, token, source);

    /// <summary>
    /// Creates an update handler
    /// </summary>
    /// <param name="bot">Bot</param>
    /// <param name="update">Update</param>
    /// <param name="state">State</param>
    /// <param name="create">Handler factory, a plain <see cref="UpdateHandler"/> if null</param>
    /// <returns>Update handler</returns>
    private UpdateHandler CreateHandler(TelegramBotClient bot, Update update, MessageState? state = null, Func<UpdateHandler>? create = null) {
        var handler = create?.Invoke() ?? new UpdateHandler();
        handler.Client = bot; handler.Stateful = this;
        handler.State = state ?? new MessageState();
        handler.Update = update;
        return handler;
    }

    /// <summary>
    /// Returns handler method to call
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="skipped">Methods that yielded already</param>
    /// <returns>Handler Method</returns>
    private async Task<MethodWrapper?> GetMethod(UpdateHandler handler, List<MethodWrapper> skipped) {
        var type = handler.Update.Type;
        if (handler.State.Expired)
            foreach (var wrapper in Handlers) {
                if (!wrapper.IsAvailable(handler.State) || !await wrapper.Conditions.MatchAsync(handler)) continue;
                foreach (var expired in wrapper.Expired)
                    if (!skipped.Contains(expired) && await expired.Conditions.MatchAsync(handler)) return expired;
            }

        foreach (var wrapper in Handlers) {
            if (!wrapper.IsAvailable(handler.State) || !await wrapper.Conditions.MatchAsync(handler)) continue;
            var method = await wrapper.Find(handler, skipped);
            if (method == null && type != UpdateType.CallbackQuery && (wrapper.PrivateOnly || Options.PrivateOnly))
                method = await GetDefault(wrapper, handler, skipped);
            if (method != null)
                return method;
        }
        
        if (type == UpdateType.CallbackQuery)
            throw new NoHandlerException(handler.Update);
        return null;
    }

    /// <summary>
    /// Returns default method in a handler class
    /// </summary>
    /// <param name="wrapper">Handler Wrapper</param>
    /// <param name="handler">Update Handler</param>
    /// <param name="skipped">Methods that yielded already</param>
    /// <returns>Default Handler</returns>
    private static async Task<MethodWrapper?> GetDefault(HandlerWrapper wrapper, UpdateHandler handler, List<MethodWrapper>? skipped = null) {
        foreach (var method in wrapper.Defaults)
            if (skipped?.Contains(method) != true && await method.Conditions.MatchAsync(handler)) return method;
        return null;
    }

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
            var method = await GetDefault(wrapper, handler);
            if (method == null) {
                if (!wrapper.PrivateOnly) return;
                throw new InvalidOperationException($"No default method found for {wrapper.HandlerId}");
            }
            
            handler = CreateHandler(handler.Client, handler.Update, handler.State, method.Create);
            await method.Invoke(handler);
        }
    }
    
    /// <summary>
    /// Update handler wrapper
    /// </summary>
    internal class HandlerWrapper {
        /// <summary>
        /// Methods that can match one type of update
        /// </summary>
        /// <param name="ByKey">Methods that only match one text or callback data value, by that value</param>
        /// <param name="Rest">Everything else</param>
        private record Bucket(Dictionary<string, MethodWrapper[]> ByKey, MethodWrapper[] Rest);

        /// <summary>
        /// Buckets by update type, created on first use
        /// </summary>
        private readonly Bucket?[] _buckets = new Bucket?[Enum.GetValues<UpdateType>().Max(x => (int)x) + 1];

        /// <summary>
        /// Update handler type
        /// </summary>
        public Type Handler { get; }

        /// <summary>
        /// Creates a handler
        /// </summary>
        public Func<UpdateHandler> Create { get; }
        
        /// <summary>
        /// An array of handler attributes (conditions)
        /// </summary>
        public HandlerAttribute[] Conditions { get; }
        
        /// <summary>
        /// An array of available method wrappers
        /// </summary>
        public MethodWrapper[] Methods { get; }
        
        /// <summary>
        /// Default methods
        /// </summary>
        public MethodWrapper[] Defaults { get; }
        
        /// <summary>
        /// Methods that handle expired messages
        /// </summary>
        public MethodWrapper[] Expired { get; }
        
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
        /// <param name="create">Creates a handler</param>
        /// <param name="id">Unique ID</param>
        public HandlerWrapper(StatefulOptions options, [DynamicallyAccessedMembers(Members)] Type handler,
            Func<UpdateHandler> create, string? id) {
            Handler = handler; Create = create; HandlerId = id;
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
                .OrderBy(x => x.HasMetadataToken() ? x.GetMetadataToken() : 0)
                .Select((x, i) => new MethodWrapper(this, x, i)).ToArray();
            Defaults = Methods.Where(x => x.IsDefault).ToArray();
            Expired = Methods.Where(x => x.IsExpired).ToArray();
        }

        /// <summary>
        /// Checks if this handler can handle updates of a message with specified state
        /// </summary>
        /// <param name="state">Message state</param>
        /// <returns>True if available</returns>
        public bool IsAvailable(MessageState state)
            => state.HandlerId == null || HandlerId == state.HandlerId || HandlerId == null;

        /// <summary>
        /// Returns the first method, in declaration order, whose conditions match the update.
        /// </summary>
        /// <param name="handler">Update Handler</param>
        /// <param name="skipped">Methods that yielded already</param>
        /// <returns>Method, null if none</returns>
        public async Task<MethodWrapper?> Find(UpdateHandler handler, List<MethodWrapper> skipped) {
            var type = handler.Update.Type;
            var bucket = _buckets[(int)type] ??= CreateBucket(type);
            var key = type switch {
                UpdateType.Message => handler.Update.Message?.Text,
                UpdateType.CallbackQuery => handler.Update.CallbackQuery?.Data,
                UpdateType.InlineQuery => handler.Update.InlineQuery?.Query,
                UpdateType.ChosenInlineResult => handler.Update.ChosenInlineResult?.Query,
                _ => null
            };
            
            var keyed = key != null && bucket.ByKey.TryGetValue(key, out var found) ? found : [];
            var rest = bucket.Rest;
            int i = 0, j = 0;
            while (i < keyed.Length || j < rest.Length) {
                var method = j >= rest.Length || (i < keyed.Length && keyed[i].Order < rest[j].Order)
                    ? keyed[i++] : rest[j++];
                if (!skipped.Contains(method) && await method.Conditions.MatchAsync(handler)) return method;
            }

            return null;
        }

        /// <summary>
        /// Creates a bucket of methods that can match updates of a type
        /// </summary>
        /// <param name="type">Update type</param>
        /// <returns>Bucket</returns>
        private Bucket CreateBucket(UpdateType type) {
            var byKey = new Dictionary<string, List<MethodWrapper>>();
            var rest = new List<MethodWrapper>();
            foreach (var method in Methods) {
                if (method.IsDefault || method.IsExpired || (method.ForUpdate != null && method.ForUpdate != type)) continue;
                if (method.Key == null) rest.Add(method);
                else if (byKey.TryGetValue(method.Key, out var list)) list.Add(method);
                else byKey.Add(method.Key, [method]);
            }

            return new Bucket(byKey.ToDictionary(x => x.Key, x => x.Value.ToArray()), rest.ToArray());
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
        /// Position among the methods of the handler
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// The only type of update this method can match, null if it can match any
        /// </summary>
        public UpdateType? ForUpdate { get; }

        /// <summary>
        /// The only text, callback data or query this method can match, null if it can match more than one
        /// </summary>
        public string? Key { get; }

        /// <summary>
        /// Number of parameters
        /// </summary>
        private int ParameterCount { get; }

        /// <summary>
        /// Creates a handler the method can be invoked on
        /// </summary>
        public Func<UpdateHandler> Create { get; }

        /// <summary>
        /// Creates a new method wrapper
        /// </summary>
        /// <param name="handler">Handler</param>
        /// <param name="method">Method</param>
        /// <param name="order">Position among the methods of the handler</param>
        public MethodWrapper(HandlerWrapper handler, MethodInfo method, int order) {
            Method = method; Order = order; Create = handler.Create;
            var attributes = method.GetCustomAttributes(false);
            ParameterCount = method.GetParameters().Length;
            Conditions = attributes.Where(x => x is HandlerAttribute).Cast<HandlerAttribute>().ToArray();
            AnswersQuery = attributes.Any(x => x is AnswersQueryAttribute);
            IsDefault = attributes.Any(x => x is DefaultHandlerAttribute);
            IsExpired = attributes.Any(x => x is ExpiredHandlerAttribute);
            Threading = handler.Threading;
            var runWith = attributes.FirstOrDefault(x => x is RunWithAttribute);
            if (runWith != null) Threading = ((RunWithAttribute)runWith).Threading;
            
            foreach (var condition in Conditions) {
                if (condition.Updates == null) continue;
                ForUpdate ??= condition.Updates;
                if (condition.Updates == ForUpdate)
                    Key ??= condition.ExactValue;
            }
        }

        /// <summary>
        /// Invokes this method
        /// </summary>
        /// <param name="handler">Update Handler</param>
        public async Task Invoke(UpdateHandler handler) {
            foreach (var cond in Conditions) {
                object[]? args;
                try {
                    args = cond.GetArguments(handler, Method);
                } catch (CommandArgumentException e) {
                    if (handler.Stateful.Options.CommandErrorHandler != null)
                        await handler.Stateful.Options.CommandErrorHandler(handler, e.InnerException!, e.Command);
                    throw new SilentException();
                }

                if (args == null) continue;
                if (args.Length != ParameterCount)
                    throw new InvalidDataException($"Expected {ParameterCount} arguments from {cond.GetType().FullName} but found {args.Length}");
                await InvokeMethod(handler, args);
                return;
            }
            
            await InvokeMethod(handler, []);
        }

        /// <summary>
        /// Invokes the method. Missing arguments become default values,
        /// anything that is not a task is not awaited.
        /// </summary>
        /// <param name="handler">Update Handler</param>
        /// <param name="args">Arguments</param>
        private Task InvokeMethod(UpdateHandler handler, object?[] args)
            => Method.Invoke(handler, BindingFlags.DoNotWrapExceptions, null, args, null) as Task ?? Task.CompletedTask;
    }
}
