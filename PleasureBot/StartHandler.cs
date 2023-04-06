using Telegram.Bot;
using Telegram.Bot.Types;

namespace PleasureBot;

public class StartHandler : RequestHandler
{
    public override async Task<bool> Handle(Message message, ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        if (message.Text == "/start")
        {
            await HelpingInstruments.ShowStartMenu(botClient, message, cancellationToken);
            SqLite.RegisterUsers(message);
            return true;
        }

        return _nextHandler != null && await _nextHandler.Handle(message, botClient, cancellationToken);
    }
}