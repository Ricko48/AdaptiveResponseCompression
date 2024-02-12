namespace AdaptiveResponseCompression.Common.Constants;

/// <summary>
/// Http headers used for bandwidth estimation.
/// </summary>
public static class BandwidthEstimationHeaders
{
    public const string SentTimestamp = "X-Sent-Timestamp";

    public const string Bandwidth = "X-Bandwidth";

    public const string BandwidthEstimation = "X-Bandwidth-Estimation";
}