using AdaptiveResponseCompression.Server.CompressionProviders.Interfaces;
using AdaptiveResponseCompression.Server.Options;
using Microsoft.Extensions.Options;
using System.IO.Compression;

namespace AdaptiveResponseCompression.Server.CompressionProviders;

public class AdaptiveGzipCompressionProvider : IAdaptiveCompressionProvider
{
    /// <summary>
    /// Creates a new instance of AdaptiveGzipCompressionProvider with options.
    /// </summary>
    /// <param name="options"></param>
    public AdaptiveGzipCompressionProvider(IOptions<GzipAdaptiveCompressionOptions> options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        Options = options.Value;
    }

    private GzipAdaptiveCompressionOptions Options { get; }

    /// <inheritdoc />
    public string EncodingName => "gzip";

    /// <inheritdoc />
    public bool SupportsFlush
    {
        get
        {
#if NET461
                return false;
#elif NETSTANDARD2_0 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1 || NET5_0_OR_GREATER
            return true;
#else
#error target frameworks need to be updated
#endif
        }
    }

    public Stream CreateStream(Stream outputStream, CompressionLevel level)
    {
        return new GZipStream(outputStream, level, leaveOpen: true);
    }

    /// <inheritdoc />
    public Stream CreateStream(Stream outputStream)
    {
        return new GZipStream(outputStream, Options.Level, leaveOpen: true);
    }
}
