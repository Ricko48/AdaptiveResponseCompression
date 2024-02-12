using AdaptiveResponseCompression.Server.CompressionProviders.Interfaces;
using System.IO.Compression;

namespace AdaptiveResponseCompression.Server.Models;

internal class CompressionMethodAnalysis
{
    public IAdaptiveCompressionProvider? BestProvider { get; set; }

    public CompressionLevel? BestLevel { get; set; }
}
