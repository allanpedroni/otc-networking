using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Otc.Networking.Extensions.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HttpClientAspNetCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddHttpClientWithCorrelation(
            this IServiceCollection services)
        {
            if (services == null)
            {
                throw new System.ArgumentNullException(nameof(services));
            }

            services.TryAddTransient<CorrelationHeaderHandler>();

            services
                .AddHttpClient(Options.Options.DefaultName)
                .AddHttpMessageHandler<CorrelationHeaderHandler>();

            return services;
        }

        public static IHttpClientBuilder AddHttpClientWithCorrelation(this IServiceCollection services,
            string name)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (name is null || name.Equals(string.Empty))
            {
                throw new ArgumentNullException(nameof(name));
            }

            services.TryAddTransient<CorrelationHeaderHandler>();

            var builder = services
                .AddHttpClient(name)
                .AddHttpMessageHandler<CorrelationHeaderHandler>();
            
            return builder;
        }

        public static IHttpClientBuilder AddHttpClientWithCorrelation(this IServiceCollection services,
            string name, Action<HttpClient> configureClient)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (configureClient is null)
            {
                throw new ArgumentNullException(nameof(configureClient));
            }

            services.TryAddTransient<CorrelationHeaderHandler>();

            var builder = services
                .AddHttpClient(name)
                .AddHttpMessageHandler<CorrelationHeaderHandler>()
                .ConfigureHttpClient(configureClient);

            return builder;
        }
    }
}
