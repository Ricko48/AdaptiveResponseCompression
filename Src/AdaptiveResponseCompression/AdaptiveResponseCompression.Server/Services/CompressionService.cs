using AdaptiveResponseCompression.Server.Helpers;
using AdaptiveResponseCompression.Server.Helpers.Interfaces;
using AdaptiveResponseCompression.Server.Models;
using AdaptiveResponseCompression.Server.Options;
using AdaptiveResponseCompression.Server.Services.Interfaces;
using AdaptiveResponseCompression.Server.Stores.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AdaptiveResponseCompression.Server.Services;

internal class CompressionService : ICompressionService
{
    private readonly IMetricsComputeTaskQueue _computeTaskQueue;
    private readonly ILogger<CompressionService> _logger;
    private readonly AdaptiveResponseCompressionOptions _options;
    private readonly IMetricsComputeTaskQueueHelper _computeTaskQueueHelper;

    public CompressionService(
        IMetricsComputeTaskQueue computeTaskQueue,
        ILogger<CompressionService> logger,
        IOptions<AdaptiveResponseCompressionOptions> options,
        IMetricsComputeTaskQueueHelper computeTaskQueueHelper)
    {
        _computeTaskQueue = computeTaskQueue;
        _logger = logger;
        _options = options.Value;
        _computeTaskQueueHelper = computeTaskQueueHelper;
    }

    public async Task CompressBodyAsync(HttpContext context, ResponseCompressionModel compressionModel)
    {
        compressionModel.Data.Position = 0;

        var analysis = compressionModel.Analysis;

        if (analysis.BestLevel == null || analysis.BestProvider == null)
        {
            _logger.LogDebug("Sending response without compression");
            await compressionModel.Data.CopyToAsync(compressionModel.OriginalBodyStream);
            await EnqueueMetricsComputeTaskAsync(compressionModel);
            return;
        }

        HeadersHelper.InitializeCompressionHeaders(context.Response.Headers, analysis.BestProvider.EncodingName);

        var provider = analysis.BestProvider;
        var level = analysis.BestLevel;

        await using var compressionStream = provider.CreateStream(compressionModel.OriginalBodyStream, level.Value);

        _logger.LogDebug(
            "Compressing - Encoding: {Encoding} - Compression level: {CompressionLevel}",
            provider.EncodingName,
            level);

        await compressionModel.Data.CopyToAsync(compressionStream);
        await compressionStream.FlushAsync();

        await EnqueueMetricsComputeTaskAsync(compressionModel);
    }

    private async Task EnqueueMetricsComputeTaskAsync(ResponseCompressionModel compressionModel)
    {
        try
        {
            var computeTask = new MetricsComputeTask
            {
                Route = compressionModel.Route,
                Data = compressionModel.Data,
                Encodings = compressionModel.CompatibleEncodings
            };

            _computeTaskQueue.EnqueueTask(computeTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while enqueuing metrics compute task into queue");
            await compressionModel.Data.DisposeAsync();
            GC.Collect();
        }
    }
}
