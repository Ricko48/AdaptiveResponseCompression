namespace AdaptiveResponseCompression.Client.Stores.Interfaces;

/// <summary>
/// Thread safe service for storing and retrieving bandwidth for hosts.
/// </summary>
internal interface IBandwidthStore
{
    /// <summary>
    /// Returns the download bandwidth for the host.
    /// </summary>
    /// <param name="host">Host</param>
    /// <returns>Bandwidth or null if no bandwidth for user is stored.</returns>
    Task<double?> GetDownloadBandwidthAsync(string host);

    Task AddOrUpdateDownloadBandwidthAsync(string host, double downloadBandwidth);

    Task RemoveDownloadBandwidthAsync(string host);
}