using Stateful.Attributes;
using Stateful.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace Stateful;

/// <summary>
/// Internal update handler
/// </summary>
public class InternalHandler : UpdateHandler {
    [InternalCallback("paginator")]
    private async Task Paginator() {
        var id = Update.CallbackQuery?.Data;
        if (!int.TryParse(id?[21..], out var page)) return;
        var data = State.GetState<Keyboard.PaginatorData>("paginator_data");
        if (data == null || page < 0 || page >= data.Pages || page == data.Page) return;
        data.Page = page; 
        State.SetState("paginator_data", data);
        await SaveState();
        try {
            await Client.EditMessageReplyMarkup(ChatId!.Value,
                MessageId!.Value, Keyboard.Inline(data.GetButtons()));
        } catch (ApiRequestException e) {
            if (e.Message.Contains("message is not modified")) return;
            throw;
        }
    }
}