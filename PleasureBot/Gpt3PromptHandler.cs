using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PleasureBot;

public class Gpt3PromptHandler : RequestHandler
{
    public override async Task<bool> Handle(Message message, ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        if (HelpingInstruments.Compare(message.ReplyToMessage?.Text!, "Пожалуйста, напишите запрос для GPT3"))
        {
            await GenerateGpt3Response(botClient, message, cancellationToken);
            await HelpingInstruments.ShowStartMenu(botClient, message, cancellationToken);
            return true;
        }

        return _nextHandler != null && await _nextHandler.Handle(message, botClient, cancellationToken);
    }

    private static async Task GenerateGpt3Response(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        var prompt = message.Text;
        var completionResult = await TakeAnswerFromNeuro(prompt);
        var text = completionResult.Choices.FirstOrDefault()?.Text;

        if (text != null)
        {
            var botMessage = await SendAnswerFromGpt3(botClient, message, text);
            HelpingInstruments.LogOutput(message, text);
        }
    }

    private static async Task<CompletionCreateResponse> TakeAnswerFromNeuro(string? prompt)
    {
        var completionResult = await OpenAiInitialization.OpenAiServicesInit().Completions.CreateCompletion(
            new CompletionCreateRequest
            {
                Prompt = prompt,
                Model = Models.TextDavinciV3,
                MaxTokens = 1000,
                Temperature = 0.7f,
                TopP = 1
            }, cancellationToken: new CancellationToken());
        return completionResult;
    }

    private static async Task<Message> SendAnswerFromGpt3(ITelegramBotClient botClient, Message message, string text)
    {
        return await botClient.SendTextMessageAsync(
            message.Chat.Id,
            text,
            replyToMessageId: message.MessageId);
    }
}