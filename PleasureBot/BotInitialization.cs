using Telegram.Bot;

namespace PleasureBot;

internal static class BotInitialization
{
    public static void BotInit()
    {
        var botClient = new TelegramBotClient(SetToken());
        using CancellationTokenSource cts = new();
        botClient.StartReceiving(
            updateHandler: UpdateAndError.Update,
            pollingErrorHandler: UpdateAndError.Error
        );
    }

    private static string SetToken()
    {
        var token = Environment.GetEnvironmentVariable("Telegram_Token")!;
        return token;
    }
}