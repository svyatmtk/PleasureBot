using Telegram.Bot;
using Telegram.Bot.Types;

namespace PleasureBot;

internal class UpdateHandling
{
    public static async Task Update(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message;

        if (message != null)
        {
            var date = update.Message!.Date;
            var datemes = DateTime.Now - date;
            if (datemes.Seconds > 5)
                Console.WriteLine("Старое сообщение");
            else
                await HandleRequest(botClient, update.Message, cancellationToken);
        }
    }

    private static async Task HandleRequest(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        var handler = new StartHandler().Build();
        await handler.Handle(message, botClient, cancellationToken);
    }
}