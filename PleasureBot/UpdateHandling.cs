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
            TimeSpan datemes;
            if (OperatingSystem.IsWindows())
            {
                 datemes = DateTime.Now - date.AddHours(3);
            }
            else
            {
                 datemes = DateTime.Now - date;
            }
            if (datemes.Seconds > 5)
                Console.WriteLine("Старое сообщение");
            else
            {

                var handle = HandleRequest(botClient, update.Message, cancellationToken);
                var timeOut = Task.Delay(90000);
                var delayCheck = Task.WhenAny(handle, timeOut);
                if (delayCheck == timeOut)
                {
                    await botClient.SendTextMessageAsync(
                        message.Chat.Id,
                        "Что-то пошло совсем не так, как планировалось, попробуйте ввести запрос снова");
                    return;
                }

                await handle;
            }
                
        }
    }

    private static async Task HandleRequest(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        var handler = new StartHandler().Build();
        await handler.Handle(message, botClient, cancellationToken);
    }
}