using System.IO.Compression;
using AdaptiveResponseCompression.Server.Dtos;

namespace AdaptiveResponseCompression.Server.Stores.Interfaces;

internal interface IMetricsStore : IMetricsStoreHelper
{
    /// <summary>
    /// Returns compression snapshot for the given encoding, data type, compression level and CPU usage.
    /// If the snapshot does not exist, returns the snapshot for default data type.
    /// Returns null if no snapshot is stored.
    /// </summary>
    /// <param name="encoding"></param>
    /// <param name="route"></param>
    /// <param name="level"></param>
    /// <param name="dataSize"></param>
    /// <returns>Ratio and Speed.</returns>
    (double Ratio, double Speed)? GetClosestMetricByDataSize(string encoding, string route, CompressionLevel level, long dataSize);

    void CreateOrUpdateMetric(MetricsCreateUpdateDto createUpdateDto);
}