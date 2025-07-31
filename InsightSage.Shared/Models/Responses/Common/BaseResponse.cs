using System.Net;

namespace InsightSage.Shared.Models.Responses.Common
{
    public class BaseResponse
    {
        public HttpStatusCode Status { get; set; }
        public List<string> Errors { get; set; } = [];
        public string? ExceptionDetails { get; set; }
    }
}
