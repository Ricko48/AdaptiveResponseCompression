using AdaptiveResponseCompression.Client.HttpClients.Interfaces;
using AdaptiveResponseCompression.Client.Services.Interfaces;
using AdaptiveResponseCompression.Client.Stores.Interfaces;
using Microsoft.Extensions.Logging;

namespace AdaptiveResponseCompression.Client.Services;

internal class BandwidthService : IBandwidthService
{
    private readonly IBandwidthStore _bandwidthStore;
    private readonly IBandwidthEstimationClient _client;
    private readonly ILogger<BandwidthService> _logger;

    public BandwidthService(
        IBandwidthStore bandwidthStore,
        IBandwidthEstimationClient client,
        ILogger<BandwidthService> logger)
    {
        _bandwidthStore = bandwidthStore;
        _client = client;
        _logger = logger;
    }

    public async Task<double> UpdateBandwidthForHostAsync(string url)
    {
        _logger.LogInformation("Estimating download bandwidth for host {Host}", url);

        var downloadBandwidth = await _client.EstimateDownloadBandwidthAsync(url);

        await _bandwidthStore.AddOrUpdateDownloadBandwidthAsync(url, downloadBandwidth);

        _logger.LogInformation(
            "Bandwidth updated for host {Host} - download-bandwidth: {DownloadBandwidth} B/ms",
            url,
            downloadBandwidth);

        return downloadBandwidth;
    }

    public async Task<double?> GetBandwidthForHostAsync(string url)
    {
        return await _bandwidthStore.GetDownloadBandwidthAsync(url);
    }

    public async Task RemoveBandwidthForHostAsync(string url)
    {
        await _bandwidthStore.RemoveDownloadBandwidthAsync(url);
    }
}