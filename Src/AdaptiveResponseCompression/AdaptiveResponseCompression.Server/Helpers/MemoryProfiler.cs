using AdaptiveResponseCompression.Server.Helpers.Interfaces;
using Microsoft.Extensions.Logging;
using NickStrupat;

namespace AdaptiveResponseCompression.Server.Helpers;

internal class MemoryProfiler : IMemoryProfiler
{
    private readonly ILogger<MemoryProfiler> _logger;

    public MemoryProfiler(ILogger<MemoryProfiler> logger)
    {
        _logger = logger;
    }

    public double GetMemoryUsage()
    {
        try
        {
            var computerInfo = new ComputerInfo();
            var totalMemory = computerInfo.TotalPhysicalMemory;
            var availableMemory = computerInfo.AvailablePhysicalMemory;
            var usedMemory = totalMemory - availableMemory;
            return (double)usedMemory / totalMemory * 100;
        }
        catch (Exception /*ex*/)
        {
            //_logger.LogDebug(ex, "An error occurred while retrieving memory usage");
            return 0;
        }
    }
}