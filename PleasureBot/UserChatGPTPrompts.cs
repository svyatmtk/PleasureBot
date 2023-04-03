using OpenAI.GPT3.ObjectModels.RequestModels;

namespace PleasureBot;

internal class UserChatGPTPrompts
{
    public long UserID { get; set; }
    public List<ChatMessage> ChatMessages { get; set; } = new();
    public string LastResponse { get; set; }
    public static Dictionary<long, UserChatGPTPrompts> UserPrompts { get; } = new();
}