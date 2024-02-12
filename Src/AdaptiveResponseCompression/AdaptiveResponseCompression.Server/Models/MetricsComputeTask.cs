namespace AdaptiveResponseCompression.Server.Models;

internal class MetricsComputeTask
{
    public required string Route { get; set; }

    public required MemoryStream Data { get; set; }

    public required IList<string> Encodings { get; set; }
}
