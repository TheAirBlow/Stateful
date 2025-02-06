using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TheAirBlow.Stateful.Conditions;

namespace TheAirBlow.Stateful;

/// <summary>
/// Various helper extensions
/// </summary>
public static class Extensions {
    /// <summary>
    /// Checks if the update happened in a private chat
    /// </summary>
    /// <param name="update">Update</param>
    /// <returns>Chat ID</returns>
    public static bool IsPrivateChat(this Update update)
        => update.Type switch {
            UpdateType.CallbackQuery => update.CallbackQuery!.Message!.Chat.Type == ChatType.Private,
            UpdateType.EditedMessage => update.EditedMessage!.Chat.Type == ChatType.Private,
            UpdateType.ChannelPost => update.ChannelPost!.Chat.Type == ChatType.Private,
            UpdateType.ChatMember => update.ChannelPost!.Chat.Type == ChatType.Private,
            UpdateType.Message => update.Message!.Chat.Type == ChatType.Private,
            _ => false
        };
    
    /// <summary>
    /// Get Chat ID from update
    /// </summary>
    /// <param name="update">Update</param>
    /// <returns>Chat ID</returns>
    public static long? GetChatId(this Update update)
        => update.Type switch {
            UpdateType.CallbackQuery => update.CallbackQuery!.Message!.Chat.Id,
            UpdateType.EditedMessage => update.EditedMessage!.Chat.Id,
            UpdateType.ChannelPost => update.ChannelPost!.Chat.Id,
            UpdateType.ChatMember => update.ChannelPost!.Chat.Id,
            UpdateType.Message => update.Message!.Chat.Id,
            _ => null
        };
    
    /// <summary>
    /// Get User ID from update
    /// </summary>
    /// <param name="update">Update</param>
    /// <returns>User ID</returns>
    public static long? GetUserId(this Update update)
        => update.Type switch {
            UpdateType.EditedChannelPost => update.EditedChannelPost!.From!.Id,
            UpdateType.PreCheckoutQuery => update.PreCheckoutQuery!.From.Id,
            UpdateType.ChatJoinRequest => update.ChatJoinRequest!.From.Id,
            UpdateType.EditedMessage => update.EditedMessage!.From!.Id,
            UpdateType.CallbackQuery => update.CallbackQuery!.From.Id,
            UpdateType.ShippingQuery => update.ShippingQuery!.From.Id,
            UpdateType.ChannelPost => update.ChannelPost!.From!.Id,
            UpdateType.ChatMember => update.ChannelPost!.From!.Id,
            UpdateType.InlineQuery => update.InlineQuery!.From.Id,
            UpdateType.PollAnswer => update.PollAnswer!.User!.Id,
            UpdateType.Message => update.Message!.From!.Id,
            _ => null
        };
    
    /// <summary>
    /// Get Message ID from update
    /// </summary>
    /// <param name="update">Update</param>
    /// <returns>Message ID</returns>
    public static int? GetMessageId(this Update update)
        => update.Type switch {
            UpdateType.CallbackQuery => update.CallbackQuery!.Message!.MessageId,
            UpdateType.EditedChannelPost => update.EditedChannelPost!.MessageId,
            UpdateType.EditedMessage => update.EditedMessage!.MessageId,
            UpdateType.ChannelPost => update.ChannelPost!.MessageId,
            UpdateType.ChatMember => update.ChannelPost!.MessageId,
            UpdateType.Message => update.Message!.MessageId,
            _ => null
        };

    /// <summary>
    /// Put state data
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="handler">Update Handler</param>
    public static async Task<Message> PutState(this Task<Message> message, UpdateHandler handler) {
        var msg = await message;
        var stateHandler = handler.Stateful.Options.StateHandler;
        if (stateHandler == null) return msg;
        var state = await stateHandler.GetState(msg);
        state.HandlerId = handler.State.HandlerId;
        state.State = handler.State.State;
        state.LastUpdated = DateTime.UtcNow;
        await stateHandler.Update(state);
        handler.State = state;
        return msg;
    }
    
    /// <summary>
    /// Put state data
    /// </summary>
    /// <param name="messages">Messages</param>
    /// <param name="handler">Update Handler</param>
    public static async Task<Message[]> PutState(this Task<Message[]> messages, UpdateHandler handler) {
        var msgs = await messages;
        var stateHandler = handler.Stateful.Options.StateHandler;
        if (stateHandler == null) return msgs;
        foreach (var msg in msgs) {
            var state = await stateHandler.GetState(msg);
            state.HandlerId = handler.State.HandlerId;
            state.State = handler.State.State;
            state.LastUpdated = DateTime.UtcNow;
            await stateHandler.Update(state);
            handler.State = state;
        }

        return msgs;
    }
    
    /// <summary>
    /// Awaits an object if it's a task
    /// </summary>
    /// <param name="obj">Object</param>
    internal static async Task AwaitIfTask(this object? obj) {
        if (obj is Task task) await task;
    }

    /// <summary>
    /// Checks if all conditions match
    /// </summary>
    /// <param name="attrs">Handler conditions</param>
    /// <param name="handler">Update handler</param>
    /// <param name="allowEmpty">Allow empty</param>
    /// <returns>True if matches</returns>
    internal static bool Match(this HandlerAttribute[] attrs, UpdateHandler handler, bool allowEmpty = true)
        => (allowEmpty && attrs.Length == 0) || attrs.All(attr => attr.MatchAsync(handler).GetAwaiter().GetResult());
}