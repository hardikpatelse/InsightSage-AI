namespace InsightSage.Shared.Models.Entities
{
    public class ChatMessage : BaseEntity
    {
        public int? ChatSessionId { get; set; } = 0;
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
    }
}
