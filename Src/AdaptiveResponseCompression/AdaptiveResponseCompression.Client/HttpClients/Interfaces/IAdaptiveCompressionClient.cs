namespace AdaptiveResponseCompression.Client.HttpClients.Interfaces;

/// <summary>
/// Wrapper for HttpClient which adds bandwidth header to each request for requested host.
/// If bandwidth is not estimated for the requested host, no bandwidth is added into headers.
/// </summary>
public interface IAdaptiveCompressionClient
{
    /// <summary>
    /// Sends request with included download bandwidth header which will trigger adaptive response compression on the server.
    /// If no bandwidth is stored for the host, the request will be sent without the bandwidth header.
    /// </summary>
    /// <param name="request">Request message</param>
    /// <param name="timeout">Request timeout. If not specified, the default timeout 100 seconds will be used.</param>
    /// <returns>Response message</returns>
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, TimeSpan? timeout = null);
}