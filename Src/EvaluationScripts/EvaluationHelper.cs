using System.Net.Http.Headers;

namespace EvaluationScripts;

public static class EvaluationHelper
{
    public static double TruncateToTwoDecimals(double value)
    {
        return (double)((long)(value * 100)) / 100;
    }

    public static HttpRequestMessage GetRequestMessage(string fileSize)
    {
        var builder = new UriBuilder(Config.Host)
        {
            Path = $"api/testData/json/{fileSize}"
        };

        var message = new HttpRequestMessage(HttpMethod.Get, builder.Uri);

        // disables cache to get relevant results
        message.Headers.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true,
            NoStore = true,
            MustRevalidate = true
        };

        message.Headers.Pragma.ParseAdd("no-cache");

        return message;
    }

    public static double ConvertToKBps(double bpms)
    {
        var kbps = bpms / 1024 * 1000;
        return TruncateToTwoDecimals(kbps);
    }
}