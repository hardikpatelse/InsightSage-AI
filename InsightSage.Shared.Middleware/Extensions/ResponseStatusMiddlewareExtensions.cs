using Microsoft.AspNetCore.Builder;

namespace InsightSage.Shared.Middleware.Extensions
{
    /// <summary>
    /// Extension methods for registering ResponseStatusMiddleware
    /// </summary>
    public static class ResponseStatusMiddlewareExtensions
    {
        /// <summary>
        /// Adds the ResponseStatusMiddleware to the application pipeline.
        /// This middleware automatically sets HttpStatusCode.OK in BaseResponse.Status
        /// when there are no errors and no exception details.
        /// </summary>
        /// <param name="builder">The application builder</param>
        /// <returns>The application builder for chaining</returns>
        public static IApplicationBuilder UseResponseStatusMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ResponseStatusMiddleware>();
        }
    }
}