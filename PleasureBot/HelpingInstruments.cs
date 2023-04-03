using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PleasureBot;

internal static class HelpingInstruments
{
    public static bool Compare(string message, string waitedMessage)
    {
        return message == waitedMessage;
    }

    public static async Task ShowStartMenu(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "Gpt3", "DALL·E", "ChatGPT" }
        })
        {
            ResizeKeyboard = true
        };
        await botClient.SendTextMessageAsync(message.Chat.Id,
            "Выберите какую нейросеть использовать",
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
    }

    public static void LogOutput(Message message, string? text)
    {
        Console.WriteLine("User " + message.Chat.FirstName + " " + message.Chat.LastName + " send " + "\"" +
                          message.Text + "\"" + " prompt");
        Console.Write("And recieved " + "\"" + text + "\"");
        Console.WriteLine("at " + ShowLocalTime());
        Console.WriteLine("------------------------------------------------------------------------------");
    }

    public static DateTime ShowLocalTime()
    {
        var utcTime = DateTime.UtcNow;
        var moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
        var moscowTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, moscowTimeZone);
        return moscowTime;
    }
}