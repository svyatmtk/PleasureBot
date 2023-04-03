using Telegram.Bot;
using Telegram.Bot.Types;

namespace PleasureBot;

public class DefaultHandler : RequestHandler
{
    public override async Task<bool> Handle(Message message, ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(message.Chat.Id,
            "Простите, я не понял что вы написали",
            cancellationToken: cancellationToken);
        await HelpingInstruments.ShowStartMenu(botClient, message, cancellationToken);
        return true;
    }
}