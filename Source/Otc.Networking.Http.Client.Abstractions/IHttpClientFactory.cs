using System.Net.Http;

namespace Otc.Networking.Http.Client.Abstractions
{
    public interface IHttpClientFactory
    {
        /// <summary>
        /// Create a new HttpClient       
        /// <para>
        /// Warning: If you try to call CreateHttpClient from a code that is executing under a NOT 
        /// valid http request scope, like startup, it will throw an InvalidOperationException 
        /// because it could not retrive TraceIdentifier and other relevant HTTP request headers 
        /// in order to add to HttpClient's DefaultRequestHeaders.
        /// </para>
        /// </summary>
        /// <exception cref="System.InvalidOperationException" />
        HttpClient CreateHttpClient();

        /// <summary>
        /// Create a new HttpClient       
        /// <para>
        /// Warning: If you try to call CreateHttpClient from a code that is executing under a NOT
        /// valid http request scope, like startup, it will throw an InvalidOperationException 
        /// because it could not retrive TraceIdentifier and other relevant HTTP request headers in
        /// order to add to HttpClient's 
        /// DefaultRequestHeaders.
        /// </para>
        /// </summary>
        /// <exception cref="System.InvalidOperationException" />
        HttpClient CreateHttpClient(HttpClientHandler handler);

        /// <summary>
        /// Create a new HttpClient       
        /// <para>
        /// Warning: If you try to call CreateHttpClient from a code that is executing under a NOT 
        /// valid http request scope, like startup, it will throw an InvalidOperationException because 
        /// it could not retrive TraceIdentifier and other relevant HTTP request headers in order 
        /// to add to HttpClient's DefaultRequestHeaders.
        /// </para>
        /// </summary>
        /// <exception cref="System.InvalidOperationException" />
        HttpClient CreateHttpClient(HttpClientHandler handler, bool disposeHandler);
    }
}
