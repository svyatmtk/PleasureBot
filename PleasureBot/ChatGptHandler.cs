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
            await ChatGptAskToWritePrompt(message, botClient, cancellationToken);
            return true;
        }

        return _nextHandler != null && await _nextHandler.Handle(message, botClient, cancellationToken);
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