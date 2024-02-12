using Microsoft.Extensions.Options;
using System.IO.Compression;

namespace AdaptiveResponseCompression.Server.Options;

public class BrotliAdaptiveCompressionOptions : IOptions<BrotliAdaptiveCompressionOptions>
{
    /// <summary>
    /// What level of compression to use for the stream. The default is <see cref="CompressionLevel.Fastest"/>.
    /// </summary>
    public CompressionLevel Level { get; set; } = CompressionLevel.Fastest;

    BrotliAdaptiveCompressionOptions IOptions<BrotliAdaptiveCompressionOptions>.Value => this;
}