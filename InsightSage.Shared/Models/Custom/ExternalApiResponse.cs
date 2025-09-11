using System.Net;

namespace InsightSage.Shared.Models.Custom
{
    /// <summary>
    /// Represents the response from an API call
    /// </summary>
    public class ExternalApiResponse
    {
        /// <summary>
        /// HTTP status code of the response
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Indicates whether the request was successful (2xx status codes)
        /// </summary>
        public bool IsSuccess => (int)StatusCode >= 200 && (int)StatusCode < 300;

        /// <summary>
        /// The response content as a string
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Response headers
        /// </summary>
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Content type of the response
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// Content length of the response
        /// </summary>
        public long? ContentLength { get; set; }

        /// <summary>
        /// Exception that occurred during the request (if any)
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// Time taken for the request to complete
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Deserializes the response content to the specified type
        /// </summary>
        /// <typeparam name="T">Type to deserialize to</typeparam>
        /// <returns>Deserialized object or default if deserialization fails</returns>
        public T? GetContent<T>() where T : class
        {
            if (string.IsNullOrWhiteSpace(Content))
                return default;

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<T>(Content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });
            }
            catch
            {
                return default;
            }
        }
    }
}
