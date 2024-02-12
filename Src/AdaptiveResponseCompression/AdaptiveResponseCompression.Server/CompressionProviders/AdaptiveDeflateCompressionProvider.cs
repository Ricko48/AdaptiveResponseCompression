using System.IO.Compression;
using AdaptiveResponseCompression.Server.CompressionProviders.Interfaces;
using AdaptiveResponseCompression.Server.Options;
using Microsoft.Extensions.Options;

namespace AdaptiveResponseCompression.Server.CompressionProviders;

public class AdaptiveDeflateCompressionProvider : IAdaptiveCompressionProvider
{
    /// <summary>
    /// Creates a new instance of AdaptiveDeflateCompressionProvider with options.
    /// </summary>
    /// <param name="options"></param>
    public AdaptiveDeflateCompressionProvider(IOptions<DeflateAdaptiveCompressionOptions> options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        Options = options.Value;
    }

    private DeflateAdaptiveCompressionOptions Options { get; }

    public string EncodingName => "deflate";

    public bool SupportsFlush => true;

    public Stream CreateStream(Stream outputStream, CompressionLevel level)
    {
        return new DeflateStream(outputStream, level, leaveOpen: true);
    }

    public Stream CreateStream(Stream outputStream)
    {
        return new DeflateStream(outputStream, Options.Level, leaveOpen: true);
    }
}
