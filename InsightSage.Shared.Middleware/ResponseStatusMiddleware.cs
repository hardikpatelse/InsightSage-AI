using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using InsightSage.Shared.Models.Responses.Common;

namespace InsightSage.Shared.Middleware
{
    /// <summary>
    /// Middleware that automatically sets HttpStatusCode.OK in BaseResponse.Status 
    /// when there are no errors and no exception details
    /// </summary>
    public class ResponseStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public ResponseStatusMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Capture the original response body stream
            var originalBodyStream = context.Response.Body;

            try
            {
                // Create a new memory stream to capture the response
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                // Continue to the next middleware
                await _next(context);

                // Only process if response hasn't started and is successful
                if (!context.Response.HasStarted && 
                    context.Response.StatusCode >= 200 && 
                    context.Response.StatusCode < 300)
                {
                    // Read the response body
                    responseBody.Seek(0, SeekOrigin.Begin);
                    var responseContent = await new StreamReader(responseBody).ReadToEndAsync();

                    // Process the response if it's a BaseResponse
                    var processedContent = ProcessResponse(responseContent, context);

                    // Write the processed response back
                    if (processedContent != responseContent)
                    {
                        // Reset the response body stream to original
                        context.Response.Body = originalBodyStream;
                        
                        // Write the modified content
                        var bytes = System.Text.Encoding.UTF8.GetBytes(processedContent);
                        context.Response.ContentLength = bytes.Length;
                        await context.Response.Body.WriteAsync(bytes);
                    }
                    else
                    {
                        // Copy original response to the original stream
                        context.Response.Body = originalBodyStream;
                        responseBody.Seek(0, SeekOrigin.Begin);
                        await responseBody.CopyToAsync(originalBodyStream);
                    }
                }
                else
                {
                    // Copy original response to the original stream if response has already started
                    context.Response.Body = originalBodyStream;
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
            catch
            {
                // Restore the original body stream in case of error
                context.Response.Body = originalBodyStream;
                throw;
            }
        }

        private static string ProcessResponse(string responseContent, HttpContext context)
        {
            // Only process JSON responses
            if (!IsJsonResponse(context))
            {
                return responseContent;
            }

            // Skip if response content is empty
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                return responseContent;
            }

            try
            {
                // Try to deserialize as BaseResponse
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var jsonDocument = JsonDocument.Parse(responseContent);
                
                // Check if the response has the properties of BaseResponse
                if (!HasBaseResponseProperties(jsonDocument.RootElement))
                {
                    return responseContent;
                }

                // Deserialize to check the current state
                var baseResponse = JsonSerializer.Deserialize<BaseResponse>(responseContent, options);
                
                if (baseResponse == null)
                {
                    return responseContent;
                }

                // Check if we should set the status to OK
                if (ShouldSetStatusToOK(baseResponse))
                {
                    // Create a new JSON object with Status set to OK
                    var modifiedJson = ModifyJsonStatus(jsonDocument, HttpStatusCode.OK);
                    return modifiedJson;
                }

                return responseContent;
            }
            catch (JsonException)
            {
                // Response is not valid JSON, return as-is
                return responseContent;
            }
            catch
            {
                // Any other error, return original content
                return responseContent;
            }
        }

        private static bool IsJsonResponse(HttpContext context)
        {
            return context.Response.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true;
        }

        private static bool HasBaseResponseProperties(JsonElement jsonElement)
        {
            // Check if the JSON has the key properties of BaseResponse
            return jsonElement.TryGetProperty("status", out _) ||
                   jsonElement.TryGetProperty("Status", out _) ||
                   jsonElement.TryGetProperty("errors", out _) ||
                   jsonElement.TryGetProperty("Errors", out _);
        }

        private static bool ShouldSetStatusToOK(BaseResponse response)
        {
            // Set status to OK if:
            // 1. Status is not already set (is default value)
            // 2. No errors exist
            // 3. No exception details exist
            return response.Status == default(HttpStatusCode) &&
                   (response.Errors == null || response.Errors.Count == 0) &&
                   string.IsNullOrWhiteSpace(response.ExceptionDetails);
        }

        private static string ModifyJsonStatus(JsonDocument originalDocument, HttpStatusCode newStatus)
        {
            var options = new JsonWriterOptions
            {
                Indented = false
            };

            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, options);

            // Start copying the original JSON
            writer.WriteStartObject();

            var hasStatusProperty = false;

            foreach (var property in originalDocument.RootElement.EnumerateObject())
            {
                if (property.Name.Equals("status", StringComparison.OrdinalIgnoreCase))
                {
                    // Replace the status value
                    writer.WriteNumber(property.Name, (int)newStatus);
                    hasStatusProperty = true;
                }
                else
                {
                    // Copy the property as-is
                    property.WriteTo(writer);
                }
            }

            // If status property didn't exist, add it
            if (!hasStatusProperty)
            {
                writer.WriteNumber("status", (int)newStatus);
            }

            writer.WriteEndObject();
            writer.Flush();

            return System.Text.Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}