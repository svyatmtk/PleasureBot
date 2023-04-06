using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PleasureBot;

public class ChatGptHandler : RequestHandler
{
    public override async Task<bool> Handle(Message message, ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        if (HelpingInstruments.Compare(message.Text, "ChatGPT"))
        {
            if (await CheckRegistration(message, botClient, cancellationToken)) return true;
            await ChatGptAskToWritePrompt(message, botClient, cancellationToken);
            return true;
        }

        return _nextHandler != null && await _nextHandler.Handle(message, botClient, cancellationToken);
    }

    private static async Task<bool> CheckRegistration(Message message, ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        if (!SqLite.Registered(message))
        {
            await AskToRegister(message, botClient, cancellationToken);
            return true;
        }

        return false;
    }

    private static async Task AskToRegister(Message message, ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(
            message.Chat.Id,
            "Нужно активировать бота (нажмите кнопку start в меню)",
            cancellationToken: cancellationToken);
    }

    private static async Task ChatGptAskToWritePrompt(Message message, ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(
            message.Chat.Id,
            "Пожалуйста, напишите запрос для ChatGPT",
            replyMarkup: new ForceReplyMarkup(), cancellationToken: cancellationToken);
    }
}