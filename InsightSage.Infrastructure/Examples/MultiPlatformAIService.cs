using InsightSage.Shared.Interfaces.Others;
using InsightSage.Shared.Interfaces.Services;
using InsightSage.Shared.Models.Custom.Examples.Response;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InsightSage.Infrastructure.Examples
{
    /// <summary>
    /// Example service demonstrating how to use AIPlatform entity with IApiConnector
    /// for multi-platform AI API integration
    /// </summary>
    public class MultiPlatformAIService
    {
        private readonly IApiConnector _apiConnector;
        private readonly IAIPlatformService<AIPlatformResponse<List<AIPlatform>>, AIPlatformResponse<AIPlatform>, AIPlatform> _aiPlatformService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MultiPlatformAIService> _logger;

        public MultiPlatformAIService(
            IApiConnector apiConnector,
            IAIPlatformService<AIPlatformResponse<List<AIPlatform>>, AIPlatformResponse<AIPlatform>, AIPlatform> aiPlatformService,
            IConfiguration configuration,
            ILogger<MultiPlatformAIService> logger)
        {
            _apiConnector = apiConnector;
            _aiPlatformService = aiPlatformService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Gets a chat completion from any configured AI platform
        /// </summary>
        /// <param name="platformSlug">The platform identifier (e.g., "openai", "anthropic", "openrouter")</param>
        /// <param name="model">The model name to use</param>
        /// <param name="messages">Array of chat messages</param>
        /// <param name="maxTokens">Maximum tokens to generate</param>
        /// <param name="temperature">Temperature for randomness</param>
        /// <returns>Chat completion response</returns>
        public async Task<ChatCompletionResponse?> GetChatCompletionAsync(
            string platformSlug,
            string model,
            object[] messages,
            int maxTokens = 150,
            double temperature = 0.7)
        {
            try
            {
                // Get platform configuration using the service
                var platformResponse = await _aiPlatformService.GetActiveBySlugAsync(platformSlug);
                
                if (platformResponse.Result == null)
                {
                    _logger.LogError("AI platform '{PlatformSlug}' not found or inactive", platformSlug);
                    return null;
                }

                var platform = platformResponse.Result;

                // Get API key from configuration
                var apiKey = _configuration[$"AIPlatforms:{platform.Name}:ApiKey"];
                if (platform.RequiresApiKey && string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("API key not configured for platform '{PlatformName}'", platform.Name);
                    return null;
                }

                // Prepare request body (this would vary by platform)
                var requestBody = CreateChatCompletionRequest(platform.Name, model, messages, maxTokens, temperature);

                // Prepare headers
                var headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = "application/json"
                };

                // Add authentication header if required
                if (platform.RequiresApiKey && !string.IsNullOrEmpty(apiKey))
                {
                    var authValue = platform.AuthHeaderFormat
                        .Replace("{api_key}", apiKey)
                        .Replace("{token}", apiKey);
                    headers[platform.AuthHeaderName] = authValue;
                }

                // Determine the appropriate endpoint
                var endpoint = GetChatCompletionEndpoint(platform.Name);
                var fullUrl = $"{platform.ApiBaseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
            
                // TODO: Temporary for testing, to be removed
                _logger.LogWarning("BYE-BYE MultiPlatformAIService: {Url}", fullUrl);

                // Make the API call
                var response = await _apiConnector.PostAsync(
                    url: fullUrl,
                    body: requestBody,
                    headers: headers,
                    timeout: platform.DefaultTimeoutSeconds
                );

                if (response.IsSuccess)
                {
                    var completionResponse = response.GetContent<ChatCompletionResponse>();
                    _logger.LogInformation("Successfully received response from {PlatformName}", platform.Name);
                    return completionResponse;
                }
                else
                {
                    _logger.LogError("{PlatformName} returned error: {StatusCode} - {Content}",
                        platform.Name, response.StatusCode, response.Content);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling {PlatformSlug} platform", platformSlug);
                return null;
            }
        }

        /// <summary>
        /// Gets all available active AI platforms
        /// </summary>
        public async Task<List<AIPlatform>> GetAvailablePlatformsAsync()
        {
            try
            {
                var response = await _aiPlatformService.GetAllActiveAsync();
                return response.Result ?? new List<AIPlatform>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available platforms");
                return new List<AIPlatform>();
            }
        }

        /// <summary>
        /// Gets a specific platform by slug
        /// </summary>
        public async Task<AIPlatform?> GetPlatformAsync(string slug)
        {
            try
            {
                var response = await _aiPlatformService.GetActiveBySlugAsync(slug);
                return response.Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting platform {Slug}", slug);
                return null;
            }
        }

        /// <summary>
        /// Tests connectivity to a platform
        /// </summary>
        public async Task<bool> TestPlatformConnectivityAsync(string platformSlug)
        {
            try
            {
                return await _aiPlatformService.TestPlatformConnectivityAsync(platformSlug);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connectivity to platform '{PlatformSlug}'", platformSlug);
                return false;
            }
        }

        /// <summary>
        /// Creates platform-specific chat completion request body
        /// </summary>
        private static object CreateChatCompletionRequest(string platformName, string model, object[] messages, int maxTokens, double temperature)
        {
            return platformName.ToLower() switch
            {
                "openai" or "openrouter" => new
                {
                    model = model,
                    messages = messages,
                    max_tokens = maxTokens,
                    temperature = temperature
                },
                "anthropic" => new
                {
                    model = model,
                    messages = messages,
                    max_tokens = maxTokens,
                    temperature = temperature
                },
                "google ai" => new
                {
                    model = model,
                    contents = messages,
                    generationConfig = new
                    {
                        maxOutputTokens = maxTokens,
                        temperature = temperature
                    }
                },
                _ => new
                {
                    model = model,
                    messages = messages,
                    max_tokens = maxTokens,
                    temperature = temperature
                }
            };
        }

        /// <summary>
        /// Gets the chat completion endpoint for each platform
        /// </summary>
        private static string GetChatCompletionEndpoint(string platformName)
        {
            return platformName.ToLower() switch
            {
                "openai" => "chat/completions",
                "openrouter" => "chat/completions",
                "anthropic" => "messages",
                "google ai" => "models/{model}:generateContent",
                "cohere" => "chat",
                _ => "chat/completions" // Default fallback
            };
        }
    }
}