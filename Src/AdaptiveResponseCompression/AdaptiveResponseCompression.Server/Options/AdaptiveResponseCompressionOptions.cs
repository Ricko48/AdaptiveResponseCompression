using AdaptiveResponseCompression.Server.CompressionProviders;
using AdaptiveResponseCompression.Server.CompressionProviders.Interfaces;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

namespace AdaptiveResponseCompression.Server.Options;

public class AdaptiveResponseCompressionOptions : ResponseCompressionOptions
{
    private float _adaptiveCompressionMaxMemory = 100;

    /// <summary>
    /// The <see cref="IAdaptiveCompressionProvider"/> types to use for responses.
    /// Providers are prioritized based on the order they are added.
    /// </summary>
    public new AdaptiveCompressionProviderCollection Providers { get; } = [];

    /// <summary>
    /// Maximal upload bandwidth in kilobytes per second suitable for adaptive compression.
    /// Standard compression will be used in case of higher bandwidth.
    /// Default value is 122 kilobytes per second (1 megabit per second).
    /// </summary>
    public double AdaptiveCompressionMaxBandwidth { get; set; } = 122;

    /// <summary>
    /// Compression levels used for adaptive compression. Default value contains all compression levels.
    /// </summary>
    public IEnumerable<CompressionLevel> AdaptiveCompressionLevels { get; set; } =
    [
        CompressionLevel.Fastest,
        CompressionLevel.Optimal,
        CompressionLevel.NoCompression,
        CompressionLevel.SmallestSize
    ];

    /// <summary>
    /// Sets the max memory usage in percentage for adaptive compression. If the memory limit is reached, response will be compressed using standard compression.
    /// Default value is 100%.
    /// Note: the determining current memory usage may not work on all platforms, in that case it is ignored.
    /// </summary>
    public float AdaptiveCompressionMaxMemory
    {
        get => _adaptiveCompressionMaxMemory;

        set
        {
            if (value > 100 || value < 0)
            {
                throw new ArgumentException($"{nameof(AdaptiveCompressionMaxMemory)} must be percentage between 0 and 100");
            }

            _adaptiveCompressionMaxMemory = value;
        }
    }
}