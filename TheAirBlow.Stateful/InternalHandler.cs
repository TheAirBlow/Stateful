using Telegram.Bot;
using Telegram.Bot.Exceptions;
using TheAirBlow.Stateful.Conditions;
using TheAirBlow.Stateful.Keyboards;

namespace TheAirBlow.Stateful;

/// <summary>
/// Internal update handler
/// </summary>
internal class InternalHandler : UpdateHandler {
    [Callback(Data.ParsedRegex, "stinternal-paginator-([0-9]*)")]
    private async Task Paginator(int page) {
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