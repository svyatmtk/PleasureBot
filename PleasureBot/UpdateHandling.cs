using System.ComponentModel.Design;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;
using Microsoft.VisualBasic;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading;
using OpenAI.GPT3.ObjectModels.ResponseModels.ImageResponseModel;
using OpenAI.GPT3.Interfaces;

namespace PleasureBot;

internal class UpdateHandling
{
    public static async Task Update(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message;

        if (CheckIfMessageHasBeenWritten(message))
        {
            var date = update.Message!.Date;
            var datemes = DateTime.Now - (date.AddHours(3));
            if (datemes.Seconds > 5)
            {
                Console.WriteLine("Старое сообщение");
            }
            else
            {
                await GiveResultIfTextTyped(botClient, message, cancellationToken, update);
            }
        } //улучшить код
    }

    private static async Task GiveResultIfTextTyped(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken, Update update)
    {
        if (message.Type != MessageType.Text)
        {
            Console.WriteLine(message.Type);
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text:
                "Бот пока не умеет работать не с текстом. Пожалуйста, выберите интересующую вас нейросеть и" +
                " введите текст запроса для неё",
                cancellationToken: cancellationToken);
            await ShowChoiceMenu(botClient, message, cancellationToken);
        } //улучшить код

        else if (IfStartButtonPushed(message))
        {
            await ShowChoiceMenu(botClient, message, cancellationToken);
        }

        else if (IfUserPushedGTP3Button(message))
        {
            await Gtp3AskToWritePrompt(botClient, message, cancellationToken);
        }

        else if (IfUsertPushedDalle2Button(message))
        {
            await AskUserToWritePromptForDalle2(botClient, message);
        }

        else if (UserWantToTalkWithGpt3(message))
        {
            await IfGtp3ButtonPushedThenConversationStart(botClient, message);
            await ShowChoiceMenu(botClient, message, cancellationToken);
        }

        else if (UserWantToMakePhotoFromDalle2(message))
        {
            await TrySendPromptToDalle2AndGetResponse(botClient, message);
            await ShowChoiceMenu(botClient, message, cancellationToken);
        }

