using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PleasureBot;

public class Gpt3Handler : RequestHandler
{
    public override async Task<bool> Handle(Message message, ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        if (message.Text == "Gpt3")
        {
            await Gtp3AskToWritePrompt(botClient, message, cancellationToken);
            return true;
        }

        return _nextHandler != null && await _nextHandler.Handle(message, botClient, cancellationToken);
    }

    private static async Task Gtp3AskToWritePrompt(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(
            message.Chat.Id,
            "Пожалуйста, напишите запрос для GPT3",
            replyMarkup: new ForceReplyMarkup()
        );
    }
}