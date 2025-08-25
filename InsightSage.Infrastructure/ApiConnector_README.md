# API Connector

A comprehensive HTTP client wrapper for making requests to 3rd party APIs across your InsightSage backend services.

## Overview

The `IApiConnector` provides a consistent, easy-to-use interface for making HTTP requests with features like:
- Support for all HTTP methods (GET, POST, PUT, PATCH, DELETE, HEAD, OPTIONS)
- Dynamic headers and request body
- Automatic JSON serialization/deserialization
- Comprehensive error handling and logging
- Timeout configuration
- Response metadata (status, headers, duration, etc.)

## Quick Start

### 1. Register the Service

In your `Program.cs`, add the API connector to your DI container:

```csharp
using InsightSage.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Basic registration
builder.Services.AddApiConnector();

// Or with custom HttpClient configuration
builder.Services.AddApiConnector(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "InsightSage/1.0");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();
```

### 2. Inject and Use in Services

```csharp
public class ExternalApiService
{
    private readonly IApiConnector _apiConnector;

    public ExternalApiService(IApiConnector apiConnector)
    {
        _apiConnector = apiConnector;
    }

    public async Task<WeatherData?> GetWeatherAsync(string city)
    {
        var headers = new Dictionary<string, string>
        {
            ["Authorization"] = "Bearer YOUR_API_KEY",
            ["X-Custom-Header"] = "CustomValue"
        };

        var response = await _apiConnector.GetAsync(
            $"https://api.weather.com/v1/weather?city={city}",
            headers: headers,
            timeout: 10
        );

        if (response.IsSuccess)
        {
            return response.GetContent<WeatherData>();
        }

        // Handle error
        Console.WriteLine($"API call failed: {response.StatusCode}");
        return null;
    }
}
```

## API Methods

### GET Request
```csharp
var response = await _apiConnector.GetAsync(
    url: "https://api.example.com/data",
    headers: new Dictionary<string, string> { ["Authorization"] = "Bearer token" },
    timeout: 30
);
```

### POST Request
```csharp
var requestBody = new { Name = "John", Email = "john@example.com" };

var response = await _apiConnector.PostAsync(
    url: "https://api.example.com/users",
    body: requestBody,
    contentType: "application/json",
    headers: new Dictionary<string, string> { ["Authorization"] = "Bearer token" }
);
```

### PUT Request
```csharp
var updateData = new { Id = 123, Name = "Updated Name" };

var response = await _apiConnector.PutAsync(
    url: "https://api.example.com/users/123",
    body: updateData,
    headers: authHeaders
);
```

### DELETE Request
```csharp
var response = await _apiConnector.DeleteAsync(
    url: "https://api.example.com/users/123",
    headers: authHeaders
);
```

### Custom Request
```csharp
var response = await _apiConnector.SendAsync(
    method: HttpMethod.Patch,
    url: "https://api.example.com/users/123",
    body: patchData,
    contentType: "application/json-patch+json",
    headers: authHeaders
);
```

## Request Body Types

The API connector supports various body types:

```csharp
// JSON object (automatic serialization)
var jsonBody = new { Name = "John", Age = 30 };
await _apiConnector.PostAsync(url, jsonBody);

// String content
var stringBody = "{\"name\":\"John\",\"age\":30}";
await _apiConnector.PostAsync(url, stringBody, "application/json");

// Byte array
var byteBody = Encoding.UTF8.GetBytes("raw data");
await _apiConnector.PostAsync(url, byteBody, "application/octet-stream");

// Stream content
using var fileStream = File.OpenRead("file.json");
await _apiConnector.PostAsync(url, fileStream, "application/json");
```

## Response Handling

