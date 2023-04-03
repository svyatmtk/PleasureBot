using Telegram.Bot;

namespace PleasureBot;

internal static class BotInitialization
{
    public static void BotInit()
    {
        var botClient = new TelegramBotClient(SetToken());
        using CancellationTokenSource cts = new();
        botClient.StartReceiving(
            UpdateHandling.Update,
            ErrorHandling.Error, cancellationToken: cts.Token);
    }

    private static string SetToken()
    {
        var token = Environment.GetEnvironmentVariable("Telegram_Token")!;
        return token;
    }
}