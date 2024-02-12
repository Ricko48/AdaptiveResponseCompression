namespace AdaptiveResponseCompression.Common.Helpers;

public static class TimeStampHelper
{
    /// <summary>
    /// Represents utilized way of getting current utc timestamp in milliseconds.
    /// </summary>
    /// <returns>Current timestamp utc in milliseconds.</returns>
    public static double GetCurrentUnixMilliseconds()
    {
        return DateTimeOffset.UtcNow.Subtract(DateTimeOffset.UnixEpoch).TotalMilliseconds;
    }
}
