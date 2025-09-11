# AIPlatform Entity Implementation

## Overview
The `AIPlatform` entity is designed to manage multiple AI service providers (OpenAI, OpenRouter, Anthropic, Google, etc.) that offer AI API integrations. This entity provides a centralized way to configure and manage different AI platforms within the InsightSage application.

## ? **Entity Properties**

### **Core Properties**
- **`Id`** - Primary key (inherited from BaseEntity)
- **`Name`** - Platform name (e.g., "OpenAI", "OpenRouter", "Anthropic")
- **`Description`** - Detailed description of the platform's capabilities
- **`Slug`** - Auto-generated URL-friendly identifier
- **`CreatedAt/UpdatedAt`** - Timestamps (inherited from BaseEntity)

### **API Configuration**
- **`ApiBaseUrl`** - Base API endpoint URL (e.g., "https://api.openai.com/v1")
- **`RequiresApiKey`** - Boolean indicating if API key authentication is required
- **`AuthHeaderName`** - Authentication header name (default: "Authorization")
- **`AuthHeaderFormat`** - Header value format (default: "Bearer {api_key}")

### **Platform Management**
- **`IsActive`** - Enable/disable platform availability
- **`RateLimitPerMinute`** - Rate limiting configuration (0 = no limit)
- **`DefaultTimeoutSeconds`** - Default request timeout (default: 30 seconds)
- **`DisplayOrder`** - Sorting order for UI display

### **Documentation & UI**
- **`WebsiteUrl`** - Official platform website
- **`DocumentationUrl`** - API documentation link
- **`IconName`** - Icon identifier for UI representation

### **Advanced Configuration**
- **`ConfigurationJson`** - JSON string for platform-specific settings

---

## ?? **Database Configuration**

### **Table Structure**
The entity is configured with the following database specifications:

```sql
CREATE TABLE [AIPlatforms] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [Slug] nvarchar(250) NOT NULL,
    [ApiBaseUrl] nvarchar(500) NOT NULL,
    [WebsiteUrl] nvarchar(500) NULL,
    [DocumentationUrl] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [RequiresApiKey] bit NOT NULL DEFAULT 1,
    [AuthHeaderName] nvarchar(100) NOT NULL DEFAULT 'Authorization',
    [AuthHeaderFormat] nvarchar(200) NOT NULL DEFAULT 'Bearer {api_key}',
    [RateLimitPerMinute] int NOT NULL DEFAULT 0,
    [DefaultTimeoutSeconds] int NOT NULL DEFAULT 30,
    [IconName] nvarchar(100) NULL,
    [DisplayOrder] int NOT NULL DEFAULT 0,
    [ConfigurationJson] ntext NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_AIPlatforms] PRIMARY KEY ([Id])
);
```

### **Indexes**
- **`IX_AIPlatforms_Name_Unique`** - Unique constraint on platform name
- **`IX_AIPlatforms_Slug_Unique`** - Unique constraint on slug for URL routing
- **`IX_AIPlatforms_IsActive`** - Index for filtering active platforms
- **`IX_AIPlatforms_DisplayOrder`** - Index for ordered queries

---

## ?? **Usage Examples**

### **Creating Platform Configurations**

```csharp
// OpenAI Platform
var openAI = new AIPlatform
{
    Name = "OpenAI",
    Description = "Leading AI research company providing GPT models and other AI services",
    ApiBaseUrl = "https://api.openai.com/v1",
    WebsiteUrl = "https://openai.com",
    DocumentationUrl = "https://platform.openai.com/docs",
    IsActive = true,
    RequiresApiKey = true,
    AuthHeaderName = "Authorization",
    AuthHeaderFormat = "Bearer {api_key}",
    RateLimitPerMinute = 60,
    DefaultTimeoutSeconds = 30,
    IconName = "openai-icon",
    DisplayOrder = 1
};

// OpenRouter Platform
var openRouter = new AIPlatform
{
    Name = "OpenRouter",
    Description = "AI model router providing access to multiple AI models through a single API",
    ApiBaseUrl = "https://openrouter.ai/api/v1",
    WebsiteUrl = "https://openrouter.ai",
    DocumentationUrl = "https://openrouter.ai/docs",
    IsActive = true,
    RequiresApiKey = true,
    AuthHeaderName = "Authorization",
    AuthHeaderFormat = "Bearer {api_key}",
    RateLimitPerMinute = 100,
    DefaultTimeoutSeconds = 45,
    IconName = "openrouter-icon",
    DisplayOrder = 2
};

// Anthropic Platform
var anthropic = new AIPlatform
{
    Name = "Anthropic",
    Description = "AI safety company developing Claude AI models for safe and beneficial AI",
    ApiBaseUrl = "https://api.anthropic.com/v1",
    WebsiteUrl = "https://www.anthropic.com",
    DocumentationUrl = "https://docs.anthropic.com",
    IsActive = true,
    RequiresApiKey = true,
    AuthHeaderName = "x-api-key",
    AuthHeaderFormat = "{api_key}",
    RateLimitPerMinute = 50,
    DefaultTimeoutSeconds = 30,
    IconName = "anthropic-icon",
    DisplayOrder = 3
};
```

