namespace InsightSage.Shared.Models.Entities
{
    /// <summary>
    /// Represents an AI Platform provider (e.g., OpenAI, OpenRouter, Anthropic, Google, etc.)
    /// that provides AI API integrations for various AI models
    /// </summary>
    public class AIPlatform : BaseEntity
    {
        private string _name = string.Empty;
        
        /// <summary>
        /// Name of the AI platform (e.g., "OpenAI", "OpenRouter", "Anthropic")
        /// </summary>
        public required string Name 
        { 
            get => _name;
            set
            {
                _name = value;
                Slug = GenerateSlugFromName(value);
            }
        }
        
        /// <summary>
        /// Description of the AI platform and its capabilities
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// URL-friendly slug generated from the platform name
        /// </summary>
        public string Slug { get; set; } = string.Empty;
        
        /// <summary>
        /// Base API URL for the platform (e.g., "https://api.openai.com/v1", "https://openrouter.ai/api/v1")
        /// </summary>
        public required string ApiBaseUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// Official website URL of the platform
        /// </summary>
        public string? WebsiteUrl { get; set; }
        
        /// <summary>
        /// Documentation URL for the platform's API
        /// </summary>
        public string? DocumentationUrl { get; set; }
        
        /// <summary>
        /// Whether the platform is currently active and available for use
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Whether the platform requires an API key for authentication
        /// </summary>
        public bool RequiresApiKey { get; set; } = true;
        
        /// <summary>
        /// Authentication header name (e.g., "Authorization", "x-api-key")
        /// </summary>
        public string AuthHeaderName { get; set; } = "Authorization";
        
        /// <summary>
        /// Authentication header value format (e.g., "Bearer {token}", "Bearer {api_key}")
        /// Use {token} or {api_key} as placeholder for the actual key
        /// </summary>
        public string AuthHeaderFormat { get; set; } = "Bearer {api_key}";
        
        /// <summary>
        /// Maximum rate limit requests per minute (0 = no limit specified)
        /// </summary>
        public int RateLimitPerMinute { get; set; } = 0;
        
        /// <summary>
        /// Default request timeout in seconds
        /// </summary>
        public int DefaultTimeoutSeconds { get; set; } = 30;
        
        /// <summary>
        /// Display order for sorting platforms in UI
        /// </summary>
        public int DisplayOrder { get; set; } = 0;
        
        /// <summary>
        /// Additional configuration settings in JSON format
        /// Can store platform-specific settings like custom headers, special endpoints, etc.
        /// </summary>
        public string? ConfigurationJson { get; set; }

        /// <summary>
        /// Generates a slug from the name by converting to lowercase and replacing spaces with underscores
        /// </summary>
        /// <param name="name">The name to convert to a slug</param>
        /// <returns>The generated slug</returns>
        private static string GenerateSlugFromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            return name.Trim()
                      .ToLowerInvariant()
                      .Replace(" ", "_")
                      .Replace("\t", "_")
                      .Replace("\n", "_")
                      .Replace("\r", "_")
                      .Replace("-", "_")
                      .Replace(".", "_");
        }
    }
}