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
    public void Register<[DynamicallyAccessedMembers(Members)] T>(string? id = null) where T : UpdateHandler, new()
        => Handlers.Add(new HandlerWrapper(Options, typeof(T), static () => new T(), id));

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
        var matcher = CreateHandler(bot, update);
        
        foreach (var filter in Options.Filters)
            if (!filter.RequiresState && !await filter.MatchAsync(matcher)) return;
        if (Options.StateHandler != null && update.GetChatId() != null && update.GetMessageId() != null)
            matcher.State = await Options.StateHandler.GetState(update);
        foreach (var filter in Options.Filters)
            if (filter.RequiresState && !await filter.MatchAsync(matcher)) return;

        var method = await GetMethod(matcher);
        if (method == null) return;
        if (update.Type == UpdateType.CallbackQuery && Options.AnswerCallbackQueries && !method.AnswersQuery)
            try {
                await bot.AnswerCallbackQuery(update.CallbackQuery!.Id, cancellationToken: token);
            } catch (ApiRequestException) {
                // ignore
            }

        var handler = CreateHandler(bot, update, matcher.State, method.Create);
        handler.ParsedCommand = matcher.ParsedCommand;
        handler.CommandParsed = matcher.CommandParsed;

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
    /// <returns>Handler Method</returns>
    private async Task<MethodWrapper?> GetMethod(UpdateHandler handler) {
        var type = handler.Update.Type;
        if (handler.State.Expired)
            foreach (var wrapper in Handlers) {
                if (!wrapper.IsAvailable(handler.State) || !await wrapper.Conditions.MatchAsync(handler)) continue;
                foreach (var expired in wrapper.Expired)
                    if (await expired.Conditions.MatchAsync(handler)) return expired;
            }

        foreach (var wrapper in Handlers) {
            if (!wrapper.IsAvailable(handler.State) || !await wrapper.Conditions.MatchAsync(handler)) continue;
            var method = await wrapper.Find(handler);
            if (method == null && type != UpdateType.CallbackQuery && (wrapper.PrivateOnly || Options.PrivateOnly))
                method = await GetDefault(wrapper, handler);
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
    /// <returns>Default Handler</returns>
    private static async Task<MethodWrapper?> GetDefault(HandlerWrapper wrapper, UpdateHandler handler) {
        foreach (var method in wrapper.Defaults)
            if (await method.Conditions.MatchAsync(handler)) return method;
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
        /// <returns>Method, null if none</returns>
        public async Task<MethodWrapper?> Find(UpdateHandler handler) {
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
                if (await method.Conditions.MatchAsync(handler)) return method;
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
