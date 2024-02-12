using System.Globalization;
using AdaptiveResponseCompression.Common.Constants;
using AdaptiveResponseCompression.Common.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace AdaptiveResponseCompression.Server.Helpers;

internal static class HeadersHelper
{
    private const string CacheControlHeaderValue = "no-store, no-cache, must-revalidate";
    private const string PragmaHeaderValue = "no-cache";
    private const string ExpiresHeaderValue = "0";
    private const string IdentityContentEncoding = "identity";

    public static double? GetUploadBandwidth(IHeaderDictionary headers)
    {
        var uploadBandwidthHeader = headers[BandwidthEstimationHeaders.Bandwidth].FirstOrDefault();

        if (uploadBandwidthHeader != null
            && double.TryParse(uploadBandwidthHeader, CultureInfo.InvariantCulture, out var uploadBandwidth))
        {
            return uploadBandwidth;
        }

        return null;
    }

    public static void InitializeCompressionHeaders(IHeaderDictionary headers, string encoding)
    {
        var varyValues = headers.GetCommaSeparatedValues(HeaderNames.Vary);
        var varyByAcceptEncoding = varyValues.Any(x => string.Equals(x, HeaderNames.AcceptEncoding, StringComparison.OrdinalIgnoreCase));

        if (!varyByAcceptEncoding)
        {
            headers[HeaderNames.Vary] = StringValues.Concat(headers[HeaderNames.Vary], HeaderNames.AcceptEncoding);
        }

        headers[HeaderNames.ContentEncoding] = StringValues.Concat(headers[HeaderNames.ContentEncoding], encoding);
        headers[HeaderNames.ContentMD5] = default;
        headers.ContentLength = null;
    }

    public static void InitializeBandwidthEstimationHeaders(IHeaderDictionary headers)
    {
        AddNoCompressionHeader(headers);
        AddNoCacheHeaders(headers);
        AddSentTimestampHeader(headers);
    }

    private static void AddNoCompressionHeader(IHeaderDictionary headers)
    {
        headers[HeaderNames.ContentEncoding] = IdentityContentEncoding;
    }

    private static void AddNoCacheHeaders(IHeaderDictionary headers)
    {
        headers[HeaderNames.CacheControl] = CacheControlHeaderValue;
        headers[HeaderNames.Pragma] = PragmaHeaderValue;
        headers[HeaderNames.Expires] = ExpiresHeaderValue;
    }

    private static void AddSentTimestampHeader(IHeaderDictionary headers)
    {
        var sentTimeStamp = TimeStampHelper.GetCurrentUnixMilliseconds();
        headers.Append(BandwidthEstimationHeaders.SentTimestamp, sentTimeStamp.ToString(CultureInfo.InvariantCulture));
    }
}
