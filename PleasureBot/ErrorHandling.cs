using Telegram.Bot;

namespace PleasureBot;

internal static class ErrorHandling
{
    public static async Task Error(ITelegramBotClient client, Exception error, CancellationToken cancelToken)
    {
        throw new NotImplementedException();
    }
}