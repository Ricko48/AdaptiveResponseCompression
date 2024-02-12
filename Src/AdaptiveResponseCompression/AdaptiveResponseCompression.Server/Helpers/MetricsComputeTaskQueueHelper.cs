using AdaptiveResponseCompression.Server.Helpers.Interfaces;
using AdaptiveResponseCompression.Server.Options;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using AdaptiveResponseCompression.Server.Stores.Interfaces;

namespace AdaptiveResponseCompression.Server.Helpers;

internal class MetricsComputeTaskQueueHelper : IMetricsComputeTaskQueueHelper
{
    private readonly IMetricsStoreHelper _store;
    private readonly IReadOnlyList<CompressionLevel> _levels;
    private readonly IMetricsComputeTaskQueue _queue;

    public MetricsComputeTaskQueueHelper(
        IOptions<AdaptiveResponseCompressionOptions> options,
        IMetricsStoreHelper store,
        IMetricsComputeTaskQueue queue)
    {
        _levels = _levels = options.Value.AdaptiveCompressionLevels.Distinct().ToList();
        _store = store;
        _queue = queue;
    }

    public bool ShouldEnqueueComputeTask(IList<string> encodings, string route, long dataSize)
    {
        if (!_queue.ShouldEnqueueComputeTask(encodings, route, dataSize)) // checks queued tasks
        {
            return false;
        }

        foreach (var encoding in encodings)
        {
            foreach (var level in _levels)
            {
                if (_store.ShouldBeNewMetricAdded(encoding, route, level, dataSize)) // check stored metrics in memory
                {
                    return true;
                }
            }
        }

        return false;
    }
}