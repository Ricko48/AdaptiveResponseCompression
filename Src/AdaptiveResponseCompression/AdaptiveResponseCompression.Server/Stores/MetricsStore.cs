using AdaptiveResponseCompression.Server.Dtos;
using AdaptiveResponseCompression.Server.Models;
using AdaptiveResponseCompression.Server.Stores.Interfaces;
using AutoMapper;
using System.Collections.Concurrent;
using System.IO.Compression;

namespace AdaptiveResponseCompression.Server.Stores;

internal class MetricsStore : IMetricsStore
{
    private readonly ConcurrentDictionary<string, ConcurrentBag<CompressionMetrics>> _metrics = new();
    private readonly IMapper _mapper;
    private const byte Coefficient = 2; // used to determine whether new metric should be added

    public MetricsStore(IMapper mapper)
    {
        _mapper = mapper;
    }

    public (double Ratio, double Speed)? GetClosestMetricByDataSize(
        string encoding,
        string route,
        CompressionLevel level,
        long dataSize)
    {
        var key = GetKey(encoding, route, level);

        if (!_metrics.TryGetValue(key, out var metrics))
        {
            return null;
        }

        var closest = GetClosestMetricByDataSizeInternal(dataSize, metrics);

        return closest == null ? null : (closest.Ratio, closest.Speed);
    }

    public void CreateOrUpdateMetric(MetricsCreateUpdateDto createUpdateDto)
    {
        var key = GetKey(createUpdateDto.Encoding, createUpdateDto.Route, createUpdateDto.Level);

        _metrics.AddOrUpdate(
            key, _ => CreateMetric(createUpdateDto),
            (_, metrics) => UpdateMetric(metrics, createUpdateDto));
    }

    public bool ShouldBeNewMetricAdded(string encoding, string route, CompressionLevel level, long dataSize)
    {
        var key = GetKey(encoding, route, level);

        if (!_metrics.TryGetValue(key, out var metrics))
        {
            return true;
        }

        return ShouldAddNewMetric(metrics, dataSize);
    }

    private static bool ShouldAddNewMetric(IReadOnlyCollection<CompressionMetrics> metrics, long dataSize)
    {
        var biggerValue = dataSize * Coefficient;
        var smallerValue = dataSize / Coefficient;
        return !metrics.Any(x => x.DataSize < biggerValue && x.DataSize > smallerValue);
    }

    private ConcurrentBag<CompressionMetrics> CreateMetric(MetricsCreateUpdateDto createUpdateDto)
    {
        var metrics = new ConcurrentBag<CompressionMetrics>();
        var metric = _mapper.Map<CompressionMetrics>(createUpdateDto);
        metrics.Add(metric);
        return metrics;
    }

    private ConcurrentBag<CompressionMetrics> UpdateMetric(
        ConcurrentBag<CompressionMetrics> metrics,
        MetricsCreateUpdateDto createUpdateDto)
    {
        if (!ShouldAddNewMetric(metrics, createUpdateDto.DataSize))
        {
            return metrics;
        }

        var metric = _mapper.Map<CompressionMetrics>(createUpdateDto);
        metrics.Add(metric);
        return metrics;
    }

    private static CompressionMetrics? GetClosestMetricByDataSizeInternal(long dataSize, IReadOnlyCollection<CompressionMetrics> metrics)
    {
        CompressionMetrics? closest = null;
        var closestSizeDifference = long.MaxValue;

        foreach (var metric in metrics)
        {
            var currentDiff = Math.Abs(metric.DataSize - dataSize);

            if (currentDiff >= closestSizeDifference)
            {
                continue;
            }

            closest = metric;
            closestSizeDifference = currentDiff;
        }

        return closest;
    }

    private static string GetKey(string encoding, string route, CompressionLevel level)
    {
        return $"{route}:{encoding}:{(int)level}";
    }
}