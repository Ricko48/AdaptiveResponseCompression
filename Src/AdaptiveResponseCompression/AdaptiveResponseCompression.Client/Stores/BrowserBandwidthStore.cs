using AdaptiveResponseCompression.Client.Stores.Interfaces;
using Blazored.LocalStorage;

namespace AdaptiveResponseCompression.Client.Stores;

internal class BrowserBandwidthStore : IBandwidthStore
{
    private readonly ILocalStorageService _localStorage;

    public BrowserBandwidthStore(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<double?> GetDownloadBandwidthAsync(string host)
    {
        var key = GetHostKey(host);
        if (!await _localStorage.ContainKeyAsync(key))
        {
            return null;
        }

        return await _localStorage.GetItemAsync<double>(key);
    }

    public async Task AddOrUpdateDownloadBandwidthAsync(string host, double downloadBandwidth)
    {
        var key = GetHostKey(host);
        await _localStorage.SetItemAsync(key, downloadBandwidth);
    }

    public async Task RemoveDownloadBandwidthAsync(string host)
    {
        var key = GetHostKey(host);
        await _localStorage.RemoveItemAsync(key);
    }

    private static string GetHostKey(string host)
    {
        var uri = new Uri(host);
        return $"{uri.Scheme}:{uri.Host}:{uri.Port}";
    }
}