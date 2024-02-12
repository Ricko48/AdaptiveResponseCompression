using AdaptiveResponseCompression.Server.Models;

namespace AdaptiveResponseCompression.Server.Stores.Interfaces;

internal interface IMetricsComputeTaskQueue
{
    /// <summary>
    /// Enqueues a task to be processed by the background service. Task can be processed immediately.
    /// </summary>
    /// <param name="computeTask"></param>
    void EnqueueTask(MetricsComputeTask computeTask);

    /// <summary>
    /// Dequeues a task to be processed by the background service. Returns null if no task is available.
    /// This method should be used only by the background service.
    /// </summary>
    /// <returns></returns>
    MetricsComputeTask? DequeueTask();

    bool ShouldEnqueueComputeTask(IList<string> encodings, string route, long dataSize);
}