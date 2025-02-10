using System.Text.RegularExpressions;
using MongoDB.Driver;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using TheAirBlow.Stateful;
using TheAirBlow.Stateful.Conditions;
using TheAirBlow.Stateful.MongoDB;
using TheAirBlow.Stateful.Testing;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console().CreateLogger();

Log.Information("Starting up stateful example");
MongoStateHandler.RegisterConvention();

var mongo = new MongoClient(new MongoClientSettings {
    Server = new MongoServerAddress("localhost"),
    MaxConnectionPoolSize = 500
});

var database = mongo.GetDatabase("stateful-test");
var states = database.GetCollection<MessageState>("states");

var token = File.ReadAllText("token.txt").Trim();
var client = new TelegramBotClient(token);
var bot = await client.GetMe();
Log.Information("Logged in as {0}", bot.Username);

var stateful = new StatefulHandler(
    new StatefulOptions {
        ErrorHandler = (_, e, src, _) => {
            Log.Error("{0} occured: {1}", src, e);
            return Task.CompletedTask;
        },
        CommandErrorHandler = async (handler, ex, cmd) => {
            Log.Error("Failed to parse {0} parameters: {1}", $"/{cmd.Name}", ex);
            var messageId = handler.Update.GetMessageId();
            var reply = messageId != null ? new ReplyParameters { MessageId = messageId.Value } : null;
            var usage = string.Join(" ", cmd.Parameters.Select(x => x.Required ? $"[{x.Name}]" : $"({x.Name})"));
            await handler.SendMessage($"Invalid usage, expected `/{cmd.Name} {usage}`", replyParameters: reply);
        },
        StateHandler = new MongoStateHandler(states),
        Filters = [new PrivateOnlyAttribute()],
        DefaultThreading = Threading.PerUpdate
    });

stateful.Register<MainHandler>("main");
stateful.Register<TestHandler>("test");
client.StartReceiving(stateful);

Log.Information("Started receiving updates");
await Task.Delay(-1);