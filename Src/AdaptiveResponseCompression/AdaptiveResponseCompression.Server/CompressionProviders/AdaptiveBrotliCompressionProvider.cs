using AdaptiveResponseCompression.Server.CompressionProviders.Interfaces;
using AdaptiveResponseCompression.Server.Options;
using Microsoft.Extensions.Options;
using System.IO.Compression;

namespace AdaptiveResponseCompression.Server.CompressionProviders;

public class AdaptiveBrotliCompressionProvider : IAdaptiveCompressionProvider
{
    /// <summary>
    /// Creates a new instance of <see cref="AdaptiveBrotliCompressionProvider"/> with options.
    /// </summary>
    /// <param name="options"></param>
    public AdaptiveBrotliCompressionProvider(IOptions<BrotliAdaptiveCompressionOptions> options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        Options = options.Value;
    }

    private BrotliAdaptiveCompressionOptions Options { get; }

    /// <inheritdoc />
    public string EncodingName => "br";

    /// <inheritdoc />
    public bool SupportsFlush => true;

    public Stream CreateStream(Stream outputStream, CompressionLevel level)
    {
#if NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1 || NET5_0_OR_GREATER
        return new BrotliStream(outputStream, level, leaveOpen: true);
#elif NET461 || NETSTANDARD2_0
        // Brotli is only supported in .NET Core 2.1+
        throw new PlatformNotSupportedException();
#else
#error Target frameworks need to be updated.
#endif
    }

    /// <inheritdoc />
    public Stream CreateStream(Stream outputStream)
    {
#if NETCOREAPP2_1 || NETCOREAPP3_1 || NET5_0_OR_GREATER
        return new BrotliStream(outputStream, Options.Level, leaveOpen: true);
#elif NET461 || NETSTANDARD2_0
        // Brotli is only supported in .NET Core 2.1+
        throw new PlatformNotSupportedException();
#else
#error Target frameworks need to be updated.
#endif
    }
}
