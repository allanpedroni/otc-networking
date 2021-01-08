using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Otc.Networking.Extensions.Http.Refit;
using Refit;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HttpRefitClientFactoryExtensions
    {
        /// <summary>
        /// Adds a Refit client to the DI container
        /// </summary>
        /// <typeparam name="T">Type of the Refit interface</typeparam>
        /// <param name="services">container</param>
        /// <param name="settings">Optional. Settings to configure the instance with</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddRefitClientWithCorrelation<T>(this IServiceCollection services,
            RefitSettings settings = null) where T : class
        {
            services.TryAddSingleton(provider => RequestBuilder.ForType<T>(settings));


            return services.AddHttpClientWithCorrelation(UniqueName.ForType<T>())
                           .ConfigureHttpMessageHandlerBuilder(ConfigureHttpMessageHandler(settings))
                           .AddTypedClient((client, serviceProvider) =>
                                RestService.For<T>(client, serviceProvider.GetService<IRequestBuilder<T>>()));
        }

        /// <summary>
        /// Adds a Refit client to the DI container
        /// </summary>
        /// <param name="services">container</param>
        /// <param name="refitInterfaceType">Type of the Refit interface</param>
        /// <param name="settings">Optional. Settings to configure the instance with</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddRefitClientWithCorrelation(this IServiceCollection services,
            Type refitInterfaceType, RefitSettings settings = null)
        {

            return services.AddHttpClientWithCorrelation(UniqueName.ForType(refitInterfaceType))
                            .ConfigureHttpMessageHandlerBuilder(ConfigureHttpMessageHandler(settings))
                           .AddTypedClient(refitInterfaceType, (client, serviceProvider) =>
                                RestService.For(refitInterfaceType, client, settings));
        }

        private static Action<HttpMessageHandlerBuilder> ConfigureHttpMessageHandler(
            RefitSettings settings = null) => (builder) =>
            {
                var innerHandler = GetHttpMessageHandler(settings);

                if (innerHandler != null)
                {
                    builder.PrimaryHandler = innerHandler;
                }
            };

        // check to see if user provided custom auth token
        private static HttpMessageHandler GetHttpMessageHandler(RefitSettings settings)
        {
            HttpMessageHandler innerHandler = null;
            if (settings != null)
            {
                if (settings.HttpMessageHandlerFactory != null)
                {
                    innerHandler = settings.HttpMessageHandlerFactory();
                }

                if (settings.AuthorizationHeaderValueGetter != null)
                {
                    innerHandler = new AuthenticatedHttpClientHandler(
                        settings.AuthorizationHeaderValueGetter, innerHandler);
                }
                else if (settings.AuthorizationHeaderValueWithParamGetter != null)
                {
                    innerHandler = new AuthenticatedParameterizedHttpClientHandler(
                        settings.AuthorizationHeaderValueWithParamGetter, innerHandler);
                }
            }

            return innerHandler;
        }

        private static IHttpClientBuilder AddTypedClient(this IHttpClientBuilder builder,
            Type type, Func<HttpClient, IServiceProvider, object> factory)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            builder.Services.AddTransient(type, s =>
            {
                var httpClientFactory = s.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient(builder.Name);

                return factory(httpClient, s);
            });

            return builder;
        }
    }
}