```csharp
var response = await _apiConnector.GetAsync("https://api.example.com/data");

// Check if successful
if (response.IsSuccess)
{
    // Get raw content
    string content = response.Content;
    
    // Or deserialize to typed object
    var data = response.GetContent<MyDataType>();
    
    // Access metadata
    Console.WriteLine($"Status: {response.StatusCode}");
    Console.WriteLine($"Content-Type: {response.ContentType}");
    Console.WriteLine($"Duration: {response.Duration.TotalMilliseconds}ms");
    
    // Access headers
    foreach (var header in response.Headers)
    {
        Console.WriteLine($"{header.Key}: {header.Value}");
    }
}
else
{
    // Handle error
    Console.WriteLine($"Request failed: {response.StatusCode}");
    
    if (response.Exception != null)
    {
        Console.WriteLine($"Exception: {response.Exception.Message}");
    }
}
```

## Error Handling

The API connector handles various error scenarios:

- **Network timeouts**: Returns `RequestTimeout` status
- **Connection errors**: Returns `BadGateway` status  
- **HTTP errors**: Returns the actual HTTP status code from the server
- **Serialization errors**: Caught and logged, returns appropriate error status

```csharp
var response = await _apiConnector.GetAsync("https://unreachable-api.com/data", timeout: 5);

switch (response.StatusCode)
{
    case HttpStatusCode.OK:
        // Success
        break;
    case HttpStatusCode.RequestTimeout:
        // Handle timeout
        break;
    case HttpStatusCode.BadGateway:
        // Handle connection error
        break;
    case HttpStatusCode.Unauthorized:
        // Handle auth error
        break;
    default:
        // Handle other errors
        break;
}
```

## Configuration Examples

### Basic Setup
```csharp
// In Program.cs
builder.Services.AddApiConnector();
```

### With Default Headers
```csharp
builder.Services.AddApiConnector(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "InsightSage API Client/1.0");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromMinutes(2);
});
```

### With Base Address
```csharp
builder.Services.AddApiConnector(client =>
{
    client.BaseAddress = new Uri("https://api.external-service.com/");
    client.DefaultRequestHeaders.Add("Authorization", "Bearer your-token-here");
});
```

## Advanced Usage

### Retry Logic
```csharp
public async Task<ApiResponse> GetWithRetryAsync(string url, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        var response = await _apiConnector.GetAsync(url);
        
        if (response.IsSuccess || attempt == maxRetries)
        {
            return response;
        }
        
        // Wait before retry (exponential backoff)
        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
    }
    
    return new ApiResponse { StatusCode = HttpStatusCode.RequestTimeout };
}
```

### Authentication Headers
```csharp
public class ApiService
{
    private readonly IApiConnector _apiConnector;
    private readonly IConfiguration _config;

    public ApiService(IApiConnector apiConnector, IConfiguration config)
    {
        _apiConnector = apiConnector;
        _config = config;
    }

    private Dictionary<string, string> GetAuthHeaders()
    {
        return new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {_config["ExternalApi:Token"]}",
            ["X-API-Key"] = _config["ExternalApi:ApiKey"]
        };
    }

    public async Task<T?> GetDataAsync<T>(string endpoint) where T : class
    {
        var response = await _apiConnector.GetAsync(
            $"{_config["ExternalApi:BaseUrl"]}/{endpoint}",
            headers: GetAuthHeaders()
        );

        return response.IsSuccess ? response.GetContent<T>() : null;
    }
}
```

## Logging

The API connector includes comprehensive logging at the `Debug` and `Error` levels:

- Request initiation (Debug)
- Response received with timing (Debug)
- Errors and exceptions (Error/Warning)
- Header addition failures (Warning)

## Thread Safety

The `IApiConnector` implementation is thread-safe and can be safely used in concurrent scenarios. It's registered as a scoped service by default.

## Dependencies

- `Microsoft.Extensions.Http` - For IHttpClientFactory
- `Microsoft.Extensions.Logging.Abstractions` - For logging
- `System.Text.Json` - For JSON serialization

## Files

- **Interface**: `InsightSage.Shared/Interfaces/Others/IApiConnector.cs`
- **Implementation**: `InsightSage.Infrastructure/ApiConnector.cs`
- **Extensions**: `InsightSage.Infrastructure/Extensions/ApiConnectorServiceExtensions.cs`