using Telegram.Bot;
using Telegram.Bot.Types;

namespace PleasureBot
{
    internal class DeleteContextHandler : RequestHandler
    {
        public override async Task<bool> Handle(Message message, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            if (HelpingInstruments.Compare(message?.Text!, "/removecontext"))
            {
                SqLite.DeleteUserMessages(message);
                botClient.SendTextMessageAsync(message.Chat.Id,
                    "Контекст ChatGPT сброшен, он забыл предыдущие сообщения", 
                    cancellationToken: cancellationToken);
                return true;
            }
            else
            {
                return _nextHandler != null && await _nextHandler.Handle(message, botClient, cancellationToken);
            }
        }
    }
}
