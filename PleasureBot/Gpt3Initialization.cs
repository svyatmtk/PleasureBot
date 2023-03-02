using OpenAI.GPT3;
using OpenAI.GPT3.Managers;

namespace PleasureBot;

internal static class Gpt3Initialization
{
    public static OpenAIService Gpt3Init()
    {
        var openAiServices = new OpenAIService(new OpenAiOptions()
        {
            ApiKey = Environment.GetEnvironmentVariable("Api_Key_Open_Ai")!
        });
        return openAiServices;
    }
}