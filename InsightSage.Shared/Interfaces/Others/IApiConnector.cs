using InsightSage.Shared.Models.Custom;
using System.Net;

namespace InsightSage.Shared.Interfaces.Others
{
    /// <summary>
    /// Interface for making HTTP requests to 3rd party APIs
    /// </summary>
    public interface IApiConnector
    {
        /// <summary>
        /// Makes an HTTP GET request
        /// </summary>
        /// <param name="url">The URL to send the request to</param>
        /// <param name="headers">Optional headers to include in the request</param>
        /// <param name="timeout">Optional timeout in seconds (default: 30)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>API response containing status, content, and headers</returns>
        Task<ExternalApiResponse> GetAsync(string url, IDictionary<string, string>? headers = null, int timeout = 30, CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes an HTTP POST request
        /// </summary>
        /// <param name="url">The URL to send the request to</param>
        /// <param name="body">The request body (can be string, object, or byte array)</param>
        /// <param name="contentType">Content type for the request body (default: application/json)</param>
        /// <param name="headers">Optional headers to include in the request</param>
        /// <param name="timeout">Optional timeout in seconds (default: 30)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>API response containing status, content, and headers</returns>
        Task<ExternalApiResponse> PostAsync(string url, object? body = null, string contentType = "application/json", IDictionary<string, string>? headers = null, int timeout = 30, CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes an HTTP PUT request
        /// </summary>
        /// <param name="url">The URL to send the request to</param>
        /// <param name="body">The request body (can be string, object, or byte array)</param>
        /// <param name="contentType">Content type for the request body (default: application/json)</param>
        /// <param name="headers">Optional headers to include in the request</param>
        /// <param name="timeout">Optional timeout in seconds (default: 30)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>API response containing status, content, and headers</returns>
        Task<ExternalApiResponse> PutAsync(string url, object? body = null, string contentType = "application/json", IDictionary<string, string>? headers = null, int timeout = 30, CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes an HTTP PATCH request
        /// </summary>
        /// <param name="url">The URL to send the request to</param>
        /// <param name="body">The request body (can be string, object, or byte array)</param>
        /// <param name="contentType">Content type for the request body (default: application/json)</param>
        /// <param name="headers">Optional headers to include in the request</param>
        /// <param name="timeout">Optional timeout in seconds (default: 30)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>API response containing status, content, and headers</returns>
        Task<ExternalApiResponse> PatchAsync(string url, object? body = null, string contentType = "application/json", IDictionary<string, string>? headers = null, int timeout = 30, CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes an HTTP DELETE request
        /// </summary>
        /// <param name="url">The URL to send the request to</param>
        /// <param name="headers">Optional headers to include in the request</param>
        /// <param name="timeout">Optional timeout in seconds (default: 30)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>API response containing status, content, and headers</returns>
        Task<ExternalApiResponse> DeleteAsync(string url, IDictionary<string, string>? headers = null, int timeout = 30, CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes an HTTP HEAD request
        /// </summary>
        /// <param name="url">The URL to send the request to</param>
        /// <param name="headers">Optional headers to include in the request</param>
        /// <param name="timeout">Optional timeout in seconds (default: 30)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>API response containing status and headers (no content for HEAD requests)</returns>
        Task<ExternalApiResponse> HeadAsync(string url, IDictionary<string, string>? headers = null, int timeout = 30, CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes an HTTP OPTIONS request
        /// </summary>
        /// <param name="url">The URL to send the request to</param>
        /// <param name="headers">Optional headers to include in the request</param>
        /// <param name="timeout">Optional timeout in seconds (default: 30)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>API response containing status, content, and headers</returns>
        Task<ExternalApiResponse> OptionsAsync(string url, IDictionary<string, string>? headers = null, int timeout = 30, CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes a custom HTTP request with full control over the HTTP method and content
        /// </summary>
        /// <param name="method">HTTP method to use</param>
        /// <param name="url">The URL to send the request to</param>
        /// <param name="body">The request body (can be string, object, or byte array)</param>
        /// <param name="contentType">Content type for the request body</param>
        /// <param name="headers">Optional headers to include in the request</param>
        /// <param name="timeout">Optional timeout in seconds (default: 30)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>API response containing status, content, and headers</returns>
        Task<ExternalApiResponse> SendAsync(HttpMethod method, string url, object? body = null, string? contentType = null, IDictionary<string, string>? headers = null, int timeout = 30, CancellationToken cancellationToken = default);
    }

    
}