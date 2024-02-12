using AdaptiveResponseCompression.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;

namespace AdaptiveResponseCompression.Server.CompressionProviders.Interfaces;

internal interface IAdaptiveResponseCompressionProvider : IResponseCompressionProvider
{
    /// <summary>
    /// Examines the request and selects an acceptable compression provider, if any.
    /// </summary>
    /// <param name="context"></param>
    /// <returns>A compression provider or null if compression should not be used.</returns>
    IAdaptiveCompressionProvider GetAdaptiveCompressionProvider(HttpContext context);

    CompressionMethodAnalysis GetCompressionAnalysis(double uploadBandwidth, string route, long dataSize, IList<string> encodings);

    IList<string> GetCompatibleEncodings(HttpContext context);

    IAdaptiveCompressionProvider? GetProvider(string encoding);
}
