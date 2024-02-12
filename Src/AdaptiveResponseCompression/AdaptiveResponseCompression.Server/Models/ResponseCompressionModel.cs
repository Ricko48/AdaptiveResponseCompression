namespace AdaptiveResponseCompression.Server.Models;

internal class ResponseCompressionModel
{
    public required CompressionMethodAnalysis Analysis { get; set; }

    public required MemoryStream Data { get; set; }

    public required Stream OriginalBodyStream { get; set; }

    public required double UploadBandwidth { get; set; }

    public required string Route { get; set; }

    public required IList<string> CompatibleEncodings { get; set; }
}
