using AdaptiveResponseCompression.Client.Stores.Interfaces;
using System.Collections.Concurrent;

namespace AdaptiveResponseCompression.Client.Stores;

internal class BandwidthStore : IBandwidthStore
{
    private readonly ConcurrentDictionary<string, double> _bandwidths = new();

    public Task<double?> GetDownloadBandwidthAsync(string host)
    {
        var key = GetHostKey(host);

        if (_bandwidths.TryGetValue(key, out var bandwidth))
        {
            return Task.FromResult((double?)bandwidth);
        }

        return Task.FromResult<double?>(null);
    }

    public Task AddOrUpdateDownloadBandwidthAsync(string host, double downloadBandwidth)
    {
        var key = GetHostKey(host);
        _bandwidths.AddOrUpdate(key, _ => downloadBandwidth, (_, _) => downloadBandwidth);
        return Task.CompletedTask;
    }

    public Task RemoveDownloadBandwidthAsync(string host)
    {
        var key = GetHostKey(host);
        _bandwidths.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    private static string GetHostKey(string host)
    {
        var uri = new Uri(host);
        return $"{uri.Scheme}:{uri.Host}:{uri.Port}";
    }
}