        else if (message.Text == "ChatGPT")
        {
                await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "уже 5 утра и щас мои глаза хотят закрыться навсегда...клянусь ниже уже на 50% готовый код запроса к чатгпт, утром доделаю)))))",
                replyToMessageId: message.MessageId, 
                cancellationToken: cancellationToken);
            /*var completionResult = await OpenAiInitialization.OpenAiServicesInit().ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem("You are a helpful assistant."),
                    ChatMessage.FromUser("напиши историю про мышь, которая стала человеком"),
                    ChatMessage.FromAssistant("The Los Angeles Dodgers won the World Series in 2020."),
                    ChatMessage.FromUser("Where was it played?")
                },
                Model = Models.ChatGpt3_5Turbo,
                MaxTokens = 1024//optional
            });
            if(completionResult.Successful)
            {
                foreach (var choices in completionResult.Choices)
                {
                    Console.WriteLine(choices.Message.Role);
                }
            }*/
            await ShowChoiceMenu(botClient, message, cancellationToken);
        }
        else
        {
            await ShowChoiceMenu(botClient, message, cancellationToken);
        }
    }

    private static bool CheckIfMessageHasBeenWritten(Message? message)
        => message != null;

    private static bool UserWantToMakePhotoFromDalle2(Message message)
        => message.ReplyToMessage != null && message.ReplyToMessage.Text.Contains("Напишите запрос для DALL·E 2");

    private static bool UserWantToTalkWithGpt3(Message message)
        => message.ReplyToMessage != null && message.ReplyToMessage.Text.Contains("Напишите запрос для GPT3");

    private static bool IfUsertPushedDalle2Button(Message message)
        => message.Text == "DALL·E";

    private static bool IfUserPushedGTP3Button(Message message)
        => message.Text == "Gpt3";

    private static bool IfStartButtonPushed(Message message)
        => message.Text == "/start";


    private static async Task ShowChoiceMenu(ITelegramBotClient botClient, Message message,CancellationToken cancellationToken)
    {
        var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "Gpt3", "DALL·E", "ChatGPT" }
        })
        {
            ResizeKeyboard = true
        };
        var sendMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Выберите какую нейросеть использовать",
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
    }

    private static async Task Gtp3AskToWritePrompt(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(
            message.Chat.Id,
            replyToMessageId: message.MessageId,
            text:"Напишите запрос для GPT3",
            replyMarkup: new ForceReplyMarkup { Selective = true },
            cancellationToken: cancellationToken
        );
    }

    private static async Task AskUserToWritePromptForDalle2(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendTextMessageAsync(
            message.Chat.Id,
            text: "Напишите запрос для DALL·E 2",
            replyToMessageId: message.MessageId,
            replyMarkup: new ForceReplyMarkup { Selective = true }
        );
    }

    private static async Task IfGtp3ButtonPushedThenConversationStart(ITelegramBotClient botClient, Message message)
    {
        await TryGetResponseFromGpt3(botClient, message);
    }

    private static async Task TrySendPromptToDalle2AndGetResponse(ITelegramBotClient botClient, Message message)
    {
        try
        {
            var imageResult = await SendRequestToDalle2(message);
            await GiveResultFromDalle2(imageResult, botClient, message);
        }
        catch (Exception e)
        {
            WriteExceptionMessageToUser(botClient, message);
        }
    }

    private static async Task TryGetResponseFromGpt3(ITelegramBotClient botClient, Message message)
    {
        try
        {
            await GettingResponseFromGpt3(botClient, message);
        }
        catch (Exception)
        {
            WriteExceptionMessageToUser(botClient, message);
        }
    }

    private static async Task GettingResponseFromGpt3(ITelegramBotClient botClient, Message message)
    {
        var prompt = message.Text;

        var completionResult = await OpenAiInitialization.OpenAiServicesInit().Completions.CreateCompletion(
            new CompletionCreateRequest()
            {
                Prompt = prompt,
                Model = Models.TextDavinciV3,
                MaxTokens = 1000,
                Temperature = 0.7f,
                TopP = 1
            });

        var text = completionResult.Choices.FirstOrDefault()?.Text;

        if (text != null)
        {
            var botMessage = await botClient.SendTextMessageAsync(
                message.Chat.Id,
                text,
                replyToMessageId: message.MessageId);
            LogOutput(message, botMessage);
        }
    }

    private static async Task<ImageCreateResponse> SendRequestToDalle2(Message message)
    {
        var imageResult = await OpenAiInitialization.OpenAiServicesInit().Image.CreateImage(new ImageCreateRequest()
        {
            Prompt = message.Text,
            N = 2,
            Size = StaticValues.ImageStatics.Size.Size1024,
            ResponseFormat = StaticValues.ImageStatics.ResponseFormat.Url,
            User = "TestUser"
        });
        return imageResult;
    }

    private static async Task GiveResultFromDalle2(ImageCreateResponse imageResult, ITelegramBotClient botClient,
        Message message)
    {
        var answers = imageResult.Results.Select(r => r.Url).ToArray();

        var botMessage = await botClient.SendMediaGroupAsync(
            chatId: message.Chat.Id,
            media: new IAlbumInputMedia[]
            {
                new InputMediaPhoto(answers[0]),
                new InputMediaPhoto(answers[1])
            },
            replyToMessageId: message.MessageId);
        LogOutput(message, botMessage);
    }

    private static void WriteExceptionMessageToUser(ITelegramBotClient botClient, Message message)
    {
        botClient.SendTextMessageAsync(
            message.Chat.Id,
            text: "Что - то не так с нейросетями, повторите попытку позже");
    }


    private static void LogOutput(Message message, Message botMessage)
    {
        Console.WriteLine(
            $"User {message.Chat.FirstName} {message.Chat.LastName} sent message " +
            $" \"{message.Text}\" at {message.Date.ToLocalTime()} and bot {botMessage.From.FirstName} " +
            $"answered {botMessage.Text} at {message.Date.ToLocalTime()}\n");

        Console.WriteLine("-----------------------------------------------------\n");

    }

    private static void LogOutput(Message message, Message[] botMessage)
    {
        foreach (var message1 in botMessage)
        {
            Console.WriteLine(
                $"User {message.Chat.FirstName} {message.Chat.LastName} sent message " +
                $" \"{message.Text}\" at {message.Date.ToLocalTime()} and bot {message1.From.FirstName} " +
                $"answered {message1.Photo} at {message.Date.ToLocalTime()}\n");
        }
        Console.WriteLine("-----------------------------------------------------\n");
    }
}