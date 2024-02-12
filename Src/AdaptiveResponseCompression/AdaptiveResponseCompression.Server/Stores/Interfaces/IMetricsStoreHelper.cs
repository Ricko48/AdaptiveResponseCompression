using System.IO.Compression;

namespace AdaptiveResponseCompression.Server.Stores.Interfaces;

internal interface IMetricsStoreHelper
{
    bool ShouldBeNewMetricAdded(string encoding, string route, CompressionLevel level, long dataSize);
}