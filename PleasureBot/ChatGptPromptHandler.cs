using System.Diagnostics;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PleasureBot;

public class ChatGptPromptHandler : RequestHandler
{
    public override async Task<bool> Handle(Message message, ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        if (HelpingInstruments.Compare(message.ReplyToMessage?.Text!, "Пожалуйста, напишите запрос для ChatGPT"))
        {
            await TryToSendPromptToChatGpt(botClient, message, cancellationToken);
            await HelpingInstruments.ShowStartMenu(botClient, message, cancellationToken);
            return true;
        }

        return _nextHandler != null && await _nextHandler.Handle(message, botClient, cancellationToken);
    }

    private static async Task TryToSendPromptToChatGpt(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        var prompt = message.Text;

        SqLite.SaveMessage(message, prompt);
        var messages = SqLite.LoadMessagesForPrompt(message);

        await GenerateResponseWithTimeout(botClient, message, cancellationToken, prompt!, messages);
    }

    private static async Task GenerateResponseWithTimeout(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken, string prompt, IList<ChatMessage> messages)
    {
        var delayMessageIds = new int[2];

        var completionResultTask = OpenAiInitialization.OpenAiServicesInit().ChatCompletion.CreateCompletion(
            new ChatCompletionCreateRequest
            {
                Messages = messages,
                Model = Models.ChatGpt3_5Turbo,
                MaxTokens = 2048
            });

        if (await CheckForDelaysOrTimeOut(botClient, message, cancellationToken, completionResultTask, delayMessageIds)) return;

        var completionResult = await completionResultTask;

        await DeleteDelaysAlerts(botClient, message, cancellationToken, delayMessageIds);
        
        var lastResponse = completionResult.Choices.First().Message.Content;

        await TryToGetAnswerFromChatGpt(botClient, message, lastResponse, completionResult);
    }

    private static async Task TryToGetAnswerFromChatGpt(ITelegramBotClient botClient, Message message,
        string lastResponse, ChatCompletionCreateResponse completionResult)

    {
        lastResponse = completionResult.Choices.First().Message.Content;

        await botClient.SendTextMessageAsync(
            message.Chat.Id,
            lastResponse,
            replyToMessageId: message.MessageId);

        HelpingInstruments.LogOutput(message, lastResponse);
    }

    private static async Task<bool> CheckForDelaysOrTimeOut(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken, Task<ChatCompletionCreateResponse> completionResultTask, int[] delayMessageIds)
    {
        if (await CheckForFirstDelay(botClient, message, cancellationToken, completionResultTask, delayMessageIds)) return false;
        if (await CheckForSecondDelay(botClient, message, cancellationToken, completionResultTask, delayMessageIds)) return false;
        return await CheckForTimeOut(botClient, message, cancellationToken, completionResultTask, delayMessageIds);
    }

    private static async Task<bool> CheckForFirstDelay(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken, Task<ChatCompletionCreateResponse> completionResultTask,
        int[] delayMessageIds)
    {
        var timeoutTask = Task.Delay(5000);
        var completedTask = await Task.WhenAny(completionResultTask, timeoutTask);
        if (completedTask == timeoutTask)
        {
            await AskToWaitFirstTime(botClient, message, cancellationToken, delayMessageIds);
            return false;
        }

        return true;
    }

    private static async Task<bool> CheckForSecondDelay(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken, Task<ChatCompletionCreateResponse> completionResultTask, int[] delayMessageIds)
    {
        var timeoutTaskSecond = Task.Delay(15000);
        var completedTaskAttempt2 = await Task.WhenAny(completionResultTask, timeoutTaskSecond);
        if (completedTaskAttempt2 == timeoutTaskSecond)
        {
            await AskToWaitSecondTime(botClient, message, cancellationToken, delayMessageIds);
            return false;
        }

        return true;
    }

    private static async Task<bool> CheckForTimeOut(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken, Task<ChatCompletionCreateResponse> completionResultTask, int[] delayMessageIds)
    {
        var timeOutOver = Task.Delay(60000);
        var completedTaskAttempt3 = await Task.WhenAny(completionResultTask, timeOutOver);
        if (completedTaskAttempt3 == timeOutOver)
        {
            DeleteDelaysAlerts(botClient, message, cancellationToken, delayMessageIds);
            await TimeOutMessage(botClient, message, cancellationToken);
            return true;
        }

        return false;
    }

    private static async Task AskToWaitFirstTime(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken, int[] delayMessageIds)
    {
        var delayMessage = await botClient.SendTextMessageAsync(
            message.Chat.Id,
            "Ваш запрос обрабатывается, подождите пожалуйста...", cancellationToken: cancellationToken);
        delayMessageIds[0] = delayMessage.MessageId;
    }

    private static async Task AskToWaitSecondTime(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken, int[] delayMessageIds)
    {
        var delayMessage2 = await botClient.SendTextMessageAsync(
            message.Chat.Id,
            "Ваш запрос очень сложный, продолжайте ждать...", cancellationToken: cancellationToken);
        delayMessageIds[1] = delayMessage2.MessageId;
    }

    private static async Task TimeOutMessage(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(message.Chat.Id,
            "Простите, возникли неполадки, попробуйте ввести запрос ещё раз",
            cancellationToken: cancellationToken);
    }

    private static async Task DeleteDelaysAlerts(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken, int[] delayMessageIds)
    {
        foreach (var delayMessageId in delayMessageIds)
        {
            if (delayMessageId != 0) await botClient.DeleteMessageAsync(message.Chat.Id, delayMessageId, cancellationToken);
        }
    }
}