using Microsoft.AspNetCore.Http;
using Otc.Networking.Http.Client.Abstractions;
using System;
using System.Net.Http;
using System.Reflection;

namespace Otc.Networking.Http.Client.AspNetCore
{
    internal class HttpClientFactory : IHttpClientFactory
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        private const string XRootCorrelationIdHeaderKey = "X-Root-Correlation-Id";
        private const string XCorrelationIdHeaderKey = "X-Correlation-Id";
        private const string XRootConsumerNameHeaderKey = "X-Root-Consumer-Name";
        private const string XConsumerNameHeaderKey = "X-Consumer-Name";
        private const string XFullTraceHeaderKey = "X-Full-Trace";
        private static readonly string applicationName =
            $"{Assembly.GetEntryAssembly().GetName().Name}-{Environment.MachineName}";

        public HttpClientFactory(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor ??
                throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private void AddCorrelationHeaders(HttpClient httpClient)
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                throw new InvalidOperationException("Could not get a valid HttpContext from " +
                    "HttpContextAccessor, make sure to create HttpClient in code executing under a " +
                    "valid http request scope. Creating HttpClient in code like STARTUP WILL NOT work " +
                    "because it could not retrive TraceIdentifier and other relevant http request " +
                    "headers.");
            }

            var requestHeaders = httpContext.Request?.Headers;

            if (requestHeaders == null)
            {
                throw new InvalidOperationException("Could not read request headers.");
            }

            var traceIdentifier = httpContext.TraceIdentifier;

            // Check if XFullTrace was provided by who is requesting this; then include provided 
            // value in order to be forward to the request initiating here.
            var fullTraceContent = requestHeaders.ContainsKey(XFullTraceHeaderKey) ?
                $"{requestHeaders[XFullTraceHeaderKey]}; " : string.Empty;

            // also append traceIdentifier and ApplicationName to XFullTrace
            fullTraceContent += $"{traceIdentifier} ({applicationName})";

            httpClient.DefaultRequestHeaders.Add(XConsumerNameHeaderKey, applicationName);
            httpClient.DefaultRequestHeaders.Add(XCorrelationIdHeaderKey, traceIdentifier);
            httpClient.DefaultRequestHeaders.Add(XFullTraceHeaderKey, fullTraceContent);

            var rootCorrelationId = requestHeaders.ContainsKey(XRootCorrelationIdHeaderKey) ?
                (string)requestHeaders[XRootCorrelationIdHeaderKey] : traceIdentifier;
            var rootCallerId = requestHeaders.ContainsKey(XRootConsumerNameHeaderKey) ?
                (string)requestHeaders[XRootConsumerNameHeaderKey] : applicationName;

            httpClient.DefaultRequestHeaders.Add(XRootCorrelationIdHeaderKey, rootCorrelationId);
            httpClient.DefaultRequestHeaders.Add(XRootConsumerNameHeaderKey, rootCallerId);
        }

        public HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            AddCorrelationHeaders(httpClient);

            return httpClient;
        }

        public HttpClient CreateHttpClient(HttpClientHandler handler)
        {
            var httpClient = new HttpClient(handler);
            AddCorrelationHeaders(httpClient);

            return httpClient;
        }

        public HttpClient CreateHttpClient(HttpClientHandler handler, bool disposeHandler)
        {
            var httpClient = new HttpClient(handler, disposeHandler);
            AddCorrelationHeaders(httpClient);

            return httpClient;
        }
    }
}
