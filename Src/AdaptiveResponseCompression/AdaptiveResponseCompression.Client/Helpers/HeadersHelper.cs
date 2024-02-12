using System.Globalization;
using System.Net.Http.Headers;

namespace AdaptiveResponseCompression.Client.Helpers;

internal static class HeadersHelper
{
    public static double? GetHeaderValueAsDouble(HttpHeaders header, string headerName)
    {
        var headerValue = GetHeaderValue(header, headerName);
        if (headerValue != null && double.TryParse(headerValue, CultureInfo.InvariantCulture, out var longValue))
        {
            return longValue;
        }

        return null;
    }

    private static string? GetHeaderValue(HttpHeaders headers, string headerName)
    {
        // custom headers are usually stored in NonValidated property
        var value = GetNonValidatedHeaderValue(headers.NonValidated, headerName);
        if (value != null)
        {
            return value;
        }

        if (headers.TryGetValues(headerName, out var headerValues)
            && headerValues != null)
        {
            return headerValues.FirstOrDefault();
        }

        return null;
    }

    private static string? GetNonValidatedHeaderValue(HttpHeadersNonValidated headersNonValidated, string headerName)
    {
        if (headersNonValidated.TryGetValues(headerName, out var headerValues)
            && headerValues.Count == 1)
        {
            return headerValues.FirstOrDefault();
        }

        return null;
    }
}
