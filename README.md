![badges](https://img.shields.io/github/contributors/TheAirBlow/Stateful.svg)
![badges](https://img.shields.io/github/forks/TheAirBlow/Stateful.svg?style=flat)
![badges](https://img.shields.io/github/stars/TheAirBlow/Stateful.svg?style=flat)
![badges](https://img.shields.io/github/issues/TheAirBlow/Stateful.svg)
![badges](https://github.com/TheAirBlow/Stateful/actions/workflows/nuget.yml/badge.svg)
# Stateful
Spice up Telegram bot development

## Features
1) Message states to enable simple interactions
2) Easy to use reflection-based handlers
3) Automatic reply or inline keyboard generator
4) Extra helper methods and classes

## States
Message state is attached to every Telegram message the bot or the user sends. \
State is inherited from the last sent message, if there is none for the current one.
```csharp
// put a database ID in your state for later use
var id = "b4b920e9-64c7-4fff-9b42-45f331aff67f";
State.SetState("id", id);

// retrieve the state in a next part of the interaction
var id = State.GetState<string>("id")!;
```

## Usage
### Bot setup
Stateful is just an update handler, which you can use for a simple polling setup:
```csharp
using Stateful.Attributes;
using Telegram.Bot;

// call this BEFORE you create a MongoDB client!
MongoStateHandler.RegisterConvention();

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
Make a class that inherits `UpdateHandler` and use attributes to filter updates:
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
