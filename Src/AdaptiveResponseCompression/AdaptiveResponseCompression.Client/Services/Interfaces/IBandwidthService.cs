namespace AdaptiveResponseCompression.Client.Services.Interfaces;

public interface IBandwidthService
{
    /// <summary>
    /// Estimates bandwidth for a given url.
    /// </summary>
    /// <param name="url">Server url.</param>
    /// <returns>Estimated bandwidth in bytes per millisecond.</returns>
    Task<double> UpdateBandwidthForHostAsync(string url);

    /// <summary>
    /// Returns newest download bandwidth estimated for given url. If the bandwidth for given url isn't
    /// estimated yet, the null is returned.
    /// </summary>
    /// <param name="url">Host</param>
    /// <returns>Bandwidth or null.</returns>
    Task<double?> GetBandwidthForHostAsync(string url);

    /// <summary>
    /// Removes estimated bandwidth for given host.
    /// </summary>
    /// <param name="url">Host</param>
    Task RemoveBandwidthForHostAsync(string url);
}