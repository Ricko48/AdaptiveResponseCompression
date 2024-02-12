namespace AdaptiveResponseCompression.Server.Helpers;
internal static class BandwidthConverter
{
    public static double ConvertIntoBytesPerMillisecond(double kiloBytesPerSecond)
    {
        return kiloBytesPerSecond * 1024 / 1000;
    }
}
