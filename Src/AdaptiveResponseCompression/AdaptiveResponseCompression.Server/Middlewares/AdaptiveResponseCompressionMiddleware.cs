using AdaptiveResponseCompression.Server.Attributes;
using AdaptiveResponseCompression.Server.CompressionProviders.Interfaces;
using AdaptiveResponseCompression.Server.Enums;
using AdaptiveResponseCompression.Server.Helpers;
using AdaptiveResponseCompression.Server.Helpers.Interfaces;
using AdaptiveResponseCompression.Server.Models;
using AdaptiveResponseCompression.Server.Options;
using AdaptiveResponseCompression.Server.Services.Interfaces;
using AdaptiveResponseCompression.Server.Streams;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using AdaptiveResponseCompression.Common.Exceptions;
using AdaptiveResponseCompression.Common.Constants;

namespace AdaptiveResponseCompression.Server.Middlewares;

internal class AdaptiveResponseCompressionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAdaptiveResponseCompressionProvider _provider;
    private readonly ICompressionService _compressionService;
    private readonly IMemoryProfiler _memoryProfiler;
    private readonly AdaptiveResponseCompressionOptions _options;
    private readonly ILogger<AdaptiveResponseCompressionMiddleware> _logger;
    private readonly double _maxBandwidth;

    public AdaptiveResponseCompressionMiddleware(
        RequestDelegate next,
        IAdaptiveResponseCompressionProvider provider,
        ICompressionService compressionService,
        IMemoryProfiler memoryProfiler,
        IOptions<AdaptiveResponseCompressionOptions> options,
        ILogger<AdaptiveResponseCompressionMiddleware> logger)
    {
        _next = next;
        _provider = provider;
        _compressionService = compressionService;
        _memoryProfiler = memoryProfiler;
        _options = options.Value;
        _logger = logger;
        _maxBandwidth =
            BandwidthConverter.ConvertIntoBytesPerMillisecond(options.Value.AdaptiveCompressionMaxBandwidth);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_provider.CheckRequestAcceptsCompression(context))
        {
            await _next(context);
            return;
        }

        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        var compressionMethod = GetCompressionMethod(endpoint);

        if (compressionMethod == ResponseCompressionMethod.None)
        {
            _logger.LogDebug(
                "Skipping compression for endpoint with set up compression method {Method}",
                compressionMethod);

            await _next(context);
            return;
        }

        var uploadBandwidth = HeadersHelper.GetUploadBandwidth(context.Request.Headers);
        if (uploadBandwidth != null && uploadBandwidth <= 0)
        {
            throw new InvalidHttpHeaderException($"Header {BandwidthEstimationHeaders.Bandwidth} value must be number bigger than 0");
        }

        if (ShouldUseAdaptiveCompression(uploadBandwidth, compressionMethod))
        {
            _logger.LogDebug(
                "Compressing response with Adaptive compression for the client with bandwidth {Bandwidth} B/ms",
                uploadBandwidth);

            await InvokeAdaptiveCompressionAsync(context, uploadBandwidth!.Value, endpoint);
            return;
        }

        _logger.LogDebug("Compressing response with Standard compression");
        await InvokeStandardCompressionAsync(context);
    }

    private bool ShouldUseAdaptiveCompression(double? uploadBandwidth, ResponseCompressionMethod compressionMethod)
    {
        if (compressionMethod != ResponseCompressionMethod.Adaptive || uploadBandwidth == null)
        {
            return false;
        }

        var memoryUsage = _memoryProfiler.GetMemoryUsage();
        if (memoryUsage > _options.AdaptiveCompressionMaxMemory)
        {
            _logger.LogDebug(
                "Memory usage {MemoryUsage}% reached threshold {MaxMemoryUsage}%",
                memoryUsage,
                _options.AdaptiveCompressionMaxMemory);

            return false;
        }

        if (uploadBandwidth > _maxBandwidth)
        {
            _logger.LogDebug(
                "Bandwidth {UploadBandwidth} bytes/ms reached threshold {AdaptiveCompressionMaxBandwidth} bytes/ms",
                uploadBandwidth,
                _maxBandwidth);

            return false;
        }

        return true;
    }

    private async Task InvokeAdaptiveCompressionAsync(HttpContext context, double uploadBandwidth, Endpoint endpoint)
    {
        var route = (endpoint as RouteEndpoint)?.RoutePattern.RawText;
        if (route == null)
        {
            await _next(context);
            return;
        }

        var compatibleEncodings = _provider.GetCompatibleEncodings(context);

        if (!compatibleEncodings.Any())
        {
            _logger.LogDebug("Skipping compression due to not compatible encodings");
            await _next(context);
            return;
        }

        var originalBodyStream = context.Response.Body;
        var newBodyStream = new MemoryStream();
        var responseBodyStream = new AdaptiveCompressionBody(originalBodyStream, newBodyStream, _provider, context);
        context.Response.Body = responseBodyStream;

        await _next(context);

        if (!responseBodyStream.ShouldBeCompressed)
        {
            await newBodyStream.DisposeAsync();
            return;
        }

        context.Response.Body = originalBodyStream;

        var compressionMethodAnalysis = _provider.GetCompressionAnalysis(uploadBandwidth, route, newBodyStream.Length, compatibleEncodings);

        var compressionModel = new ResponseCompressionModel
        {
            Data = newBodyStream,
            OriginalBodyStream = originalBodyStream,
            UploadBandwidth = uploadBandwidth,
            Analysis = compressionMethodAnalysis,
            Route = route,
            CompatibleEncodings = compatibleEncodings,
        };

        await _compressionService.CompressBodyAsync(context, compressionModel);
    }

    private async Task InvokeStandardCompressionAsync(HttpContext context)
    {
        var originalBodyFeature = context.Features.Get<IHttpResponseBodyFeature>();
        var originalCompressionFeature = context.Features.Get<IHttpsCompressionFeature>();

        Debug.Assert(originalBodyFeature != null);

        var compressionBody = new ResponseCompressionBody(context, _provider, originalBodyFeature);

        context.Features.Set<IHttpResponseBodyFeature>(compressionBody);
        context.Features.Set<IHttpsCompressionFeature>(compressionBody);

        try
        {
            await _next(context);
            await compressionBody.FinishCompressionAsync();
        }
        finally
        {
            context.Features.Set(originalBodyFeature);
            context.Features.Set(originalCompressionFeature);
        }
    }

    private static ResponseCompressionMethod GetCompressionMethod(Endpoint endpoint)
    {
        var attribute = endpoint.Metadata.GetMetadata<ResponseCompressionAttribute>();

        // if not attribute was set up, Standard compression is used
        return attribute?.Method ?? ResponseCompressionMethod.Standard;
    }
}
