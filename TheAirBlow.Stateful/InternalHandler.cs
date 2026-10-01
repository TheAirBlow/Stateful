using Telegram.Bot;
using Telegram.Bot.Exceptions;
using TheAirBlow.Stateful.Conditions;
using TheAirBlow.Stateful.Exceptions;
using TheAirBlow.Stateful.Keyboards;

namespace TheAirBlow.Stateful;

/// <summary>
/// Internal update handler
/// </summary>
internal class InternalHandler : UpdateHandler {
    [Callback(Data.ParsedRegex, "^{internal}paginator-([0-9]+)$")]
    private async Task Paginator(int page) {
        for (var attempt = 0; ; attempt++) {
            var data = State.GetState<Keyboard.PaginatorData>("paginator_data");
            if (data == null || page < 0 || page >= data.Pages || page == data.Page) return;
            data.Page = page; 
            State.SetState("paginator_data", data, true);
            try {
                await SaveState();
            } catch (StateConflictException) when (attempt < 2 && Stateful.Options.StateHandler != null) {
                State = await Stateful.Options.StateHandler.GetState(Update);
                continue;
            }

            try {
                await Client.EditMessageReplyMarkup(ChatId!.Value,
                    MessageId!.Value, Keyboard.Inline(data.GetButtons(Stateful.Options.InternalPrefix)));
            } catch (ApiRequestException e) {
                if (e.Message.Contains("message is not modified")) return;
                throw;
            }
            return;
        }
    }
}
