using System.IO.Compression;
using Microsoft.Extensions.Options;

namespace ApiServerDemo.DeflateCompressionProvider;

public class DeflateCompressionProviderOptions : IOptions<DeflateCompressionProviderOptions>
{
    /// <summary>
    /// What level of compression to use for the stream. The default is <see cref="CompressionLevel.Fastest"/>.
    /// </summary>
    public CompressionLevel Level { get; set; } = CompressionLevel.Fastest;

    DeflateCompressionProviderOptions IOptions<DeflateCompressionProviderOptions>.Value => this;
}