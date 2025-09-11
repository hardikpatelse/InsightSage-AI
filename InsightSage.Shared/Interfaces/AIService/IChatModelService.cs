using InsightSage.Shared.Models;

namespace InsightSage.Shared.Interfaces.AIServices
{
    public interface IChatModelService
    {
        Task<string> SendMessageAsync(string model, string message, string connectionId);
        //Task<List<FunctionCallResult>> ChatWithFunctionSupportAsync(string model, List<Dictionary<string, string>> userMessage, string connectionId);
    }
}
