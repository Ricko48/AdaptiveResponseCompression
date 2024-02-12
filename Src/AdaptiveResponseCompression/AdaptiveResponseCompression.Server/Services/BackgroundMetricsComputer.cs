using AdaptiveResponseCompression.Server.CompressionProviders.Interfaces;
using AdaptiveResponseCompression.Server.Dtos;
using AdaptiveResponseCompression.Server.Models;
using AdaptiveResponseCompression.Server.Stores.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO.Compression;
using AdaptiveResponseCompression.Common.Streams;
using Microsoft.Extensions.Options;
using AdaptiveResponseCompression.Server.Options;

namespace AdaptiveResponseCompression.Server.Services;

internal class BackgroundMetricsComputer : BackgroundService
{
    private readonly IAdaptiveResponseCompressionProvider _provider;
    private readonly IMetricsStore _metricsStore;
    private readonly IMetricsComputeTaskQueue _computeTaskQueue;
    private readonly ILogger<BackgroundMetricsComputer> _logger;
    private readonly IReadOnlyList<CompressionLevel> _levels;
    private readonly TimeSpan _waitTime = TimeSpan.FromSeconds(1); // wait time when there are no tasks in the queue
    
    public BackgroundMetricsComputer(
        IAdaptiveResponseCompressionProvider provider,
        IMetricsStore metricsMetricsStore,
        IMetricsComputeTaskQueue computeTaskQueue,
        ILogger<BackgroundMetricsComputer> logger,
        IOptions<AdaptiveResponseCompressionOptions> options)
    {
        _provider = provider;
        _metricsStore = metricsMetricsStore;
        _computeTaskQueue = computeTaskQueue;
        _logger = logger;
        _levels = _levels = options.Value.AdaptiveCompressionLevels.Distinct().ToList();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ComputeNextTaskAsync(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while computing compression metrics");
            }
        }
    }

    private async Task ComputeNextTaskAsync(CancellationToken stoppingToken)
    {
        var task = _computeTaskQueue.DequeueTask();
        if (task == null)
        {
            await Task.Delay(_waitTime, stoppingToken).ConfigureAwait(false);
            return;
        }

        await ComputeMetricAsync(task);
    }

    private async Task ComputeMetricAsync(MetricsComputeTask computeTask)
    {
        try
        {
            foreach (var encoding in computeTask.Encodings)
            {
                var provider = _provider.GetProvider(encoding);
                if (provider == null)
                {
                    _logger.LogWarning("Provider for encoding {Encoding} not found. Skipping...", encoding);
                    continue;
                }

                await ComputeForProviderAsync(computeTask, provider);
            }
        }
        finally
        {
            await computeTask.Data.DisposeAsync();
            GC.Collect();
        }
    }

    private async Task ComputeForProviderAsync(
        MetricsComputeTask computeTask,
        IAdaptiveCompressionProvider provider)
    {
        foreach (var level in _levels)
        {
            try
            {
                // if metric was already computed by another task from queue, it will be skipped
                if (!_metricsStore.ShouldBeNewMetricAdded(provider.EncodingName, computeTask.Route, level, computeTask.Data.Length))
                {
                    continue;
                }

                await ComputeForLevelAsync(computeTask, provider, level);
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    "Error while computing compression metrics for encoding {Encoding} with compression level {Level}",
                    provider.EncodingName,
                    level);
            }
        }
    }

    private async Task ComputeForLevelAsync(
        MetricsComputeTask computeTask,
        IAdaptiveCompressionProvider provider,
        CompressionLevel level)
    {
        var compressedStream = new TrackingStream(Stream.Null);
        await using var compressionStream = provider.CreateStream(compressedStream, level);

        computeTask.Data.Position = 0;

        var stopwatch = Stopwatch.StartNew();

        await computeTask.Data.CopyToAsync(compressionStream);
        await compressionStream.FlushAsync();

        stopwatch.Stop();

        var compressedDataSize = compressedStream.NumberOfWrittenBytes;
        var ratio = (double)compressedDataSize / computeTask.Data.Length;
        var speed = computeTask.Data.Length / stopwatch.Elapsed.TotalMilliseconds;

        // ToDo debug
        _logger.LogInformation(
            "Metric computed - Encoding: {Encoding} - level: {Level} - original size: {OriginalLength} B - compressed size: " +
            "{CompressedLength} B - speed: {Speed} B/ms - ratio: {Ratio}",
            provider.EncodingName, level, computeTask.Data.Length, compressedDataSize, speed, ratio);

        var metric = new MetricsCreateUpdateDto
        {
            Encoding = provider.EncodingName,
            Route = computeTask.Route,
            Level = level,
            Ratio = ratio,
            DataSize = computeTask.Data.Length,
            Speed = speed
        };

        _metricsStore.CreateOrUpdateMetric(metric);
    }
}
