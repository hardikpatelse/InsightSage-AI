using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using InsightSage.Shared.Interfaces.Others;
using InsightSage.Shared.Models.Custom;
using Microsoft.Extensions.Logging;

namespace InsightSage.Infrastructure
{
    /// <summary>
    /// Implementation of IApiConnector for making HTTP requests to 3rd party APIs
    /// </summary>
    public class ApiConnector : IApiConnector
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ApiConnector> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiConnector(IHttpClientFactory httpClientFactory, ILogger<ApiConnector> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
        }

        public async Task<ExternalApiResponse> GetAsync(string url, IDictionary<string, string>? headers = null, int timeout = 30, CancellationToken cancellationToken = default)
        {
            return await SendAsync(HttpMethod.Get, url, null, null, headers, timeout, cancellationToken);
        }

        public async Task<ExternalApiResponse> PostAsync(string url, object? body = null, string contentType = "application/json", IDictionary<string, string>? headers = null, int timeout = 30, CancellationToken cancellationToken = default)
        {
            return await SendAsync(HttpMethod.Post, url, body, contentType, headers, timeout, cancellationToken);
        }

        public async Task<ExternalApiResponse> PutAsync(string url, object? body = null, string contentType = "application/json", IDictionary<string, string>? headers = null, int timeout = 30, CancellationToken cancellationToken = default)
        {
            return await SendAsync(HttpMethod.Put, url, body, contentType, headers, timeout, cancellationToken);
        }

        public async Task<ExternalApiResponse> PatchAsync(string url, object? body = null, string contentType = "application/json", IDictionary<string, string>? headers = null, int timeout = 30, CancellationToken cancellationToken = default)
        {
            return await SendAsync(HttpMethod.Patch, url, body, contentType, headers, timeout, cancellationToken);
        }

        public async Task<ExternalApiResponse> DeleteAsync(string url, IDictionary<string, string>? headers = null, int timeout = 30, CancellationToken cancellationToken = default)
        {
            return await SendAsync(HttpMethod.Delete, url, null, null, headers, timeout, cancellationToken);
        }

        public async Task<ExternalApiResponse> HeadAsync(string url, IDictionary<string, string>? headers = null, int timeout = 30, CancellationToken cancellationToken = default)
        {
            return await SendAsync(HttpMethod.Head, url, null, null, headers, timeout, cancellationToken);
        }

        public async Task<ExternalApiResponse> OptionsAsync(string url, IDictionary<string, string>? headers = null, int timeout = 30, CancellationToken cancellationToken = default)
        {
            return await SendAsync(HttpMethod.Options, url, null, null, headers, timeout, cancellationToken);
        }

        public async Task<ExternalApiResponse> SendAsync(HttpMethod method, string url, object? body = null, string? contentType = null, IDictionary<string, string>? headers = null, int timeout = 30, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var response = new ExternalApiResponse();

            try
            {
                _logger.LogDebug("Sending {Method} request to {Url}", method, url);

                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(timeout);

                using var request = new HttpRequestMessage(method, url);

                // Add headers if provided
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        // Try to add to request headers first
                        if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
                        {
                            // If it fails, it might be a content header, we'll add it after creating content
                            _logger.LogDebug("Header {HeaderName} will be added to content headers", header.Key);
                        }
                    }
                }

                // Add body content if provided
                if (body != null && (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch))
                {
                    HttpContent content = CreateHttpContent(body, contentType ?? "application/json");
                    
                    // Add headers that might be content headers
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            if (!request.Headers.Contains(header.Key))
                            {
                                if (!content.Headers.TryAddWithoutValidation(header.Key, header.Value))
                                {
                                    _logger.LogWarning("Failed to add header {HeaderName} with value {HeaderValue}", header.Key, header.Value);
                                }
                            }
                        }
                    }
                    
                    request.Content = content;
                }

                // Send the request
                using var httpResponse = await httpClient.SendAsync(request, cancellationToken);

                // Populate response object
                response.StatusCode = httpResponse.StatusCode;
                response.Content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                response.ContentType = httpResponse.Content.Headers.ContentType?.ToString();
                response.ContentLength = httpResponse.Content.Headers.ContentLength;

                // Add response headers
                foreach (var header in httpResponse.Headers)
                {
                    response.Headers[header.Key] = string.Join(", ", header.Value);
                }

                // Add content headers
                foreach (var header in httpResponse.Content.Headers)
                {
                    if (!response.Headers.ContainsKey(header.Key))
                    {
                        response.Headers[header.Key] = string.Join(", ", header.Value);
                    }
                }

                stopwatch.Stop();
                response.Duration = stopwatch.Elapsed;

                _logger.LogDebug("Received {StatusCode} response from {Url} in {Duration}ms", 
                    response.StatusCode, url, response.Duration.TotalMilliseconds);

                return response;
            }
            catch (TaskCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();
                response.Duration = stopwatch.Elapsed;
                response.StatusCode = HttpStatusCode.RequestTimeout;
                response.Exception = ex;
                
                _logger.LogWarning(ex, "Request to {Url} timed out after {Timeout}s", url, timeout);
                
                return response;
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                response.Duration = stopwatch.Elapsed;
                response.StatusCode = HttpStatusCode.BadGateway;
                response.Exception = ex;
                
                _logger.LogError(ex, "HTTP request exception occurred for {Method} {Url}", method, url);
                
                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                response.Duration = stopwatch.Elapsed;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Exception = ex;
                
                _logger.LogError(ex, "Unexpected exception occurred for {Method} {Url}", method, url);
                
                return response;
            }
        }

        private HttpContent CreateHttpContent(object body, string contentType)
        {
            return body switch
            {
                string stringBody => new StringContent(stringBody, Encoding.UTF8, contentType),
                byte[] byteBody => new ByteArrayContent(byteBody),
                Stream streamBody => new StreamContent(streamBody),
                HttpContent httpContent => httpContent,
                _ => new StringContent(JsonSerializer.Serialize(body, _jsonOptions), Encoding.UTF8, contentType)
            };
        }
    }
}