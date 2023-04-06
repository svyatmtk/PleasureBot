namespace PleasureBot;

public static class RequestHandlerExtensions
{
    public static RequestHandler Build(this RequestHandler handler)
    {
        var startHandler = new StartHandler();
        var gpt3Handler = new Gpt3Handler();
        var gpt3PromptHandler = new Gpt3PromptHandler();
        var dalle2Handler = new Dalle2Handler();
        var dalle2PromptHandler = new Dalle2PromptHandler();
        var chatGptHandler = new ChatGptHandler();
        var chatGptPromptHandler = new ChatGptPromptHandler();
        var deleteContextHandler = new DeleteContextHandler();
        var byuSubscriptionHandler = new ByuSubscriptionHandler();
        var defaultHandler = new DefaultHandler();

        handler.SetNext(startHandler)
            .SetNext(gpt3Handler)
            .SetNext(gpt3PromptHandler)
            .SetNext(dalle2Handler)
            .SetNext(dalle2PromptHandler)
            .SetNext(chatGptHandler)
            .SetNext(chatGptPromptHandler)
            .SetNext(deleteContextHandler)
            .SetNext(byuSubscriptionHandler)
            .SetNext(defaultHandler);

        return handler;
    }
}