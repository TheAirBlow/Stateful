using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;
using Telegram.Bot.Types.ReplyMarkups;

namespace TheAirBlow.Stateful;

/// <summary>
/// Extension methods that integrate Bot APIs with Stateful
/// </summary>
public partial class UpdateHandler {
    /// <summary>
    /// Use this method to send text messages.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="text">Text of the message to be sent, 1-4096 characters after entities parsing</param>
    /// <param name="parseMode">Mode for parsing entities in the message text. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details.</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</param>
    /// <param name="linkPreviewOptions">Link preview generation options for the message</param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</param>
    /// <param name="entities">A list of special entities that appear in message text, which can be specified instead of <paramref name="parseMode"/></param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public async Task<Message> SendMessage(
        string text,
        IReplyMarkup? replyMarkup = default,
        ChatId? chatId = default,
        ParseMode parseMode = ParseMode.Markdown,
        ReplyParameters? replyParameters = default,
        LinkPreviewOptions? linkPreviewOptions = default,
        int? messageThreadId = default,
        IEnumerable<MessageEntity>? entities = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        var msg = await Client.SendMessage(chatId, text, parseMode, replyParameters, replyMarkup, linkPreviewOptions, messageThreadId, entities, disableNotification, protectContent, messageEffectId, businessConnectionId, allowPaidBroadcast, cancellationToken).PutState(this);
        await SaveState();
        return msg;
    }

    /// <summary>
    /// Use this method to send photos.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="photo">Photo to send. Pass a FileId as String to send a photo that exists on the Telegram servers (recommended), pass an HTTP URL as a String for Telegram to get a photo from the Internet, or upload a new photo using <see cref="InputFileStream"/>. The photo must be at most 10 MB in size. The photo's width and height must not exceed 10000 in total. Width and height ratio must be at most 20. <a href="https://core.telegram.org/bots/api#sending-files">More information on Sending Files »</a></param>
    /// <param name="caption">Photo caption (may also be used when resending photos by <em>FileId</em>), 0-1024 characters after entities parsing</param>
    /// <param name="parseMode">Mode for parsing entities in the photo caption. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details.</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</param>
    /// <param name="captionEntities">A list of special entities that appear in the caption, which can be specified instead of <paramref name="parseMode"/></param>
    /// <param name="showCaptionAboveMedia">Pass <see langword="true"/>, if the caption must be shown above the message media</param>
    /// <param name="hasSpoiler">Pass <see langword="true"/> if the photo needs to be covered with a spoiler animation</param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public async Task<Message> SendPhoto(
        InputFile photo,
        IReplyMarkup? replyMarkup = default,
        ChatId? chatId = default,
        string? caption = default,
        ParseMode parseMode = ParseMode.Markdown,
        ReplyParameters? replyParameters = default,
        int? messageThreadId = default,
        IEnumerable<MessageEntity>? captionEntities = default,
        bool showCaptionAboveMedia = default,
        bool hasSpoiler = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        var msg = await Client.SendPhoto(chatId, photo, caption, parseMode, replyParameters, replyMarkup, messageThreadId, captionEntities, showCaptionAboveMedia, hasSpoiler, disableNotification, protectContent, messageEffectId, businessConnectionId, allowPaidBroadcast, cancellationToken).PutState(this);
        await SaveState();
        return msg;
    }

    /// <summary>
    /// Use this method to send audio files, if you want Telegram clients to display them in the music player. Your audio must be in the .MP3 or .M4A format.
    /// </summary>
    /// <remarks>
    /// Bots can currently send audio files of up to 50 MB in size, this limit may be changed in the future.<br/>
    /// For sending voice messages, use the <see cref="TelegramBotClientExtensions.SendVoice">SendVoice</see> method instead.
    /// </remarks>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="audio">Audio file to send. Pass a FileId as String to send an audio file that exists on the Telegram servers (recommended), pass an HTTP URL as a String for Telegram to get an audio file from the Internet, or upload a new one using <see cref="InputFileStream"/>. <a href="https://core.telegram.org/bots/api#sending-files">More information on Sending Files »</a></param>
    /// <param name="caption">Audio caption, 0-1024 characters after entities parsing</param>
    /// <param name="parseMode">Mode for parsing entities in the audio caption. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details.</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</param>
    /// <param name="duration">Duration of the audio in seconds</param>
    /// <param name="performer">Performer</param>
    /// <param name="title">Track name</param>
    /// <param name="thumbnail">Thumbnail of the file sent; can be ignored if thumbnail generation for the file is supported server-side. The thumbnail should be in JPEG format and less than 200 kB in size. A thumbnail's width and height should not exceed 320. Ignored if the file is not uploaded using <see cref="InputFileStream"/>. Thumbnails can't be reused and can be only uploaded as a new file, so you can use <see cref="InputFileStream(Stream, string?)"/> with a specific filename. <a href="https://core.telegram.org/bots/api#sending-files">More information on Sending Files »</a></param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</param>
    /// <param name="captionEntities">A list of special entities that appear in the caption, which can be specified instead of <paramref name="parseMode"/></param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public async Task<Message> SendAudio(
        InputFile audio,
        IReplyMarkup? replyMarkup = default,
        ChatId? chatId = default,
        string? caption = default,
        ParseMode parseMode = ParseMode.Markdown,
        ReplyParameters? replyParameters = default,
        int? duration = default,
        string? performer = default,
        string? title = default,
        InputFile? thumbnail = default,
        int? messageThreadId = default,
        IEnumerable<MessageEntity>? captionEntities = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        var msg = await Client.SendAudio(chatId, audio, caption, parseMode, replyParameters, replyMarkup, duration, performer, title, thumbnail, messageThreadId, captionEntities, disableNotification, protectContent, messageEffectId, businessConnectionId, allowPaidBroadcast, cancellationToken).PutState(this);
        await SaveState();
        return msg;
    }

    /// <summary>
    /// Use this method to send general files.
    /// </summary>
    /// <remarks>
    /// Bots can currently send files of any type of up to 50 MB in size, this limit may be changed in the future.
    /// </remarks>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="document">File to send. Pass a FileId as String to send a file that exists on the Telegram servers (recommended), pass an HTTP URL as a String for Telegram to get a file from the Internet, or upload a new one using <see cref="InputFileStream"/>. <a href="https://core.telegram.org/bots/api#sending-files">More information on Sending Files »</a></param>
    /// <param name="caption">Document caption (may also be used when resending documents by <em>FileId</em>), 0-1024 characters after entities parsing</param>
    /// <param name="parseMode">Mode for parsing entities in the document caption. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details.</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</param>
    /// <param name="thumbnail">Thumbnail of the file sent; can be ignored if thumbnail generation for the file is supported server-side. The thumbnail should be in JPEG format and less than 200 kB in size. A thumbnail's width and height should not exceed 320. Ignored if the file is not uploaded using <see cref="InputFileStream"/>. Thumbnails can't be reused and can be only uploaded as a new file, so you can use <see cref="InputFileStream(Stream, string?)"/> with a specific filename. <a href="https://core.telegram.org/bots/api#sending-files">More information on Sending Files »</a></param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</param>
    /// <param name="captionEntities">A list of special entities that appear in the caption, which can be specified instead of <paramref name="parseMode"/></param>
    /// <param name="disableContentTypeDetection">Disables automatic server-side content type detection for files uploaded using <see cref="InputFileStream"/></param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public async Task<Message> SendDocument(
        InputFile document,
        IReplyMarkup? replyMarkup = default,
        ChatId? chatId = default,
        string? caption = default,
        ParseMode parseMode = ParseMode.Markdown,
        ReplyParameters? replyParameters = default,
        InputFile? thumbnail = default,
        int? messageThreadId = default,
        IEnumerable<MessageEntity>? captionEntities = default,
        bool disableContentTypeDetection = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        var msg = await Client.SendDocument(chatId, document, caption, parseMode, replyParameters, replyMarkup, thumbnail, messageThreadId, captionEntities, disableContentTypeDetection, disableNotification, protectContent, messageEffectId, businessConnectionId, allowPaidBroadcast, cancellationToken).PutState(this);
        await SaveState();
        return msg;
    }

    /// <summary>
    /// Use this method to send video files, Telegram clients support MPEG4 videos (other formats may be sent as <see cref="Document"/>).
    /// </summary>
    /// <remarks>
    /// Bots can currently send video files of up to 50 MB in size, this limit may be changed in the future.
    /// </remarks>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="video">Video to send. Pass a FileId as String to send a video that exists on the Telegram servers (recommended), pass an HTTP URL as a String for Telegram to get a video from the Internet, or upload a new video using <see cref="InputFileStream"/>. <a href="https://core.telegram.org/bots/api#sending-files">More information on Sending Files »</a></param>
    /// <param name="caption">Video caption (may also be used when resending videos by <em>FileId</em>), 0-1024 characters after entities parsing</param>
    /// <param name="parseMode">Mode for parsing entities in the video caption. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details.</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</param>
    /// <param name="duration">Duration of sent video in seconds</param>
    /// <param name="width">Video width</param>
    /// <param name="height">Video height</param>
    /// <param name="thumbnail">Thumbnail of the file sent; can be ignored if thumbnail generation for the file is supported server-side. The thumbnail should be in JPEG format and less than 200 kB in size. A thumbnail's width and height should not exceed 320. Ignored if the file is not uploaded using <see cref="InputFileStream"/>. Thumbnails can't be reused and can be only uploaded as a new file, so you can use <see cref="InputFileStream(Stream, string?)"/> with a specific filename. <a href="https://core.telegram.org/bots/api#sending-files">More information on Sending Files »</a></param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</param>
    /// <param name="captionEntities">A list of special entities that appear in the caption, which can be specified instead of <paramref name="parseMode"/></param>
    /// <param name="showCaptionAboveMedia">Pass <see langword="true"/>, if the caption must be shown above the message media</param>
    /// <param name="hasSpoiler">Pass <see langword="true"/> if the video needs to be covered with a spoiler animation</param>
    /// <param name="supportsStreaming">Pass <see langword="true"/> if the uploaded video is suitable for streaming</param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public async Task<Message> SendVideo(
        InputFile video,
        IReplyMarkup? replyMarkup = default,
        ChatId? chatId = default,
        string? caption = default,
        ParseMode parseMode = ParseMode.Markdown,
        ReplyParameters? replyParameters = default,
        int? duration = default,
        int? width = default,
        int? height = default,
        InputFile? thumbnail = default,
        int? messageThreadId = default,
        IEnumerable<MessageEntity>? captionEntities = default,
        bool showCaptionAboveMedia = default,
        bool hasSpoiler = default,
        bool supportsStreaming = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        var msg = await Client.SendVideo(chatId, video, caption, parseMode, replyParameters, replyMarkup, duration, width, height, thumbnail, messageThreadId, captionEntities, showCaptionAboveMedia, hasSpoiler, supportsStreaming, disableNotification, protectContent, messageEffectId, businessConnectionId, allowPaidBroadcast, cancellationToken).PutState(this);
        await SaveState();
        return msg;
    }

    /// <summary>
    /// Use this method to send animation files (GIF or H.264/MPEG-4 AVC video without sound).
    /// </summary>
    /// <remarks>
    /// Bots can currently send animation files of up to 50 MB in size, this limit may be changed in the future.
    /// </remarks>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="animation">Animation to send. Pass a FileId as String to send an animation that exists on the Telegram servers (recommended), pass an HTTP URL as a String for Telegram to get an animation from the Internet, or upload a new animation using <see cref="InputFileStream"/>. <a href="https://core.telegram.org/bots/api#sending-files">More information on Sending Files »</a></param>
    /// <param name="caption">Animation caption (may also be used when resending animation by <em>FileId</em>), 0-1024 characters after entities parsing</param>
    /// <param name="parseMode">Mode for parsing entities in the animation caption. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details.</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</param>
    /// <param name="duration">Duration of sent animation in seconds</param>
    /// <param name="width">Animation width</param>
    /// <param name="height">Animation height</param>
    /// <param name="thumbnail">Thumbnail of the file sent; can be ignored if thumbnail generation for the file is supported server-side. The thumbnail should be in JPEG format and less than 200 kB in size. A thumbnail's width and height should not exceed 320. Ignored if the file is not uploaded using <see cref="InputFileStream"/>. Thumbnails can't be reused and can be only uploaded as a new file, so you can use <see cref="InputFileStream(Stream, string?)"/> with a specific filename. <a href="https://core.telegram.org/bots/api#sending-files">More information on Sending Files »</a></param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</param>
    /// <param name="captionEntities">A list of special entities that appear in the caption, which can be specified instead of <paramref name="parseMode"/></param>
    /// <param name="showCaptionAboveMedia">Pass <see langword="true"/>, if the caption must be shown above the message media</param>
    /// <param name="hasSpoiler">Pass <see langword="true"/> if the animation needs to be covered with a spoiler animation</param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public async Task<Message> SendAnimation(
        InputFile animation,
        IReplyMarkup? replyMarkup = default,
        ChatId? chatId = default,
        string? caption = default,
        ParseMode parseMode = ParseMode.Markdown,
        ReplyParameters? replyParameters = default,
        int? duration = default,
        int? width = default,
        int? height = default,
        InputFile? thumbnail = default,
        int? messageThreadId = default,
        IEnumerable<MessageEntity>? captionEntities = default,
        bool showCaptionAboveMedia = default,
        bool hasSpoiler = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        var msg = await Client.SendAnimation(chatId, animation, caption, parseMode, replyParameters, replyMarkup, duration, width, height, thumbnail, messageThreadId, captionEntities, showCaptionAboveMedia, hasSpoiler, disableNotification, protectContent, messageEffectId, businessConnectionId, allowPaidBroadcast, cancellationToken).PutState(this);
        await SaveState();
        return msg;
    }

    /// <summary>
    /// Use this method to send audio files, if you want Telegram clients to display the file as a playable voice message. For this to work, your audio must be in an .OGG file encoded with OPUS, or in .MP3 format, or in .M4A format (other formats may be sent as <see cref="Audio"/> or <see cref="Document"/>).
    /// </summary>
    /// <remarks>
    /// Bots can currently send voice messages of up to 50 MB in size, this limit may be changed in the future.
    /// </remarks>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="voice">Audio file to send. Pass a FileId as String to send a file that exists on the Telegram servers (recommended), pass an HTTP URL as a String for Telegram to get a file from the Internet, or upload a new one using <see cref="InputFileStream"/>. <a href="https://core.telegram.org/bots/api#sending-files">More information on Sending Files »</a></param>
    /// <param name="caption">Voice message caption, 0-1024 characters after entities parsing</param>
    /// <param name="parseMode">Mode for parsing entities in the voice message caption. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details.</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</param>
    /// <param name="duration">Duration of the voice message in seconds</param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</param>
    /// <param name="captionEntities">A list of special entities that appear in the caption, which can be specified instead of <paramref name="parseMode"/></param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public async Task<Message> SendVoice(
        InputFile voice,
        IReplyMarkup? replyMarkup = default,
        ChatId? chatId = default,
        string? caption = default,
        ParseMode parseMode = ParseMode.Markdown,
        ReplyParameters? replyParameters = default,
        int? duration = default,
        int? messageThreadId = default,
        IEnumerable<MessageEntity>? captionEntities = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        var msg = await Client.SendVoice(chatId, voice, caption, parseMode, replyParameters, replyMarkup, duration, messageThreadId, captionEntities, disableNotification, protectContent, messageEffectId, businessConnectionId, allowPaidBroadcast, cancellationToken).PutState(this);
        await SaveState();
        return msg;
    }

    /// <summary>
    /// As of <a href="https://telegram.org/blog/video-messages-and-telescope">v.4.0</a>, Telegram clients support rounded square MPEG4 videos of up to 1 minute long. Use this method to send video messages.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="videoNote">Video note to send. Pass a FileId as String to send a video note that exists on the Telegram servers (recommended) or upload a new video using <see cref="InputFileStream"/>. <a href="https://core.telegram.org/bots/api#sending-files">More information on Sending Files »</a>. Sending video notes by a URL is currently unsupported</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</param>
    /// <param name="duration">Duration of sent video in seconds</param>
    /// <param name="length">Video width and height, i.e. diameter of the video message</param>
    /// <param name="thumbnail">Thumbnail of the file sent; can be ignored if thumbnail generation for the file is supported server-side. The thumbnail should be in JPEG format and less than 200 kB in size. A thumbnail's width and height should not exceed 320. Ignored if the file is not uploaded using <see cref="InputFileStream"/>. Thumbnails can't be reused and can be only uploaded as a new file, so you can use <see cref="InputFileStream(Stream, string?)"/> with a specific filename. <a href="https://core.telegram.org/bots/api#sending-files">More information on Sending Files »</a></param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public async Task<Message> SendVideoNote(
        InputFile videoNote,
        IReplyMarkup? replyMarkup = default,
        ChatId? chatId = default,
        ReplyParameters? replyParameters = default,
        int? duration = default,
        int? length = default,
        InputFile? thumbnail = default,
        int? messageThreadId = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        var msg = await Client.SendVideoNote(chatId, videoNote, replyParameters, replyMarkup, duration, length, thumbnail, messageThreadId, disableNotification, protectContent, messageEffectId, businessConnectionId, allowPaidBroadcast, cancellationToken).PutState(this);
        await SaveState();
        return msg;
    }

    /// <summary>
    /// Use this method to send paid media.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>). If the chat is a channel, all Telegram Star proceeds from this media will be credited to the chat's balance. Otherwise, they will be credited to the bot's balance.</param>
    /// <param name="starCount">The number of Telegram Stars that must be paid to buy access to the media; 1-2500</param>
    /// <param name="media">A array describing the media to be sent; up to 10 items</param>
    /// <param name="caption">Media caption, 0-1024 characters after entities parsing</param>
    /// <param name="parseMode">Mode for parsing entities in the media caption. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details.</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</param>
    /// <param name="payload">Bot-defined paid media payload, 0-128 bytes. This will not be displayed to the user, use it for your internal processes.</param>
    /// <param name="captionEntities">A list of special entities that appear in the caption, which can be specified instead of <paramref name="parseMode"/></param>
    /// <param name="showCaptionAboveMedia">Pass <see langword="true"/>, if the caption must be shown above the message media</param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public async Task<Message> SendPaidMedia(
        int starCount,
        IEnumerable<InputPaidMedia> media,
        IReplyMarkup? replyMarkup = default,
        ChatId? chatId = default,
        string? caption = default,
        ParseMode parseMode = ParseMode.Markdown,
        ReplyParameters? replyParameters = default,
        string? payload = default,
        IEnumerable<MessageEntity>? captionEntities = default,
        bool showCaptionAboveMedia = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        var msg = await Client.SendPaidMedia(chatId, starCount, media, caption, parseMode, replyParameters, replyMarkup, payload, captionEntities, showCaptionAboveMedia, disableNotification, protectContent, businessConnectionId, allowPaidBroadcast, cancellationToken).PutState(this);
        await SaveState();
        return msg;
    }

    /// <summary>
    /// Use this method to send a group of photos, videos, documents or audios as an album. Documents and audio files can be only grouped in an album with messages of the same type.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="media">A array describing messages to be sent, must include 2-10 items</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</param>
    /// <param name="disableNotification">Sends messages <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent messages from forwarding and saving</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>An array of <see cref="Message">Messages</see> that were sent is returned.</returns>
    public async Task<Message[]> SendMediaGroup(
        IEnumerable<IAlbumInputMedia> media,
        ChatId? chatId = default,
        ReplyParameters? replyParameters = default,
        int? messageThreadId = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        var msg = await Client.SendMediaGroup(chatId, media, replyParameters, messageThreadId, disableNotification, protectContent, messageEffectId, businessConnectionId, allowPaidBroadcast, cancellationToken).PutState(this);
        await SaveState();
        return msg;
    }

    /// <summary>
    /// Use this method to send point on the map.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="latitude">Latitude of the location</param>
    /// <param name="longitude">Longitude of the location</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</param>
    /// <param name="horizontalAccuracy">The radius of uncertainty for the location, measured in meters; 0-1500</param>
    /// <param name="livePeriod">Period in seconds during which the location will be updated (see <a href="https://telegram.org/blog/live-locations">Live Locations</a>, should be between 60 and 86400, or 0x7FFFFFFF for live locations that can be edited indefinitely.</param>
    /// <param name="heading">For live locations, a direction in which the user is moving, in degrees. Must be between 1 and 360 if specified.</param>
    /// <param name="proximityAlertRadius">For live locations, a maximum distance for proximity alerts about approaching another chat member, in meters. Must be between 1 and 100000 if specified.</param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public async Task<Message> SendLocation(
        double latitude,
        double longitude,
        ChatId? chatId = default,
        IReplyMarkup? replyMarkup = default,
        ReplyParameters? replyParameters = default,
        double? horizontalAccuracy = default,
        int? livePeriod = default,
        int? heading = default,
        int? proximityAlertRadius = default,
        int? messageThreadId = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        var msg = await Client.SendLocation(chatId, latitude, longitude, replyParameters, replyMarkup, horizontalAccuracy, livePeriod, heading, proximityAlertRadius, messageThreadId, disableNotification, protectContent, messageEffectId, businessConnectionId, allowPaidBroadcast, cancellationToken).PutState(this);
        await SaveState();
        return msg;
    }

    /// <summary>
    /// Use this method to send information about a venue.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="latitude">Latitude of the venue</param>
    /// <param name="longitude">Longitude of the venue</param>
    /// <param name="title">Name of the venue</param>
    /// <param name="address">Address of the venue</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</param>
    /// <param name="foursquareId">Foursquare identifier of the venue</param>
    /// <param name="foursquareType">Foursquare type of the venue, if known. (For example, “arts_entertainment/default”, “arts_entertainment/aquarium” or “food/icecream”.)</param>
    /// <param name="googlePlaceId">Google Places identifier of the venue</param>
    /// <param name="googlePlaceType">Google Places type of the venue. (See <a href="https://developers.google.com/places/web-service/supported_types">supported types</a>.)</param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public async Task<Message> SendVenue(
        double latitude,
        double longitude,
        string title,
        string address,
        ChatId? chatId = default,
        IReplyMarkup? replyMarkup = default,
        ReplyParameters? replyParameters = default,
        string? foursquareId = default,
        string? foursquareType = default,
        string? googlePlaceId = default,
        string? googlePlaceType = default,
        int? messageThreadId = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        var msg = await Client.SendVenue(chatId, latitude, longitude, title, address, replyParameters, replyMarkup, foursquareId, foursquareType, googlePlaceId, googlePlaceType, messageThreadId, disableNotification, protectContent, messageEffectId, businessConnectionId, allowPaidBroadcast, cancellationToken).PutState(this);
        await SaveState();
        return msg;
    }

    /// <summary>
    /// Use this method to send phone contacts.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="phoneNumber">Contact's phone number</param>
    /// <param name="firstName">Contact's first name</param>
    /// <param name="lastName">Contact's last name</param>
    /// <param name="vcard">Additional data about the contact in the form of a <a href="https://en.wikipedia.org/wiki/VCard">vCard</a>, 0-2048 bytes</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public async Task<Message> SendContact(
        string phoneNumber,
        string firstName,
        IReplyMarkup? replyMarkup = default,
        ChatId? chatId = default,
        string? lastName = default,
        string? vcard = default,
        ReplyParameters? replyParameters = default,
        int? messageThreadId = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        var msg = await Client.SendContact(chatId, phoneNumber, firstName, lastName, vcard, replyParameters, replyMarkup, messageThreadId, disableNotification, protectContent, messageEffectId, businessConnectionId, allowPaidBroadcast, cancellationToken).PutState(this);
        await SaveState();
        return msg;
    }

    /// <summary>
    /// Use this method to send a native poll.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="question">Poll question, 1-300 characters</param>
    /// <param name="options">A list of 2-10 answer options</param>
    /// <param name="isAnonymous"><see langword="true"/>, if the poll needs to be anonymous, defaults to <see langword="true"/></param>
    /// <param name="type">Poll type, <see cref="PollType.Quiz">Quiz</see> or <see cref="PollType.Regular">Regular</see>, defaults to <see cref="PollType.Regular">Regular</see></param>
    /// <param name="allowsMultipleAnswers"><see langword="true"/>, if the poll allows multiple answers, ignored for polls in quiz mode, defaults to <see langword="false"/></param>
    /// <param name="correctOptionId">0-based identifier of the correct answer option, required for polls in quiz mode</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</param>
    /// <param name="explanation">Text that is shown when a user chooses an incorrect answer or taps on the lamp icon in a quiz-style poll, 0-200 characters with at most 2 line feeds after entities parsing</param>
    /// <param name="explanationParseMode">Mode for parsing entities in the explanation. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details.</param>
    /// <param name="explanationEntities">A list of special entities that appear in the poll explanation. It can be specified instead of <paramref name="explanationParseMode"/></param>
    /// <param name="questionParseMode">Mode for parsing entities in the question. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details. Currently, only custom emoji entities are allowed</param>
    /// <param name="questionEntities">A list of special entities that appear in the poll question. It can be specified instead of <paramref name="questionParseMode"/></param>
    /// <param name="openPeriod">Amount of time in seconds the poll will be active after creation, 5-600. Can't be used together with <paramref name="closeDate"/>.</param>
    /// <param name="closeDate">Point in time when the poll will be automatically closed. Must be at least 5 and no more than 600 seconds in the future. Can't be used together with <paramref name="openPeriod"/>.</param>
    /// <param name="isClosed">Pass <see langword="true"/> if the poll needs to be immediately closed. This can be useful for poll preview.</param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public async Task<Message> SendPoll(
        string question,
        IEnumerable<InputPollOption> options,
        IReplyMarkup? replyMarkup = default,
        ChatId? chatId = default,
        bool isAnonymous = true,
        PollType? type = default,
        bool allowsMultipleAnswers = default,
        int? correctOptionId = default,
        ReplyParameters? replyParameters = default,
        string? explanation = default,
        ParseMode explanationParseMode = ParseMode.Markdown,
        IEnumerable<MessageEntity>? explanationEntities = default,
        ParseMode questionParseMode = ParseMode.Markdown,
        IEnumerable<MessageEntity>? questionEntities = default,
        int? openPeriod = default,
        DateTime? closeDate = default,
        bool isClosed = default,
        int? messageThreadId = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        var msg = await Client.SendPoll(chatId, question, options, isAnonymous, type, allowsMultipleAnswers, correctOptionId, replyParameters, replyMarkup, explanation, explanationParseMode, explanationEntities, questionParseMode, questionEntities, openPeriod, closeDate, isClosed, messageThreadId, disableNotification, protectContent, messageEffectId, businessConnectionId, allowPaidBroadcast, cancellationToken).PutState(this);
        await SaveState();
        return msg;
    }

    /// <summary>
    /// Use this method to send an animated emoji that will display a random value.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="emoji">Emoji on which the dice throw animation is based. Currently, must be one of “🎲”, “🎯”, “🏀”, “⚽”, “🎳”, or “🎰”. Dice can have values 1-6 for “🎲”, “🎯” and “🎳”, values 1-5 for “🏀” and “⚽”, and values 1-64 for “🎰”. Defaults to “🎲”</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public async Task<Message> SendDice(
        IReplyMarkup? replyMarkup = default,
        ChatId? chatId = default,
        string? emoji = default,
        ReplyParameters? replyParameters = default,
        int? messageThreadId = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        var msg = await Client.SendDice(chatId, emoji, replyParameters, replyMarkup, messageThreadId, disableNotification, protectContent, messageEffectId, businessConnectionId, allowPaidBroadcast, cancellationToken).PutState(this);
        await SaveState();
        return msg;
    }

    /// <summary>
    /// Use this method when you need to tell the user that something is happening on the bot's side. The status is set for 5 seconds or less (when a message arrives from your bot, Telegram clients clear its typing status).<br/>
    /// We only recommend using this method when a response from the bot will take a <b>noticeable</b> amount of time to arrive.
    /// </summary>
    /// <remarks>
    /// Example: The <a href="https://t.me/imagebot">ImageBot</a> needs some time to process a request and upload the image. Instead of sending a text message along the lines of “Retrieving image, please wait…”, the bot may use <see cref="TelegramBotClientExtensions.SendChatAction">SendChatAction</see> with <paramref name="action"/> = <em>UploadPhoto</em>. The user will see a “sending photo” status for the bot.
    /// </remarks>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="action">Type of action to broadcast. Choose one, depending on what the user is about to receive: <em>typing</em> for <see cref="TelegramBotClientExtensions.SendMessage">text messages</see>, <em>UploadPhoto</em> for <see cref="TelegramBotClientExtensions.SendPhoto">photos</see>, <em>RecordVideo</em> or <em>UploadVideo</em> for <see cref="TelegramBotClientExtensions.SendVideo">videos</see>, <em>RecordVoice</em> or <em>UploadVoice</em> for <see cref="TelegramBotClientExtensions.SendVoice">voice notes</see>, <em>UploadDocument</em> for <see cref="TelegramBotClientExtensions.SendDocument">general files</see>, <em>ChooseSticker</em> for <see cref="TelegramBotClientExtensions.SendSticker">stickers</see>, <em>FindLocation</em> for <see cref="TelegramBotClientExtensions.SendLocation">location data</see>, <em>RecordVideoNote</em> or <em>UploadVideoNote</em> for <see cref="TelegramBotClientExtensions.SendVideoNote">video notes</see>.</param>
    /// <param name="messageThreadId">Unique identifier for the target message thread; for supergroups only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the action will be sent</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    public async Task SendChatAction(
        ChatAction action,
        ChatId? chatId = default,
        int? messageThreadId = default,
        string? businessConnectionId = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        await Client.SendChatAction(chatId, action, messageThreadId, businessConnectionId, cancellationToken);
    }

    /// <summary>
    /// Use this method to edit text and <a href="https://core.telegram.org/bots/api#games">game</a> messages.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="messageId">Identifier of the message to edit</param>
    /// <param name="text">New text of the message, 1-4096 characters after entities parsing</param>
    /// <param name="parseMode">Mode for parsing entities in the message text. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details.</param>
    /// <param name="entities">A list of special entities that appear in message text, which can be specified instead of <paramref name="parseMode"/></param>
    /// <param name="linkPreviewOptions">Link preview generation options for the message</param>
    /// <param name="replyMarkup">An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>.</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message to be edited was sent</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The edited <see cref="Message"/> is returned</returns>
    public async Task<Message?> EditMessage(
        string text,
        InlineKeyboardMarkup? replyMarkup = default,
        ChatId? chatId = default,
        int? messageId = default,
        ParseMode parseMode = ParseMode.Markdown,
        IEnumerable<MessageEntity>? entities = default,
        LinkPreviewOptions? linkPreviewOptions = default,
        string? businessConnectionId = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        messageId ??= MessageId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        if (messageId == null) throw new ArgumentNullException(nameof(messageId), "Failed to infer message ID");
        try {
            var msg = await Client.EditMessageText(chatId, messageId.Value, text, parseMode, entities, linkPreviewOptions, replyMarkup, businessConnectionId, cancellationToken).PutState(this);
            await SaveState();
            return msg;
        } catch (ApiRequestException e) {
            if (e.Message.Contains("message is not modified"))
                return null;
            throw;
        }
    }

    /// <summary>
    /// Use this method to edit captions of messages.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="messageId">Identifier of the message to edit</param>
    /// <param name="caption">New caption of the message, 0-1024 characters after entities parsing</param>
    /// <param name="parseMode">Mode for parsing entities in the message caption. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details.</param>
    /// <param name="captionEntities">A list of special entities that appear in the caption, which can be specified instead of <paramref name="parseMode"/></param>
    /// <param name="showCaptionAboveMedia">Pass <see langword="true"/>, if the caption must be shown above the message media. Supported only for animation, photo and video messages.</param>
    /// <param name="replyMarkup">An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>.</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message to be edited was sent</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The edited <see cref="Message"/> is returned</returns>
    public async Task<Message?> EditMessageCaption(
        string? caption,
        InlineKeyboardMarkup? replyMarkup = default,
        ChatId? chatId = default,
        int? messageId = default,
        ParseMode parseMode = ParseMode.Markdown,
        IEnumerable<MessageEntity>? captionEntities = default,
        bool showCaptionAboveMedia = default,
        string? businessConnectionId = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        messageId ??= MessageId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        if (messageId == null) throw new ArgumentNullException(nameof(messageId), "Failed to infer message ID");
        try {
            var msg = await Client.EditMessageCaption(chatId, messageId.Value, caption, parseMode, captionEntities, showCaptionAboveMedia, replyMarkup, businessConnectionId, cancellationToken).PutState(this);
            await SaveState();
            return msg;
        } catch (ApiRequestException e) {
            if (e.Message.Contains("message is not modified"))
                return null;
            throw;
        }
    }

    /// <summary>
    /// Use this method to edit animation, audio, document, photo, or video messages, or to add media to text messages. If a message is part of a message album, then it can be edited only to an audio for audio albums, only to a document for document albums and to a photo or a video otherwise. When an inline message is edited, a new file can't be uploaded; use a previously uploaded file via its FileId or specify a URL.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="messageId">Identifier of the message to edit</param>
    /// <param name="media">An object for a new media content of the message</param>
    /// <param name="replyMarkup">An object for a new <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>.</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message to be edited was sent</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The edited <see cref="Message"/> is returned</returns>
    public async Task<Message?> EditMessageMedia(
        InputMedia media,
        InlineKeyboardMarkup? replyMarkup = default,
        ChatId? chatId = default,
        int? messageId = default,
        string? businessConnectionId = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        messageId ??= MessageId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        if (messageId == null) throw new ArgumentNullException(nameof(messageId), "Failed to infer message ID");
        try {
            var msg = await Client.EditMessageMedia(chatId, messageId.Value, media, replyMarkup, businessConnectionId, cancellationToken).PutState(this);
            await SaveState();
            return msg;
        } catch (ApiRequestException e) {
            if (e.Message.Contains("message is not modified"))
                return null;
            throw;
        }
    }

    /// <summary>
    /// Use this method to edit only the reply markup of messages.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="messageId">Identifier of the message to edit</param>
    /// <param name="replyMarkup">An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>.</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message to be edited was sent</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The edited <see cref="Message"/> is returned</returns>
    public async Task<Message?> EditMessageReplyMarkup(
        InlineKeyboardMarkup? replyMarkup = default,
        ChatId? chatId = default,
        int? messageId = default,
        string? businessConnectionId = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        messageId ??= MessageId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        if (messageId == null) throw new ArgumentNullException(nameof(messageId), "Failed to infer message ID");
        try {
            var msg = await Client.EditMessageReplyMarkup(chatId, messageId.Value, replyMarkup, businessConnectionId, cancellationToken).PutState(this);
            await SaveState();
            return msg;
        } catch (ApiRequestException e) {
            if (e.Message.Contains("message is not modified"))
                return null;
            throw;
        }
    }

    /// <summary>
    /// Use this method to send .WEBP, <a href="https://telegram.org/blog/animated-stickers">animated</a> .TGS, or <a href="https://telegram.org/blog/video-stickers-better-reactions">video</a> .WEBM stickers.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="sticker">Sticker to send. Pass a FileId as String to send a file that exists on the Telegram servers (recommended), pass an HTTP URL as a String for Telegram to get a .WEBP sticker from the Internet, or upload a new .WEBP, .TGS, or .WEBM sticker using <see cref="InputFileStream"/>. <a href="https://core.telegram.org/bots/api#sending-files">More information on Sending Files »</a>. Video and animated stickers can't be sent via an HTTP URL.</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</param>
    /// <param name="emoji">Emoji associated with the sticker; only for just uploaded stickers</param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public async Task<Message> SendSticker(
        InputFile sticker,
        IReplyMarkup? replyMarkup = default,
        ChatId? chatId = default,
        ReplyParameters? replyParameters = default,
        string? emoji = default,
        int? messageThreadId = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        var msg = await Client.SendSticker(chatId, sticker, replyParameters, replyMarkup, emoji, messageThreadId, disableNotification, protectContent, messageEffectId, businessConnectionId, allowPaidBroadcast, cancellationToken).PutState(this);
        await SaveState();
        return msg;
    }

    /// <summary>
    /// Sends a gift to the given user. The gift can't be converted to Telegram Stars by the user.
    /// </summary>
    /// <param name="userId">Unique identifier of the target user that will receive the gift</param>
    /// <param name="giftId">Identifier of the gift</param>
    /// <param name="text">Text that will be shown along with the gift; 0-255 characters</param>
    /// <param name="textParseMode">Mode for parsing entities in the text. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details. Entities other than <see cref="MessageEntityType.Bold">Bold</see>, <see cref="MessageEntityType.Italic">Italic</see>, <see cref="MessageEntityType.Underline">Underline</see>, <see cref="MessageEntityType.Strikethrough">Strikethrough</see>, <see cref="MessageEntityType.Spoiler">Spoiler</see>, and <see cref="MessageEntityType.CustomEmoji">CustomEmoji</see> are ignored.</param>
    /// <param name="textEntities">A list of special entities that appear in the gift text. It can be specified instead of <paramref name="textParseMode"/>. Entities other than <see cref="MessageEntityType.Bold">Bold</see>, <see cref="MessageEntityType.Italic">Italic</see>, <see cref="MessageEntityType.Underline">Underline</see>, <see cref="MessageEntityType.Strikethrough">Strikethrough</see>, <see cref="MessageEntityType.Spoiler">Spoiler</see>, and <see cref="MessageEntityType.CustomEmoji">CustomEmoji</see> are ignored.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    public async Task SendGift(
        string giftId,
        long? userId = null,
        string? text = default,
        ParseMode textParseMode = ParseMode.Markdown,
        IEnumerable<MessageEntity>? textEntities = default,
        CancellationToken cancellationToken = default
    ) {
        userId ??= UserId;
        if (userId == null) throw new ArgumentNullException(nameof(userId), "Failed to infer user ID");
        await Client.SendGift(userId.Value, giftId, text, textParseMode, textEntities, cancellationToken);
    }
    
    /// <summary>
    /// Use this method to send invoices.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="title">Product name, 1-32 characters</param>
    /// <param name="description">Product description, 1-255 characters</param>
    /// <param name="payload">Bot-defined invoice payload, 1-128 bytes. This will not be displayed to the user, use it for your internal processes.</param>
    /// <param name="currency">Three-letter ISO 4217 currency code, see <a href="https://core.telegram.org/bots/payments#supported-currencies">more on currencies</a>. Pass “XTR” for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</param>
    /// <param name="prices">Price breakdown, a list of components (e.g. product price, tax, discount, delivery cost, delivery tax, bonus, etc.). Must contain exactly one item for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</param>
    /// <param name="providerToken">Payment provider token, obtained via <a href="https://t.me/botfather">@BotFather</a>. Pass an empty string for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</param>
    /// <param name="providerData">JSON-serialized data about the invoice, which will be shared with the payment provider. A detailed description of required fields should be provided by the payment provider.</param>
    /// <param name="maxTipAmount">The maximum accepted amount for tips in the <em>smallest units</em> of the currency (integer, <b>not</b> float/double). For example, for a maximum tip of <c>US$ 1.45</c> pass <c><paramref name="maxTipAmount"/> = 145</c>. See the <em>exp</em> parameter in <a href="https://core.telegram.org/bots/payments/currencies.json">currencies.json</a>, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies). Defaults to 0. Not supported for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</param>
    /// <param name="suggestedTipAmounts">A array of suggested amounts of tips in the <em>smallest units</em> of the currency (integer, <b>not</b> float/double). At most 4 suggested tip amounts can be specified. The suggested tip amounts must be positive, passed in a strictly increased order and must not exceed <paramref name="maxTipAmount"/>.</param>
    /// <param name="photoUrl">URL of the product photo for the invoice. Can be a photo of the goods or a marketing image for a service. People like it better when they see what they are paying for.</param>
    /// <param name="photoSize">Photo size in bytes</param>
    /// <param name="photoWidth">Photo width</param>
    /// <param name="photoHeight">Photo height</param>
    /// <param name="needName">Pass <see langword="true"/> if you require the user's full name to complete the order. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</param>
    /// <param name="needPhoneNumber">Pass <see langword="true"/> if you require the user's phone number to complete the order. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</param>
    /// <param name="needEmail">Pass <see langword="true"/> if you require the user's email address to complete the order. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</param>
    /// <param name="needShippingAddress">Pass <see langword="true"/> if you require the user's shipping address to complete the order. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</param>
    /// <param name="sendPhoneNumberToProvider">Pass <see langword="true"/> if the user's phone number should be sent to the provider. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</param>
    /// <param name="sendEmailToProvider">Pass <see langword="true"/> if the user's email address should be sent to the provider. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</param>
    /// <param name="isFlexible">Pass <see langword="true"/> if the final price depends on the shipping method. Ignored for payments in <a href="https://t.me/BotNews/90">Telegram Stars</a>.</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>. If empty, one 'Pay <c>total price</c>' button will be shown. If not empty, the first button must be a Pay button.</param>
    /// <param name="startParameter">Unique deep-linking parameter. If left empty, <b>forwarded copies</b> of the sent message will have a <em>Pay</em> button, allowing multiple users to pay directly from the forwarded message, using the same invoice. If non-empty, forwarded copies of the sent message will have a <em>URL</em> button with a deep link to the bot (instead of a <em>Pay</em> button), with the value used as the start parameter</param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public async Task<Message> SendInvoice(
        string title,
        string description,
        string payload,
        string currency,
        IEnumerable<LabeledPrice> prices,
        InlineKeyboardMarkup? replyMarkup = default,
        ChatId? chatId = default,
        string? providerToken = default,
        string? providerData = default,
        int? maxTipAmount = default,
        IEnumerable<int>? suggestedTipAmounts = default,
        string? photoUrl = default,
        int? photoSize = default,
        int? photoWidth = default,
        int? photoHeight = default,
        bool needName = default,
        bool needPhoneNumber = default,
        bool needEmail = default,
        bool needShippingAddress = default,
        bool sendPhoneNumberToProvider = default,
        bool sendEmailToProvider = default,
        bool isFlexible = default,
        ReplyParameters? replyParameters = default,
        string? startParameter = default,
        int? messageThreadId = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        var msg = await Client.SendInvoice(chatId, title, description, payload, currency, prices, providerToken, providerData, maxTipAmount, suggestedTipAmounts, photoUrl, photoSize, photoWidth, photoHeight, needName, needPhoneNumber, needEmail, needShippingAddress, sendPhoneNumberToProvider, sendEmailToProvider, isFlexible, replyParameters, replyMarkup, startParameter, messageThreadId, disableNotification, protectContent, messageEffectId, allowPaidBroadcast, cancellationToken).PutState(this);
        await SaveState();
        return msg;
    }
    
    /// <summary>
    /// Use this method to send a game.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat</param>
    /// <param name="gameShortName">Short name of the game, serves as the unique identifier for the game. Set up your games via <a href="https://t.me/botfather">@BotFather</a>.</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>. If empty, one 'Play GameTitle' button will be shown. If not empty, the first button must launch the game.</param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public async Task<Message> SendGame(
        string gameShortName,
        InlineKeyboardMarkup? replyMarkup = default,
        long? chatId = default,
        ReplyParameters? replyParameters = default,
        int? messageThreadId = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default
    ) {
        chatId ??= ChatId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "Failed to infer chat ID");
        var msg = await Client.SendGame(chatId.Value, gameShortName, replyParameters, replyMarkup, messageThreadId, disableNotification, protectContent, messageEffectId, businessConnectionId, allowPaidBroadcast, cancellationToken).PutState(this);
        await SaveState();
        return msg;
    }
    
    /// <summary>
    /// Edits current message if the update was a callback query, otherwise sends a new one.
    /// </summary>
    /// <param name="chatId">Unique identifier for the target chat or username of the target channel (in the format <c>@channelusername</c>)</param>
    /// <param name="messageId">Identifier of the message to edit</param>
    /// <param name="text">Text of the message to be sent, 1-4096 characters after entities parsing</param>
    /// <param name="parseMode">Mode for parsing entities in the message text. See <a href="https://core.telegram.org/bots/api#formatting-options">formatting options</a> for more details.</param>
    /// <param name="replyParameters">Description of the message to reply to</param>
    /// <param name="replyMarkup">Additional interface options. An object for an <a href="https://core.telegram.org/bots/features#inline-keyboards">inline keyboard</a>, <a href="https://core.telegram.org/bots/features#keyboards">custom reply keyboard</a>, instructions to remove a reply keyboard or to force a reply from the user</param>
    /// <param name="linkPreviewOptions">Link preview generation options for the message</param>
    /// <param name="messageThreadId">Unique identifier for the target message thread (topic) of the forum; for forum supergroups only</param>
    /// <param name="entities">A list of special entities that appear in message text, which can be specified instead of <paramref name="parseMode"/></param>
    /// <param name="disableNotification">Sends the message <a href="https://telegram.org/blog/channels-2-0#silent-messages">silently</a>. Users will receive a notification with no sound.</param>
    /// <param name="protectContent">Protects the contents of the sent message from forwarding and saving</param>
    /// <param name="messageEffectId">Unique identifier of the message effect to be added to the message; for private chats only</param>
    /// <param name="businessConnectionId">Unique identifier of the business connection on behalf of which the message will be sent</param>
    /// <param name="allowPaidBroadcast">Pass <see langword="true"/> to allow up to 1000 messages per second, ignoring <a href="https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once">broadcasting limits</a> for a fee of 0.1 Telegram Stars per message. The relevant Stars will be withdrawn from the bot's balance</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation</param>
    /// <returns>The sent <see cref="Message"/> is returned.</returns>
    public async Task<Message?> SendOrEditMessage(
        string text,
        InlineKeyboardMarkup? replyMarkup = default,
        ChatId? chatId = default,
        MessageId? messageId = default,
        ParseMode parseMode = ParseMode.Markdown,
        ReplyParameters? replyParameters = default,
        LinkPreviewOptions? linkPreviewOptions = default,
        int? messageThreadId = default,
        IEnumerable<MessageEntity>? entities = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default) {
        chatId ??= ChatId; messageId ??= MessageId;
        if (chatId == null) throw new ArgumentNullException(nameof(chatId), "failed to infer chat ID");
        if (Update.Type == UpdateType.CallbackQuery) {
            if (messageId == null) throw new ArgumentNullException(nameof(messageId), "failed to infer message ID");
            return await EditMessage(text, replyMarkup, chatId, messageId, parseMode, entities, linkPreviewOptions, businessConnectionId, cancellationToken);
        }
        return await SendMessage(text, replyMarkup, chatId, parseMode, replyParameters, linkPreviewOptions, messageThreadId, entities, disableNotification, protectContent, messageEffectId, businessConnectionId, allowPaidBroadcast, cancellationToken);
    }
}