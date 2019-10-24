using Otc.Networking.Http.Client.Abstractions;
using Otc.Networking.Http.Client.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OtcNetClientAspNetCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddAspNetCoreHttpClientFactoryWithCorrelation(
            this IServiceCollection services)
        {
            if (services == null)
            {
                throw new System.ArgumentNullException(nameof(services));
            }

            services.AddScoped<IHttpClientFactory, HttpClientFactory>();

            return services;
        }
    }
}
