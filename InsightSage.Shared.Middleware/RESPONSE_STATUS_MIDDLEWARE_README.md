# ResponseStatusMiddleware

## Overview
The `ResponseStatusMiddleware` automatically sets the `HttpStatusCode.OK` (200) in the `BaseResponse.Status` property for successful API responses that don't have errors or exception details.

## Purpose
This middleware eliminates the need to manually set `Status = HttpStatusCode.OK` in every controller action by automatically handling this for responses that:
- Have no errors (`Errors` list is null or empty)
- Have no exception details (`ExceptionDetails` is null or empty)
- Have a default (unset) status code
- Return a successful HTTP status code (2xx)

## How It Works
1. **Intercepts HTTP responses** before they're sent to the client
2. **Checks if the response is JSON** and contains BaseResponse properties
3. **Analyzes the response** to determine if it should set Status to OK
4. **Modifies the JSON** to include `"status": 200` if conditions are met
5. **Preserves original responses** that already have status codes or errors

## Usage

### 1. Register the Middleware
In your `Program.cs`, add the middleware after authentication and authorization:

```csharp
using InsightSage.Infrastructure.Extensions;

var app = builder.Build();

// ... other middleware ...

app.UseAuthentication();
app.UseAuthorization();

// Add the ResponseStatusMiddleware
app.UseResponseStatusMiddleware();

app.MapControllers();
```

### 2. Controller Actions - Before
Without the middleware, you need to manually set the status:

```csharp
[HttpGet]
public async Task<ChatMessageResponse<List<ChatMessage>>> GetAllMessages()
{
    try
    {
        var result = await _chatMessageService.GetAllAsync();
        result.Status = HttpStatusCode.OK; // Manual status setting
        return result;
    }
    catch (Exception ex)
    {
        return new ChatMessageResponse<List<ChatMessage>>
        {
            Status = HttpStatusCode.InternalServerError,
            Errors = new List<string> { ex.Message }
        };
    }
}
```

### 3. Controller Actions - After
With the middleware, you can omit manual status setting for successful responses:

```csharp
[HttpGet]
public async Task<ChatMessageResponse<List<ChatMessage>>> GetAllMessages()
{
    try
    {
        var result = await _chatMessageService.GetAllAsync();
        // No need to set Status = HttpStatusCode.OK - middleware handles it
        return result;
    }
    catch (Exception ex)
    {
        return new ChatMessageResponse<List<ChatMessage>>
        {
            Status = HttpStatusCode.InternalServerError, // Still need to set error statuses
            Errors = new List<string> { ex.Message }
        };
    }
}
```

## When Status is Set to OK
The middleware sets `Status = HttpStatusCode.OK` when **ALL** of the following conditions are met:

? Response is JSON content type  
? HTTP response status code is 2xx (successful)  
? Response contains BaseResponse properties (`status`, `errors`, etc.)  
? `Status` property is default/unset (`0`)  
? `Errors` list is null or empty  
? `ExceptionDetails` is null or empty  

## When Status is NOT Modified
The middleware preserves the original response when:

? Response is not JSON  
? HTTP status code is not 2xx  
? Response doesn't match BaseResponse structure  
? `Status` is already set to a non-default value  
? `Errors` list contains items  
? `ExceptionDetails` has content  
? JSON parsing fails  

## Example Transformations

### Successful Response
**Input:**
```json
{
  "result": [{"id": 1, "content": "Hello"}],
  "status": 0,
  "errors": [],
  "exceptionDetails": null
}
```

**Output:**
```json
{
  "result": [{"id": 1, "content": "Hello"}],
  "status": 200,
  "errors": [],
  "exceptionDetails": null
}
```

### Error Response (No Change)
**Input:**
```json
{
  "result": null,
  "status": 500,
  "errors": ["Database error"],
  "exceptionDetails": "System.Exception: Connection failed"
}
```

**Output:** (No change - errors present)
```json
{
  "result": null,
  "status": 500,
  "errors": ["Database error"],
  "exceptionDetails": "System.Exception: Connection failed"
}
```

## Benefits
- **Reduces boilerplate code** in controllers
- **Consistent status setting** across all APIs
- **Automatic handling** without changing existing logic
- **Preserves error responses** as-is
- **Type-safe** - only affects BaseResponse-based responses

## Implementation Files
- `InsightSage.Infrastructure/Middleware/ResponseStatusMiddleware.cs`
- `InsightSage.Infrastructure/Extensions/ResponseStatusMiddlewareExtensions.cs`

## Dependencies
- `Microsoft.AspNetCore.Http`
- `System.Text.Json`
- `InsightSage.Shared.Models.Responses.Common.BaseResponse`