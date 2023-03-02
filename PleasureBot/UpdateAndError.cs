using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;

namespace PleasureBot;

internal class UpdateAndError
{
    public static async Task Update(ITelegramBotClient botClient, Update update, CancellationToken cancelToken)
    {
        var message = update.Message;

        if (CheckIfMessageHasBeenWritten(message))
        {
            await GiveResultIfTextTyped(botClient, cancelToken, message);
        }
    }

    public static async Task Error(ITelegramBotClient client, Exception error, CancellationToken cancelToken)
    {
        throw new NotImplementedException();
    }

    private static async Task GiveResultIfTextTyped(ITelegramBotClient botClient, CancellationToken cancelToken,
        Message message)
    {
        if (CheckIfStartButtonHasBeenPushed(message))
        {
            await IfStartButtonPushedThenDoStartAnswer(botClient, message);
        }
        else if (CheckIfGiveBaseButtonIsPushed(message))
        {
            await IfGiveBaseButtonIsPushedThenDoBaseAnswer(botClient, cancelToken, message);
        }
        else
        {
            await IfGtp3ButtonPushedThenConversationStart(botClient, message);
        }
    }

    private static bool CheckIfMessageHasBeenWritten(Message? message) => message?.Text != null;
    private static bool CheckIfGiveBaseButtonIsPushed(Message message) => message.Text?.ToLower() == "/give_base";
    private static bool CheckIfStartButtonHasBeenPushed(Message message) => message.Text?.ToLower() == "/start";

    private static void WriteExceptionMessageToUser(ITelegramBotClient botClient, Message message)
    {
        botClient.SendTextMessageAsync(
            message.Chat.Id,
            text: "Что - то не так с нейросетями, повторите попытку позже");
    }

    private static async Task IfGiveBaseButtonIsPushedThenDoBaseAnswer(ITelegramBotClient botClient,
        CancellationToken cancelToken, Message message)
    {
        var botMessage = await botClient.SendPhotoAsync(
            chatId: message.Chat.Id,
            photo:
            "https://raw.githubusercontent.com/svyatmtk/PleasureBot/master/Pictures/DimaBaza1.png?token=GHSAT0AAAAAAB5QWYRGAC6DOLJF4LXGTICOY6EXDDA",
            caption: "<b>Это база</b>...",
            parseMode: ParseMode.Html,
            cancellationToken: cancelToken);

        LogOutput(message, botMessage);
    }

    private static async Task IfStartButtonPushedThenDoStartAnswer(ITelegramBotClient botClient, Message message)
    {
        var botMessage = await botClient.SendTextMessageAsync(
            message.Chat.Id,
            $"Привет {message.Chat.FirstName}, " +
            $"я Даня Круговой, левый защитник Зенита, готов с тобой немного поболтать (да это правда я)))))",
            replyToMessageId: message.MessageId);

        LogOutput(message, botMessage);
    }

    private static void LogOutput(Message message, Message botMessage)
    {
        Console.WriteLine(
            $"User {message.Chat.FirstName} {message.Chat.LastName} sent message " +
            $" \"{message.Text}\" at {message.Date.ToLocalTime()} and bot {botMessage.From.FirstName} " +
            $"answered {botMessage.Text} at {message.Date.ToLocalTime()}\n");
    }

    private static async Task IfGtp3ButtonPushedThenConversationStart(ITelegramBotClient botClient, Message message)
    {
        await TryGetResponseFromGpt3(botClient, message);
    }

    private static async Task TryGetResponseFromGpt3(ITelegramBotClient botClient, Message message)
    {
        try
        {
            await GettingResponseFromGpt3(botClient, message);
        }
        catch (Exception e)
        {
            WriteExceptionMessageToUser(botClient, message);
        }
    }


    private static async Task GettingResponseFromGpt3(ITelegramBotClient botClient, Message message)
    {
        var prompt = message.Text;
        var completionResult = await Gpt3Initialization.Gpt3Init().Completions.CreateCompletion(
            new CompletionCreateRequest()
            {
                Prompt = prompt,
                Model = Models.TextDavinciV3,
                MaxTokens = 1000,
                Temperature = 0.7f,
                TopP = 1
            });
        var text = completionResult.Choices.FirstOrDefault()?.Text;
        if (text != null)
        {
            var botMessage = await botClient.SendTextMessageAsync(
                message.Chat.Id,
                text,
                replyToMessageId: message.MessageId);
            LogOutput(message, botMessage);
        }
    }
}