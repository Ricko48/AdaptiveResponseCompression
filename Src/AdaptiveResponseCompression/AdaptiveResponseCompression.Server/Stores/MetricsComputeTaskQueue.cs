using AdaptiveResponseCompression.Server.Models;
using AdaptiveResponseCompression.Server.Stores.Interfaces;
using System.Collections.Concurrent;

namespace AdaptiveResponseCompression.Server.Stores;

internal class MetricsComputeTaskQueue : IMetricsComputeTaskQueue
{
    private readonly ConcurrentQueue<MetricsComputeTask> _tasks = new();
    private const byte Coefficient = 2; // used to determine whether new metric task should be added

    public void EnqueueTask(MetricsComputeTask computeTask)
    {
        _tasks.Enqueue(computeTask);
    }

    public MetricsComputeTask? DequeueTask()
    {
        return _tasks.TryDequeue(out var computeTask) ? computeTask : null;
    }

    public bool ShouldEnqueueComputeTask(IList<string> encodings, string route, long dataSize)
    {
        var biggerValue = dataSize * Coefficient;
        var smallerValue = dataSize / Coefficient;

        foreach (var task in _tasks)
        {
            if (string.Equals(task.Route, route, StringComparison.OrdinalIgnoreCase)
                && encodings.Any(x => task.Encodings.Any(z => string.Equals(x, z, StringComparison.OrdinalIgnoreCase)))
                && task.Data.Length < biggerValue && task.Data.Length > smallerValue)
            {
                return false;
            }
        }

        return true;
    }
}
