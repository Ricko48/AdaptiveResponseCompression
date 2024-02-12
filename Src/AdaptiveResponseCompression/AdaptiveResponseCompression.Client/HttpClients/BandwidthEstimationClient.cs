using AdaptiveResponseCompression.Client.Constants;
using AdaptiveResponseCompression.Client.Helpers;
using AdaptiveResponseCompression.Client.HttpClients.Interfaces;
using AdaptiveResponseCompression.Client.Options;
using AdaptiveResponseCompression.Common.Constants;
using AdaptiveResponseCompression.Common.Exceptions;
using AdaptiveResponseCompression.Common.Helpers;
using AdaptiveResponseCompression.Common.Streams;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
namespace AdaptiveResponseCompression.Client.HttpClients;

internal class BandwidthEstimationClient : IBandwidthEstimationClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AdaptiveCompressionOptions _options;

    public BandwidthEstimationClient(
        IHttpClientFactory httpClientFactory,
        IOptions<AdaptiveCompressionOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<double> EstimateDownloadBandwidthAsync(string host)
    {
        var httpClient = _httpClientFactory.CreateClient(HttpClientConstants.BandwidthEstimationClient);
        using var request = GetRequestMessage(host);

        return _options.UseResponseSentTime
            ? await GetBandwidthUsingSentTimeAsync(request, httpClient)
            : await GetBandwidthUsingRttAsync(request, httpClient);
    }

    private HttpRequestMessage GetRequestMessage(string host)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(host));

        // disables cache to get relevant results
        request.Headers.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true,
            NoStore = true,
            MustRevalidate = true
        };

        request.Headers.Pragma.ParseAdd("no-cache");

        // enables response streaming for Blazor WebAssembly
        request.SetBrowserResponseStreamingEnabled(true);

        AddBandwidthEstimationHeader(request.Headers);

        return request;
    }

    private void AddBandwidthEstimationHeader(HttpHeaders headers)
    {
        headers.Add(
            BandwidthEstimationHeaders.BandwidthEstimation,
            ((int)_options.BandwidthAccuracy).ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Sends the request and returns the download bandwidth. Latency is estimated using the
    /// Round Trip Time (RTT).
    /// </summary>
    private async Task<double> GetBandwidthUsingRttAsync(
        HttpRequestMessage request,
        HttpClient client)
    {
        await using var trackingStream = new TrackingStream(Stream.Null);
        var token = GetCancellationToken();

        var stopwatch = Stopwatch.StartNew();
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        try
        {
            await response.Content.CopyToAsync(trackingStream, token);
        }
        catch (OperationCanceledException)
        {
        }

        stopwatch.Stop();

        var responseSize = trackingStream.NumberOfWrittenBytes + response.Headers.ToString().Length;
        return responseSize / stopwatch.Elapsed.TotalMilliseconds;
    }

    /// <summary>
    /// Sends the request and returns the download bandwidth. Latency is estimated using the server's
    /// sent timestamp header from the response.
    /// </summary>
    private async Task<double> GetBandwidthUsingSentTimeAsync(
        HttpRequestMessage request,
        HttpClient client)
    {
        await using var trackingStream = new TrackingStream(Stream.Null);
        var token = GetCancellationToken();

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        try
        {
            await response.Content.CopyToAsync(trackingStream, token);
        }
        catch (OperationCanceledException)
        {
        }

        var receivedTime = TimeStampHelper.GetCurrentUnixMilliseconds();

        var sentTimestamp = GetSendTimeStamp(response.Headers);
        var latency = receivedTime - sentTimestamp;
        var responseSize = trackingStream.NumberOfWrittenBytes + response.Headers.ToString().Length;

        return responseSize / latency;
    }

    private CancellationToken GetCancellationToken()
    {
        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(TimeSpan.FromMilliseconds(GetRequestDuration()));
        return tokenSource.Token;
    }

    private static double GetSendTimeStamp(HttpHeaders headers)
    {
        var sentTimestamp = HeadersHelper.GetHeaderValueAsDouble(headers, BandwidthEstimationHeaders.SentTimestamp);
        if (sentTimestamp == null)
        {
            throw new InvalidHttpHeaderException("Sent timestamp is not present in the response headers");
        }

        return sentTimestamp.Value;
    }

    private int GetRequestDuration()
    {
        return (int)_options.BandwidthAccuracy * 1000; // ms
    }
}
