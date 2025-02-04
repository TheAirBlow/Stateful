using TheAirBlow.Stateful.Attributes;

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
    
    [Message("Hello 👋")]
    private async Task ExampleReply()
        => await SendOrEditMessage(
            $"Hello! Here's a random number: {Random.Shared.Next(15)}", 
            GenerateInline());

    [Callback("Click me")]
    private async Task ExampleCallback()
        => await ExampleReply();
}