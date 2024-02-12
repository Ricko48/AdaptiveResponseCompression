using AdaptiveResponseCompression.Server.Models;
using AdaptiveResponseCompression.Server.Options;
using AdaptiveResponseCompression.Server.Services.Interfaces;
using AdaptiveResponseCompression.Server.Stores.Interfaces;
using Microsoft.Extensions.Options;
using System.IO.Compression;

namespace AdaptiveResponseCompression.Server.Services;

internal class CompressionLevelComputer : ICompressionLevelComputer
{
    private readonly IMetricsStore _metricsStore;
    private readonly IReadOnlyList<CompressionLevel> _levels;

    public CompressionLevelComputer(
        IMetricsStore metricsStore,
        IOptions<AdaptiveResponseCompressionOptions> options)
    {
        _metricsStore = metricsStore;
        _levels = options.Value.AdaptiveCompressionLevels.Distinct().ToList();
    }

    public CompressionLevelAnalysis GetCompressionLevelAnalysis(
        double uploadBandwidth,
        string encoding,
        string route,
        long dataSize)
    {
        var bestCompressionLevel = CompressionLevel.Fastest;
        var bestProcessingTime = double.MaxValue;

        foreach (var level in _levels)
        {
            var metrics = _metricsStore.GetClosestMetricByDataSize(encoding, route, level, dataSize);

            if (metrics == null)
            {
                continue;
            }

            var processingTime = GetExpectedProcessingTime(dataSize, uploadBandwidth, metrics.Value.Ratio, metrics.Value.Speed);

            if (processingTime < bestProcessingTime)
            {
                bestProcessingTime = processingTime;
                bestCompressionLevel = level;
            }
        }

        return new CompressionLevelAnalysis
        {
            BestProcessingTime = bestProcessingTime,
            BestLevel = bestCompressionLevel,
        };
    }

    private static double GetExpectedProcessingTime(
        long dataSize,
        double uploadBandwidth,
        double ratio,
        double speed)
    {
        var expectedCompressedSize = dataSize * ratio; // Bytes
        var expectedUploadTime = expectedCompressedSize / uploadBandwidth; // milliseconds
        var expectedCompressionTime = dataSize / speed; // millisecond

        return expectedUploadTime + expectedCompressionTime;
    }
}
