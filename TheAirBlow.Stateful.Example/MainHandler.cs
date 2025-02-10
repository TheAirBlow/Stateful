using TheAirBlow.Stateful.Attributes;
using TheAirBlow.Stateful.Commands;
using TheAirBlow.Stateful.Conditions;
using TheAirBlow.Stateful.Keyboards;

namespace TheAirBlow.Stateful.Testing;

[PrivateOnly]
public class MainHandler : UpdateHandler {
    [DefaultHandler]
    private async Task Default()
        => await SendMessage(
            "Welcome to my amazing Telegram bot 👋", 
            GenerateReply());

    [Message("Open test")]
    private async Task Test()
        => await ChangeHandler("test", true);
    
    [Command("start")]
    private async Task StartCommand()
        => await SendOrEditMessage("wow this is the start command",
            Keyboard.Paginator(this, 3, "a\n", "b\n", "c\n", "d\n", "e\n", "f\n", "g\n", "h\n", "i\n"));
    
    [Command("args")]
    private async Task ArgsTest(int a, string b, int? c = null)
        => await SendOrEditMessage($"a: `{a}`, b: `{b}`, c: `{c}`");
    
    [Message("Hello 👋")]
    private async Task ExampleReply()
        => await SendOrEditMessage(
            $"Hello! Here's a random number: {Random.Shared.Next(15)}", 
            GenerateInline());

    [Callback("Click me")]
    private async Task ExampleCallback()
        => await ExampleReply();
}