using System.IO.Compression;

namespace AdaptiveResponseCompression.Server.Models;

internal class CompressionLevelAnalysis
{
    public double BestProcessingTime { get; set; } // ms

    public required CompressionLevel BestLevel { get; set; }
}
