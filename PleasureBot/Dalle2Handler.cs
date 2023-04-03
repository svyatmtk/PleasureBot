using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PleasureBot;

public class Dalle2Handler : RequestHandler
{
    public override async Task<bool> Handle(Message message, ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        if (HelpingInstruments.Compare(message.Text!, "DALL·E"))
        {
            await AskToWritePromptToDalle(botClient, message);
            return true;
        }

        return _nextHandler != null && await _nextHandler.Handle(message, botClient, cancellationToken);
    }

    public async Task AskToWritePromptToDalle(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendTextMessageAsync(
            message.Chat.Id,
            "Напишите запрос для Dalle2",
            replyMarkup: new ForceReplyMarkup());
    }
}