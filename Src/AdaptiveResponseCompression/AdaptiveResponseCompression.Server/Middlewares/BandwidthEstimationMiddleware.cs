using AdaptiveResponseCompression.Common.Constants;
using AdaptiveResponseCompression.Common.Enums;
using AdaptiveResponseCompression.Common.Exceptions;
using AdaptiveResponseCompression.Server.Helpers;
using AdaptiveResponseCompression.Server.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AdaptiveResponseCompression.Server.Middlewares;

internal class BandwidthEstimationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BandwidthEstimationMiddleware> _logger;
    private readonly double _adaptiveCompressionMaxBandwidth; // bytes per millisecond

    private const short BufferLength = 4096;

    public BandwidthEstimationMiddleware(
        RequestDelegate next,
        ILogger<BandwidthEstimationMiddleware> logger,
        IOptions<AdaptiveResponseCompressionOptions> options)
    {
        _next = next;
        _logger = logger;
        _adaptiveCompressionMaxBandwidth =
            BandwidthConverter.ConvertIntoBytesPerMillisecond(options.Value.AdaptiveCompressionMaxBandwidth);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var bandwidthEstimationHeader = context.Request.Headers[BandwidthEstimationHeaders.BandwidthEstimation].FirstOrDefault();
        if (bandwidthEstimationHeader == null)
        {
            await _next(context);
            return;
        }

        var bandwidthAccuracy = GetBandwidthAccuracy(bandwidthEstimationHeader);

        _logger.LogDebug("Bandwidth estimation triggered with requested accuracy {BandwidthAccuracy}", bandwidthAccuracy.ToString());

        context.Response.OnStarting(_ =>
        {
            HeadersHelper.InitializeBandwidthEstimationHeaders(context.Response.Headers);
            return Task.CompletedTask;
        }, context);

        await SendResponseAsync(context, bandwidthAccuracy);
    }

    /// <summary>
    /// Sends response without buffering whole response at once to save memory.
    /// Sending response is cancelled if the client aborted the request to not waste network bandwidth.
    /// </summary>
    private async Task SendResponseAsync(HttpContext context, BandwidthAccuracy bandwidthAccuracy)
    {
        var bytesToWrite = GetResponseSize(bandwidthAccuracy);
        var buffer = new byte[BufferLength];

        while (bytesToWrite > 0)
        {
            var writeLength = (int)Math.Min(buffer.Length, bytesToWrite);

            try
            {
                await context.Response.Body.WriteAsync(buffer, 0, writeLength, context.RequestAborted);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Bandwidth estimation request aborted by the client");
                return;
            }

            bytesToWrite -= writeLength;
        }
    }

    private static BandwidthAccuracy GetBandwidthAccuracy(string bandwidthEstimationHeader)
    {
        if (!int.TryParse(bandwidthEstimationHeader, out var accuracy) || !Enum.IsDefined(typeof(BandwidthAccuracy), accuracy))
        {
            throw new InvalidHttpHeaderException($"Invalid bandwidth estimation header with accuracy value {bandwidthEstimationHeader}");
        }

        return (BandwidthAccuracy)accuracy;
    }

    private long GetResponseSize(BandwidthAccuracy bandwidthAccuracy)
    {
        var duration = (int)bandwidthAccuracy * 1000; // ms
        return (long)(_adaptiveCompressionMaxBandwidth * duration);
    }
}
