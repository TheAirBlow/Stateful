using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TheAirBlow.Stateful;

public partial class UpdateHandler {
    /// <summary>
    /// Edits current message if the update was a callback query, otherwise sends a new one.
    /// </summary>
    /// <param name="text">Text of the message, 1-4096 characters after entities parsing</param>
    /// <param name="replyMarkup">An inline keyboard</param>
    /// <param name="chatId">Unique identifier for the target chat. Taken from the handled update if not specified.</param>
    /// <param name="messageId">Identifier of the message to edit. Taken from the handled update if not specified.</param>
    /// <param name="parseMode">Mode for parsing entities in the message text</param>
    /// <param name="replyParameters">Description of the message to reply to, only used when sending</param>
    /// <param name="linkPreviewOptions">Link preview generation options for the message</param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum, only used when sending</param>
    /// <param name="entities">A list of special entities that appear in message text, which can be specified instead of <paramref name="parseMode"/></param>
    /// <param name="disableNotification">Sends the message silently, only used when sending</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving, only used when sending</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message, only used when sending</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent or edited</param>
    /// <param name="allowPaidBroadcast">Allow up to 1000 messages per second for a fee of 0.1 Telegram Stars per message, only used when sending</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent or edited message, null if it was not modified.</returns>
    public async Task<Message?> SendOrEditMessage(
        string text,
        InlineKeyboardMarkup? replyMarkup = null,
        ChatId? chatId = null,
        MessageId? messageId = null,
        ParseMode parseMode = ParseMode.Markdown,
        ReplyParameters? replyParameters = null,
        LinkPreviewOptions? linkPreviewOptions = null,
        int? messageThreadId = null,
        IEnumerable<MessageEntity>? entities = null,
        bool disableNotification = false,
        bool protectContent = false,
        string? messageEffectId = null,
        string? businessConnectionId = null,
        bool allowPaidBroadcast = false,
        CancellationToken cancellationToken = default) {
        chatId ??= ChatId; messageId ??= MessageId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "failed to infer chat ID");
        if (Update.Type == UpdateType.CallbackQuery) {
            if (messageId == null) throw new ArgumentNullException(nameof(messageId), "failed to infer message ID");
            return await EditMessage(text, replyMarkup, chatId, messageId, parseMode, linkPreviewOptions: linkPreviewOptions,
                entities: entities, businessConnectionId: businessConnectionId, cancellationToken: cancellationToken);
        }

        return await SendMessage(text, replyMarkup, chatId, parseMode, replyParameters, linkPreviewOptions, messageThreadId,
            entities, disableNotification, protectContent, messageEffectId, businessConnectionId, allowPaidBroadcast,
            cancellationToken: cancellationToken);
    }
}