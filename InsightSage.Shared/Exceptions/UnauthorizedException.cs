using System.Net;

namespace InsightSage.Shared.Exceptions
{
    /// <summary>
    /// Exception thrown when user authentication is invalid or missing
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public UnauthorizedException() : this("Authentication is required. Please log in to continue.")
        {
        }

        public UnauthorizedException(string message) : base(message)
        {
            StatusCode = HttpStatusCode.Unauthorized;
        }

        public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = HttpStatusCode.Unauthorized;
        }
    }
}