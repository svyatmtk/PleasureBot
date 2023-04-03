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

        CheckIfDialogAlreadyExists(message, UserChatGPTPrompts.UserPrompts);

        var userDialogState = UserChatGPTPrompts.UserPrompts[message.Chat.Id];
        userDialogState.ChatMessages.Add(ChatMessage.FromUser(prompt!));

        await GenerateResponseWithTimeout(botClient, message, cancellationToken, prompt!, userDialogState);
    }

    private static void CheckIfDialogAlreadyExists(Message message, Dictionary<long, UserChatGPTPrompts> userPrompts)
    {
        if (!userPrompts.ContainsKey(message.Chat.Id))
            userPrompts[message.Chat.Id] = new UserChatGPTPrompts { UserID = message.Chat.Id };
    }

    private static async Task GenerateResponseWithTimeout(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken, string prompt, UserChatGPTPrompts userDialogState)
    {
        var delayMessageIds = new int[2];
        var stopWatch = Stopwatch.StartNew();

        var completionResultTask = OpenAiInitialization.OpenAiServicesInit().ChatCompletion.CreateCompletion(
            new ChatCompletionCreateRequest
            {
                Messages = userDialogState.ChatMessages,
                Model = Models.ChatGpt3_5Turbo,
                MaxTokens = 2048
            });

        var timeoutTask = Task.Delay(5000);
        var completedTask = await Task.WhenAny(completionResultTask, timeoutTask);
        stopWatch.Stop();

        if (completedTask == timeoutTask)
            await AskToWaitFirstTime(botClient, message, cancellationToken, delayMessageIds);

        var timeoutTaskSecond = Task.Delay(15000);
        var completedTaskAttempt2 = await Task.WhenAny(completionResultTask, timeoutTaskSecond);

        if (completedTaskAttempt2 == timeoutTaskSecond)
            await AskToWaitSecondTime(botClient, message, cancellationToken, delayMessageIds);

        var completionResult = await completionResultTask;

        foreach (var delayMessageId in delayMessageIds)
            await DeleteAskToWaitMessages(botClient, message, cancellationToken, delayMessageId);

        userDialogState.LastResponse = completionResult.Choices.First().Message.Content;

        await TryToGetAnswerFromChatGpt(botClient, message, userDialogState, completionResult);
    }

    private static async Task TryToGetAnswerFromChatGpt(ITelegramBotClient botClient, Message message,
        UserChatGPTPrompts userDialogState, ChatCompletionCreateResponse completionResult)

    {
        userDialogState.LastResponse = completionResult.Choices.First().Message.Content;

        await botClient.SendTextMessageAsync(
            message.Chat.Id,
            userDialogState.LastResponse,
            replyToMessageId: message.MessageId);

        HelpingInstruments.LogOutput(message, userDialogState.LastResponse);
    }

    private static async Task AskToWaitSecondTime(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken, int[] delayMessageIds)
    {
        var delayMessage2 = await botClient.SendTextMessageAsync(
            message.Chat.Id,
            "Ваш запрос очень сложный, продолжайте ждать...", cancellationToken: cancellationToken);
        delayMessageIds[1] = delayMessage2.MessageId;
    }

    private static async Task AskToWaitFirstTime(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken, int[] delayMessageIds)
    {
        var delayMessage = await botClient.SendTextMessageAsync(
            message.Chat.Id,
            "Ваш запрос обрабатывается, подождите пожалуйста...", cancellationToken: cancellationToken);
        delayMessageIds[0] = delayMessage.MessageId;
    }

    private static async Task DeleteAskToWaitMessages(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken, int delayMessageId)
    {
        if (delayMessageId != 0) await botClient.DeleteMessageAsync(message.Chat.Id, delayMessageId, cancellationToken);
    }
}