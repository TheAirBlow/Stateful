using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TheAirBlow.Stateful.Conditions;

namespace TheAirBlow.Stateful;

/// <summary>
/// Various helper extensions
/// </summary>
public static class Extensions {
    /// <summary>
    /// Returns the message this update is about, if any
    /// </summary>
    /// <param name="update">Update</param>
    /// <returns>Message, null if none</returns>
    private static Message? GetMessage(this Update update)
        => update.Message ?? update.EditedMessage ?? update.ChannelPost
           ?? update.EditedChannelPost ?? update.CallbackQuery?.Message;

    /// <summary>
    /// Returns the chat this update happened in, if any
    /// </summary>
    /// <param name="update">Update</param>
    /// <returns>Chat, null if none</returns>
    private static Chat? GetChat(this Update update)
        => update.GetMessage()?.Chat ?? update.ChatMember?.Chat
           ?? update.MyChatMember?.Chat ?? update.ChatJoinRequest?.Chat;

    /// <summary>
    /// Checks if the update happened in a private chat
    /// </summary>
    /// <param name="update">Update</param>
    /// <returns>True if private chat</returns>
    public static bool IsPrivateChat(this Update update)
        => update.GetChat()?.Type == ChatType.Private;
    
    /// <summary>
    /// Get Chat ID from update
    /// </summary>
    /// <param name="update">Update</param>
    /// <returns>Chat ID</returns>
    public static long? GetChatId(this Update update)
        => update.GetChat()?.Id;
    
    /// <summary>
    /// Get User ID from update
    /// </summary>
    /// <param name="update">Update</param>
    /// <returns>User ID</returns>
    public static long? GetUserId(this Update update)
        => update.CallbackQuery?.From.Id ?? update.InlineQuery?.From.Id
           ?? update.ChosenInlineResult?.From.Id ?? update.ShippingQuery?.From.Id
           ?? update.PreCheckoutQuery?.From.Id ?? update.PollAnswer?.User?.Id
           ?? update.ChatJoinRequest?.From.Id ?? update.ChatMember?.From.Id
           ?? update.MyChatMember?.From.Id ?? update.Message?.From?.Id
           ?? update.EditedMessage?.From?.Id ?? update.ChannelPost?.From?.Id
           ?? update.EditedChannelPost?.From?.Id;
    
    /// <summary>
    /// Get Message ID from update
    /// </summary>
    /// <param name="update">Update</param>
    /// <returns>Message ID</returns>
    public static int? GetMessageId(this Update update)
        => update.GetMessage()?.MessageId;

    /// <summary>
    /// Stores the handler's state for a message it just sent or edited, and makes it the handler's current state.
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="handler">Update Handler</param>
    public static async Task<Message> PutState(this Task<Message> message, UpdateHandler handler) {
        var msg = await message;
        await PutState(handler, handler.State, msg);
        return msg;
    }
    
    /// <summary>
    /// Stores the handler's state for messages it just sent
    /// </summary>
    /// <param name="messages">Messages</param>
    /// <param name="handler">Update Handler</param>
    public static async Task<Message[]> PutState(this Task<Message[]> messages, UpdateHandler handler) {
        var msgs = await messages;
        var source = handler.State;
        foreach (var msg in msgs)
            await PutState(handler, source, msg);
        return msgs;
    }

    /// <summary>
    /// Stores state for a message
    /// </summary>
    /// <param name="handler">Update Handler</param>
    /// <param name="source">State to take values from</param>
    /// <param name="msg">Message</param>
    private static async Task PutState(UpdateHandler handler, MessageState source, Message msg) {
        var stateHandler = handler.Stateful.Options.StateHandler;
        if (stateHandler == null) return;
        var state = source;
        if (source.ChatId != msg.Chat.Id || source.MessageId != msg.MessageId) {
            state = await stateHandler.GetState(msg);
            state.HandlerId = source.HandlerId;
            state.SubMenu = source.SubMenu;
            state.State = new Dictionary<string, string>(source.State);
            foreach (var key in source.PendingLocal)
                if (source.LocalState.TryGetValue(key, out var value))
                    state.LocalState[key] = value;
        }

        state.LastUpdated = DateTime.UtcNow;
        await stateHandler.Update(state);
        handler.State = state;
    }
    
    /// <summary>
    /// Checks if all conditions match
    /// </summary>
    /// <param name="attrs">Handler conditions</param>
    /// <param name="handler">Update handler</param>
    /// <returns>True if matches</returns>
    internal static async ValueTask<bool> MatchAsync(this HandlerAttribute[] attrs, UpdateHandler handler) {
        foreach (var attr in attrs)
            if (!await attr.MatchAsync(handler)) return false;
        return true;
    }

    /// <summary>
    /// Checks if all conditions match, blocking until they are checked.
    /// </summary>
    /// <param name="attrs">Handler conditions</param>
    /// <param name="handler">Update handler</param>
    /// <returns>True if matches</returns>
    internal static bool Match(this HandlerAttribute[] attrs, UpdateHandler handler)
        => attrs.All(attr => attr.MatchAsync(handler).GetAwaiter().GetResult());
}
