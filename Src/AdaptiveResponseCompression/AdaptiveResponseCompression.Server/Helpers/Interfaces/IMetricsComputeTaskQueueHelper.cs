namespace AdaptiveResponseCompression.Server.Helpers.Interfaces;

internal interface IMetricsComputeTaskQueueHelper
{
    /// <summary>
    /// Decides whether computation task should be qneueud into the queue.
    /// </summary>
    /// <param name="encodings">Encodings</param>
    /// <param name="route">Endpoint route</param>
    /// <param name="dataSize">Response data size</param>
    /// <returns></returns>
    bool ShouldEnqueueComputeTask(IList<string> encodings, string route, long dataSize);
}