using InsightSage.Shared.Interfaces.Others;
using InsightSage.Shared.Models.Custom.Examples;
using InsightSage.Shared.Models.Custom.Examples.Response;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InsightSage.Infrastructure.Examples
{
    /// <summary>
    /// Example service demonstrating how to use IApiConnector for 3rd party API integration
    /// This service shows integration with a hypothetical external AI service for chat completions
    /// </summary>
    public class ExternalAiService
    {
        private readonly IApiConnector _apiConnector;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ExternalAiService> _logger;

        public ExternalAiService(
            IApiConnector apiConnector,
            IConfiguration configuration,
            ILogger<ExternalAiService> logger)
        {
            _apiConnector = apiConnector;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Gets a chat completion from an external AI service
        /// </summary>
        public async Task<ChatCompletionResponse?> GetChatCompletionAsync(string prompt, CancellationToken cancellationToken = default)
        {
            try
            {
                var baseUrl = _configuration["ExternalAi:BaseUrl"] ?? "https://api.openai.com/v1";
                var apiKey = _configuration["ExternalAi:ApiKey"];

                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("External AI API key not configured");
                    return null;
                }

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 150,
                    temperature = 0.7
                };

                var headers = new Dictionary<string, string>
                {
                    ["Authorization"] = $"Bearer {apiKey}",
                    ["Content-Type"] = "application/json"
                };

                _logger.LogInformation("Sending request to external AI service for prompt: {Prompt}", prompt.Substring(0, Math.Min(50, prompt.Length)));

                var response = await _apiConnector.PostAsync(
                    url: $"{baseUrl}/chat/completions",
                    body: requestBody,
                    headers: headers,
                    timeout: 30,
                    cancellationToken: cancellationToken
                );

                if (response.IsSuccess)
                {
                    var completionResponse = response.GetContent<ChatCompletionResponse>();
                    _logger.LogInformation("Successfully received response from external AI service");
                    return completionResponse;
                }
                else
                {
                    _logger.LogError("External AI service returned error: {StatusCode} - {Content}",
                        response.StatusCode, response.Content);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling external AI service");
                return null;
            }
        }

        /// <summary>
        /// Gets weather information from a weather API
        /// </summary>
        public async Task<WeatherResponse?> GetWeatherAsync(string city, CancellationToken cancellationToken = default)
        {
            try
            {
                var baseUrl = _configuration["WeatherApi:BaseUrl"] ?? "https://api.openweathermap.org/data/2.5";
                var apiKey = _configuration["WeatherApi:ApiKey"];

                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogWarning("Weather API key not configured");
                    return null;
                }

                var queryParams = $"?q={Uri.EscapeDataString(city)}&appid={apiKey}&units=metric";

                _logger.LogInformation("Getting weather for city: {City}", city);

                var response = await _apiConnector.GetAsync(
                    url: $"{baseUrl}/weather{queryParams}",
                    timeout: 10,
                    cancellationToken: cancellationToken
                );

                if (response.IsSuccess)
                {
                    var weatherResponse = response.GetContent<WeatherResponse>();
                    _logger.LogInformation("Successfully received weather data for {City}", city);
                    return weatherResponse;
                }
                else
                {
                    _logger.LogError("Weather API returned error: {StatusCode} - {Content}",
                        response.StatusCode, response.Content);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling weather API for city: {City}", city);
                return null;
            }
        }

        /// <summary>
        /// Sends a notification to an external webhook
        /// </summary>
        public async Task<bool> SendWebhookNotificationAsync(string webhookUrl, object payload, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Sending webhook notification to: {Url}", webhookUrl);

                var headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = "application/json",
                    ["User-Agent"] = "InsightSage-Webhook/1.0"
                };

                var response = await _apiConnector.PostAsync(
                    url: webhookUrl,
                    body: payload,
                    headers: headers,
                    timeout: 15,
                    cancellationToken: cancellationToken
                );

                if (response.IsSuccess)
                {
                    _logger.LogInformation("Webhook notification sent successfully");
                    return true;
                }
                else
                {
                    _logger.LogError("Webhook notification failed: {StatusCode} - {Content}",
                        response.StatusCode, response.Content);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending webhook notification to: {Url}", webhookUrl);
                return false;
            }
        }

        /// <summary>
        /// Example of using custom headers and authentication
        /// </summary>
        public async Task<UserProfileResponse?> GetUserProfileFromExternalServiceAsync(string userId, string accessToken)
        {
            try
            {
                var baseUrl = _configuration["ExternalService:BaseUrl"];

                if (string.IsNullOrEmpty(baseUrl))
                {
                    _logger.LogError("External service base URL not configured");
                    return null;
                }

                var headers = new Dictionary<string, string>
                {
                    ["Authorization"] = $"Bearer {accessToken}",
                    ["Accept"] = "application/json",
                    ["X-API-Version"] = "v2",
                    ["X-Request-ID"] = Guid.NewGuid().ToString()
                };

                var response = await _apiConnector.GetAsync(
                    url: $"{baseUrl}/users/{userId}/profile",
                    headers: headers,
                    timeout: 10
                );

                return response.IsSuccess ? response.GetContent<UserProfileResponse>() : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile for user: {UserId}", userId);
                return null;
            }
        }
    }
}