// See https://aka.ms/new-console-template for more information

using TgNews.BL;

var db = new TgNews.BL.Client.DbStorage(new DbConfiguration());
var tg = new TgNews.BL.Client.Telegram();
var tgBot = new TgNews.BL.Client.TelegramBot();

await tg.Init(new TgConfiguration());
await tgBot.Init(new TgConfiguration());

var forwarder = new Forwarder(tg, tgBot, db);
await forwarder.Execute();

Console.WriteLine("Press any key to quit");