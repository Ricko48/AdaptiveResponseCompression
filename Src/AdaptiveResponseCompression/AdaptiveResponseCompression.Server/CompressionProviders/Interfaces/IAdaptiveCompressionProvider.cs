using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;

namespace AdaptiveResponseCompression.Server.CompressionProviders.Interfaces;

public interface IAdaptiveCompressionProvider : ICompressionProvider
{
    /// <summary>
    /// Create a new compression stream with given compression level.
    /// </summary>
    /// <param name="outputStream">The stream where the compressed data have to be written</param>
    /// <param name="level">Compression level</param>
    /// <returns>The compression stream</returns>
    Stream CreateStream(Stream outputStream, CompressionLevel level);
}
