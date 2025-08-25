using InsightSage.Shared.Interfaces.Others;
using Microsoft.Extensions.DependencyInjection;

namespace InsightSage.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for registering API connector services
    /// </summary>
    public static class ApiConnectorServiceExtensions
    {
        /// <summary>
        /// Adds the API connector service to the DI container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddApiConnector(this IServiceCollection services)
        {
            // Register HttpClient factory if not already registered
            services.AddHttpClient();

            // Register the API connector as scoped
            services.AddScoped<IApiConnector, ApiConnector>();

            return services;
        }

        /// <summary>
        /// Adds the API connector service with custom HttpClient configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureClient">Action to configure the default HttpClient</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddApiConnector(this IServiceCollection services, Action<HttpClient> configureClient)
        {
            // Register HttpClient with configuration
            services.AddHttpClient();
            services.ConfigureHttpClientDefaults(builder => builder.ConfigureHttpClient(configureClient));

            // Register the API connector as scoped
            services.AddScoped<IApiConnector, ApiConnector>();

            return services;
        }

        /// <summary>
        /// Adds the API connector service with named HttpClient configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="httpClientName">Name for the HttpClient</param>
        /// <param name="configureClient">Optional action to configure the named HttpClient</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddApiConnector(this IServiceCollection services, string httpClientName, Action<HttpClient>? configureClient = null)
        {
            // Register named HttpClient
            var clientBuilder = services.AddHttpClient(httpClientName);

            if (configureClient != null)
            {
                clientBuilder.ConfigureHttpClient(configureClient);
            }

            // Register the API connector as scoped
            services.AddScoped<IApiConnector, ApiConnector>();

            return services;
        }
    }
}