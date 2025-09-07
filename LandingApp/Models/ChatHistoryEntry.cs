namespace LandingApp.Models
{
    public class ChatHistoryEntry
    {
        public string Role { get; set; } = ""; // "user" или "assistant"
        public string Content { get; set; } = "";
    }
}
