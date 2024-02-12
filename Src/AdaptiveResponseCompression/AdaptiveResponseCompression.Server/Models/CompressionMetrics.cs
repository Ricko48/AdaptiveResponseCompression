namespace AdaptiveResponseCompression.Server.Models;

internal class CompressionMetrics
{
    /// <summary>
    /// Compressed size / original size.
    /// </summary>
    public required double Ratio { get; set; }

    /// <summary>
    /// Original size of the data before compression.
    /// </summary>
    public required long DataSize { get; set; }

    /// <summary>
    /// Compression speed in Bytes per millisecond.
    /// </summary>
    public required double Speed { get; set; }
}