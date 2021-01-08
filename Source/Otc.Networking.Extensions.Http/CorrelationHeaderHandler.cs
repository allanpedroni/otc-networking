using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Otc.Networking.Extensions.Http
{
    internal class CorrelationHeaderHandler : DelegatingHandler
    {
        private const string XRootCorrelationIdHeaderKey = "X-Root-Correlation-Id";
        private const string XCorrelationIdHeaderKey = "X-Correlation-Id";
        private const string XRootConsumerNameHeaderKey = "X-Root-Consumer-Name";
        private const string XConsumerNameHeaderKey = "X-Consumer-Name";
        private const string XFullTraceHeaderKey = "X-Full-Trace";
        private static readonly string ApplicationName =
            $"{Assembly.GetEntryAssembly().GetName().Name}-{Environment.MachineName}";

        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger logger;

        public CorrelationHeaderHandler(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory?.CreateLogger<CorrelationHeaderHandler>() ??
                throw new ArgumentNullException(nameof(loggerFactory));
            this.httpContextAccessor = httpContextAccessor ??
                throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            logger.LogInformation($"{nameof(SendAsync)}. Adding headers...");

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
            fullTraceContent += $"{traceIdentifier} ({ApplicationName})";

            request.Headers.Add(XConsumerNameHeaderKey, ApplicationName);
            request.Headers.Add(XCorrelationIdHeaderKey, traceIdentifier);
            request.Headers.Add(XFullTraceHeaderKey, fullTraceContent);

            var rootCorrelationId = requestHeaders.ContainsKey(XRootCorrelationIdHeaderKey) ?
                (string)requestHeaders[XRootCorrelationIdHeaderKey] : traceIdentifier;
            var rootCallerId = requestHeaders.ContainsKey(XRootConsumerNameHeaderKey) ?
                (string)requestHeaders[XRootConsumerNameHeaderKey] : ApplicationName;

            request.Headers.Add(XRootCorrelationIdHeaderKey, rootCorrelationId);
            request.Headers.Add(XRootConsumerNameHeaderKey, rootCallerId);

            logger.LogInformation($"{nameof(SendAsync)}. Headers were added...");

            return base.SendAsync(request, cancellationToken);
        }
    }
}
