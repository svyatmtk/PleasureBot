using System.ComponentModel.Design;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PleasureBot
{
    internal class ByuSubscriptionHandler : RequestHandler
    {
        public override async Task<bool> Handle(Message message, ITelegramBotClient botClient,
            CancellationToken cancellationToken)
        {
            if (HelpingInstruments.Compare(message.Text, "/buysubscription"))
            {
                await TryToSetSub(message, botClient, cancellationToken);
                return true;
            }
            else
            {
                return _nextHandler != null && await _nextHandler.Handle(message, botClient, cancellationToken);
            }
        }

        private static async Task TryToSetSub(Message message, ITelegramBotClient botClient,
            CancellationToken cancellationToken)
        {
            if (SqLite.SetSubscription(message))
            {
                await SubSuccessMessage(message, botClient, cancellationToken);
            }
            else
            {
                await AlreadyHaveSubMessage(message, botClient, cancellationToken);
            }
        }

        private static async Task AlreadyHaveSubMessage(Message message, ITelegramBotClient botClient,
            CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                "Ты уже подписан, олух. Чо,второй раз заплатить хочешь? ну ты и дебил господи..",
                cancellationToken: cancellationToken);
        }

        private static async Task SubSuccessMessage(Message message, ITelegramBotClient botClient,
            CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                "Поздравляю, подписка ваша!))",
                cancellationToken: cancellationToken);
        }
    }
}