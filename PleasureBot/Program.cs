using Telegram.Bot;
using Telegram.Bot.Types;

namespace PleasureBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var botClient = new TelegramBotClient("5693080132:AAFJIKepqagosQPdO4b9sVxfauW7bGWtcJA");           
            botClient.StartReceiving(Update, Error);
            Console.ReadLine();
        }

        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken canselToken)
        {
            var message = update.Message;

            if (message.Text != null) 
            {
                if (message.Text.ToLower().Contains("/start"))
                {
                    var botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, $"Привет {message.Chat.FirstName}, " +
                        $"я Даня Круговой, левый защитник Зенита, готов с тобой немного поболтать (да это правда я)))))", 
                    replyToMessageId: message.MessageId);

                    Console.WriteLine(
                    $"User {message.Chat.FirstName} {message.Chat.LastName} sent message " +
                    $" \"{message.Text}\" at {message.Date.ToLocalTime()} and bot {botMessage.From.FirstName} " +
                    $"answered {botMessage.Text} at {message.Date.ToLocalTime()}\n");

                    return;
                }    
            }
        }

        async static Task Error(ITelegramBotClient client, Exception error, CancellationToken canselToken)
        {
            throw new NotImplementedException();
        }
    }
}