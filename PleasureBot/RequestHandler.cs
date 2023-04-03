using Telegram.Bot;
using Telegram.Bot.Types;

namespace PleasureBot;

public abstract class RequestHandler
{
    protected RequestHandler _nextHandler;

    public RequestHandler SetNext(RequestHandler handler)
    {
        _nextHandler = handler;
         return handler;
    }

    public abstract Task<bool> Handle(Message message, ITelegramBotClient botClient,
        CancellationToken cancellationToken);
}