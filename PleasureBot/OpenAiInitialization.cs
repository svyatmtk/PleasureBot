using OpenAI.GPT3;
using OpenAI.GPT3.Managers;

namespace PleasureBot;

internal static class OpenAiInitialization
{
    public static OpenAIService OpenAiServicesInit()
    {
        var openAiServices = new OpenAIService(new OpenAiOptions
        {
            ApiKey = Environment.GetEnvironmentVariable("Api_Key_Open_Ai")!
        });
        return openAiServices;
    }
}