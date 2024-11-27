![badges](https://img.shields.io/github/contributors/TheAirBlow/Stateful.svg)
![badges](https://img.shields.io/github/forks/TheAirBlow/Stateful.svg)
![badges](https://img.shields.io/github/stars/TheAirBlow/Stateful.svg)
![badges](https://img.shields.io/github/issues/TheAirBlow/Stateful.svg)
![badges](https://github.com/TheAirBlow/Stateful/actions/workflows/nuget.yml/badge.svg)
# Stateful
Spices up Telegram bot development

## Features
1) Message states to enable simple interactions
2) Easy to use eflection-based handlers
3) Other miscellaneous helper classes

## Usage
### Bot setup
```csharp
using Stateful.Attributes;
using Telegram.Bot;

var client = new TelegramBotClient("TOKEN");
var stateful = new StatefulHandler(
    new StatefulOptions {
        ErrorHandler = (bot, e, src, _) => Log.Error("Error occured from {0}: {1}", src, e),
        StateHandler = new MongoStateHandler(collection),
        Filters = [ new PrivateOnlyAttribute() ]
    });
stateful.Register<MainHandler>("main");
client.StartReceiving(stateful);
```

### Handling updates
```csharp
using Stateful.Attributes;

namespace Stateful;

[PrivateOnly]
public class MainHandler : UpdateHandler {
    [DefaultHandler]
    private async Task Default()
        => await SendMessage(
            "Welcome to my amazing Telegram bot 👋", 
            this.GenerateReply());

    [Message("Hello 👋")]
    private async Task ExampleReply()
        => await SendOrEditMessage(
            $"Hello! Here's a random number: {Random.Shared.Next(15)}", 
            this.GenerateInline());

    [Callback("Click me")]
    private async Task ExampleCallback()
        => await ExampleReply();
}
```

## Licence
[Mozilla Public License Version 2.0](https://github.com/TheAirBlow/Syndical/blob/main/LICENCE)
