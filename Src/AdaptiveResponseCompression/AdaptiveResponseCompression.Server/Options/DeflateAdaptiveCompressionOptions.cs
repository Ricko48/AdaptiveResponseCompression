using System.IO.Compression;
using Microsoft.Extensions.Options;

namespace AdaptiveResponseCompression.Server.Options;

public class DeflateAdaptiveCompressionOptions : IOptions<DeflateAdaptiveCompressionOptions>
{
    /// <summary>
    /// What level of compression to use for the stream. The default is <see cref="CompressionLevel.Fastest"/>.
    /// </summary>
    public CompressionLevel Level { get; set; } = CompressionLevel.Fastest;

    DeflateAdaptiveCompressionOptions IOptions<DeflateAdaptiveCompressionOptions>.Value => this;
}
