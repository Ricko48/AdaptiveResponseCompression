using AdaptiveResponseCompression.Client.Constants;
using AdaptiveResponseCompression.Client.HttpClients.Interfaces;
using AdaptiveResponseCompression.Client.Stores.Interfaces;
using AdaptiveResponseCompression.Common.Constants;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace AdaptiveResponseCompression.Client.HttpClients;

internal class AdaptiveCompressionClient : IAdaptiveCompressionClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IBandwidthStore _bandwidthStore;
    private readonly ILogger<AdaptiveCompressionClient> _logger;

    public AdaptiveCompressionClient(
        IHttpClientFactory httpClientFactory,
        IBandwidthStore bandwidthStore,
        ILogger<AdaptiveCompressionClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _bandwidthStore = bandwidthStore;
        _logger = logger;
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.RequestUri);

        var httpClient = _httpClientFactory.CreateClient(HttpClientConstants.AdaptiveClient);

        if (timeout != null)
        {
            httpClient.Timeout = timeout.Value;
        }

        await AddBandwidthHeaderAsync(request);

        return await httpClient.SendAsync(request);
    }

    private async Task AddBandwidthHeaderAsync(HttpRequestMessage request)
    {
        ArgumentNullException.ThrowIfNull(request.RequestUri);

        var downloadBandwidth = await _bandwidthStore.GetDownloadBandwidthAsync(request.RequestUri.AbsoluteUri);
        if (downloadBandwidth == null)
        {
            _logger.LogInformation(
                "Sending {Method} request to {Url} without bandwidth header",
                request.Method,
                request.RequestUri);

            return;
        }

        request.Headers.Add(BandwidthEstimationHeaders.Bandwidth, downloadBandwidth.Value.ToString(CultureInfo.InvariantCulture));

        _logger.LogInformation(
            "Sending {Method} request to {Url} with download-bandwidth {DownloadBandwidth}",
            request.Method,
            request.RequestUri,
            downloadBandwidth);
    }
}
