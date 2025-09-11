namespace InsightSage.Shared.Models.Custom
{
    public class AIProcessingRequest
    {
        public int SessionId { get; set; }
        public int UserMessageId { get; set; }
        public Guid UserId { get; set; }
        public string UserMessage { get; set; } = string.Empty;
        public string? PlatformSlug { get; set; }
        public string? Model { get; set; }
        public int? MaxTokens { get; set; }
        public double? Temperature { get; set; }
    }
}
