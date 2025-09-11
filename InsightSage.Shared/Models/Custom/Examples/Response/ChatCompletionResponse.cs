namespace InsightSage.Shared.Models.Custom.Examples.Response
{
    public class ChatCompletionResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Object { get; set; } = string.Empty;
        public long Created { get; set; }
        public string Model { get; set; } = string.Empty;
        public List<Choice> Choices { get; set; } = new();
        public Usage Usage { get; set; } = new();
    }
}
