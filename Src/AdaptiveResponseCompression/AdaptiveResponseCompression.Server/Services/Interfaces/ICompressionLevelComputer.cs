using AdaptiveResponseCompression.Server.Models;

namespace AdaptiveResponseCompression.Server.Services.Interfaces;

internal interface ICompressionLevelComputer
{
    CompressionLevelAnalysis GetCompressionLevelAnalysis(double uploadBandwidth, string encoding, string route, long dataSize);
}