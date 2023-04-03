using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels.ResponseModels.ImageResponseModel;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PleasureBot;

public class Dalle2PromptHandler : RequestHandler
{
    public override async Task<bool> Handle(Message message, ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        if (HelpingInstruments.Compare(message.ReplyToMessage?.Text!, "Напишите запрос для Dalle2"))
        {
            await SendPromptToDalle(botClient, message, cancellationToken);
            HelpingInstruments.ShowStartMenu(botClient, message, cancellationToken);
            return true;
        }

        return _nextHandler != null && await _nextHandler.Handle(message, botClient, cancellationToken);
    }

    public async Task SendPromptToDalle(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        var imageResult = await OpenAiInitialization.OpenAiServicesInit().Image.CreateImage(new ImageCreateRequest
        {
            Prompt = message.Text,
            N = 2,
            Size = StaticValues.ImageStatics.Size.Size1024,
            ResponseFormat = StaticValues.ImageStatics.ResponseFormat.Url,
            User = "TestUser"
        });

        await GiveResultFromDalle2(imageResult, botClient, message);
    }

    private static async Task GiveResultFromDalle2(ImageCreateResponse imageResult, ITelegramBotClient botClient,
        Message message)
    {
        var answers = imageResult.Results.Select(r => r.Url).ToArray();

        await botClient.SendMediaGroupAsync(
            message.Chat.Id,
            new IAlbumInputMedia[]
            {
                new InputMediaPhoto(answers[0]),
                new InputMediaPhoto(answers[1])
            },
            replyToMessageId: message.MessageId);

        ShowResult(imageResult, message);
    }

    private static void ShowResult(ImageCreateResponse imageResult, Message message)
    {
        HelpingInstruments.LogOutput(message, imageResult.Results[0].Url);
        HelpingInstruments.LogOutput(message, imageResult.Results[1].Url);
    }
}