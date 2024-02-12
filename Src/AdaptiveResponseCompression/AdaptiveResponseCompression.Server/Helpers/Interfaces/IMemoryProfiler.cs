namespace AdaptiveResponseCompression.Server.Helpers.Interfaces;

internal interface IMemoryProfiler
{
    /// <summary>
    /// Get the current physical memory usage in percentage.
    /// </summary>
    /// <returns>Memory usage in percentage.</returns>
    double GetMemoryUsage();
}