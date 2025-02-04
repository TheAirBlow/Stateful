using TheAirBlow.Stateful.Attributes;

namespace TheAirBlow.Stateful.Testing;

[PrivateOnly]
public class TestHandler : UpdateHandler {
    [DefaultHandler]
    private async Task Default()
        => await SendMessage(
            "Welcome to an example additional menu 👋", 
            GenerateReply());

    [Message("Open main")]
    private async Task Test()
        => await ChangeHandler("main", true);
    
    [Message("Hello V2 👋")]
    private async Task ExampleReply()
        => await SendOrEditMessage(
            $"Hello V2! Here's a random number: {Random.Shared.Next(15)}", 
            GenerateInline());

    [Callback("Click me")]
    private async Task ExampleCallback()
        => await ExampleReply();
}