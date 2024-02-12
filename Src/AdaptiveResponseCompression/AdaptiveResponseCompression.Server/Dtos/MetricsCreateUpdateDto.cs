using System.IO.Compression;

namespace AdaptiveResponseCompression.Server.Dtos;

internal class MetricsCreateUpdateDto
{
    public required string Encoding { get; set; }

    /// <summary>
    /// Endpoint route.
    /// </summary>
    public required string Route { get; set; }

    /// <summary>
    /// Bytes per millisecond.
    /// </summary>
    public required double Speed { get; set; }

    /// <summary>
    /// Data size in bytes before compression.
    /// </summary>
    public required long DataSize { get; set; }

    public required double Ratio { get; set; }

    public required CompressionLevel Level { get; set; }
}
