using System.IO.Compression;
using AdaptiveResponseCompression.Server.Options;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;

namespace ApiServerDemo.DeflateCompressionProvider;

/// <summary>
/// Implementation of the DeflateCompressionProvider for use in the standard Response Compression
/// is not provided by the .NET, thus we have to implement it ourselves.
/// </summary>
public class DeflateCompressionProvider : ICompressionProvider
{
    private DeflateAdaptiveCompressionOptions Options { get; }

    public DeflateCompressionProvider(IOptions<DeflateAdaptiveCompressionOptions> options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        Options = options.Value;
    }

    public Stream CreateStream(Stream outputStream)
    {
        return new DeflateStream(outputStream, Options.Level, leaveOpen: true);
    }

    public string EncodingName => "deflate";

    public bool SupportsFlush => true;
}