### **Entity Framework Queries**

```csharp
// Get all active platforms ordered by display order
var activePlatforms = await context.AIPlatforms
    .Where(p => p.IsActive)
    .OrderBy(p => p.DisplayOrder)
    .ToListAsync();

// Find platform by slug
var platform = await context.AIPlatforms
    .FirstOrDefaultAsync(p => p.Slug == "openai");

// Get platforms that require API keys
var apiKeyPlatforms = await context.AIPlatforms
    .Where(p => p.RequiresApiKey && p.IsActive)
    .ToListAsync();

// Search platforms by name
var searchResults = await context.AIPlatforms
    .Where(p => p.Name.Contains("Open"))
    .ToListAsync();
```

---

## ?? **Integration with IApiConnector**

The AIPlatform entity is designed to work seamlessly with the existing `IApiConnector` service:

```csharp
public class PlatformApiService
{
    private readonly IApiConnector _apiConnector;
    private readonly InsightSageDbContext _context;
    
    public PlatformApiService(IApiConnector apiConnector, InsightSageDbContext context)
    {
        _apiConnector = apiConnector;
        _context = context;
    }
    
    public async Task<ApiResponse> CallPlatformApi(string platformSlug, string endpoint, object? body = null, string? apiKey = null)
    {
        // Get platform configuration
        var platform = await _context.AIPlatforms
            .FirstOrDefaultAsync(p => p.Slug == platformSlug && p.IsActive);
            
        if (platform == null)
            throw new InvalidOperationException($"Platform '{platformSlug}' not found or inactive");
        
        // Build full URL
        var fullUrl = $"{platform.ApiBaseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
        
        // Prepare headers
        var headers = new Dictionary<string, string>();
        
        if (platform.RequiresApiKey && !string.IsNullOrEmpty(apiKey))
        {
            var authValue = platform.AuthHeaderFormat.Replace("{api_key}", apiKey).Replace("{token}", apiKey);
            headers[platform.AuthHeaderName] = authValue;
        }
        
        // Make API call with platform-specific timeout
        return await _apiConnector.PostAsync(
            url: fullUrl,
            body: body,
            headers: headers,
            timeout: platform.DefaultTimeoutSeconds
        );
    }
}
```

---

## ?? **Key Benefits**

### ? **Centralized Configuration**
- Single source of truth for all AI platform settings
- Easy to add new platforms without code changes
- Configuration changes through database updates

### ? **Flexible Authentication**
- Support for different authentication methods (Bearer tokens, API keys, custom headers)
- Configurable header names and formats per platform

### ? **Platform Management**
- Enable/disable platforms dynamically
- Rate limiting and timeout configuration
- Display ordering for UI consistency

### ? **Extensibility**
- JSON configuration field for platform-specific settings
- Custom authentication patterns
- Additional metadata storage

### ? **URL-Friendly**
- Auto-generated slugs for API routing
- Unique constraints prevent conflicts
- SEO-friendly platform identification

---

## ?? **Migration Instructions**

After the entity is added to the DbContext, create and apply the migration:

```bash
# Generate the migration
dotnet ef migrations add AddAIPlatformTable

# Apply the migration
dotnet ef database update

# Optional: Generate SQL script
dotnet ef migrations script --output AIPlatform.sql
```

---

## ?? **Example Platforms to Seed**

```csharp
// Common AI platforms that could be seeded
var platforms = new[]
{
    new AIPlatform { Name = "OpenAI", ApiBaseUrl = "https://api.openai.com/v1", DisplayOrder = 1 },
    new AIPlatform { Name = "OpenRouter", ApiBaseUrl = "https://openrouter.ai/api/v1", DisplayOrder = 2 },
    new AIPlatform { Name = "Anthropic", ApiBaseUrl = "https://api.anthropic.com/v1", AuthHeaderName = "x-api-key", AuthHeaderFormat = "{api_key}", DisplayOrder = 3 },
    new AIPlatform { Name = "Google AI", ApiBaseUrl = "https://generativelanguage.googleapis.com/v1", DisplayOrder = 4 },
    new AIPlatform { Name = "Cohere", ApiBaseUrl = "https://api.cohere.ai/v1", DisplayOrder = 5 },
    new AIPlatform { Name = "Hugging Face", ApiBaseUrl = "https://api-inference.huggingface.co", DisplayOrder = 6 }
};
```

This entity provides a robust foundation for managing multiple AI platforms in your InsightSage application! ??