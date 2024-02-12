namespace AdaptiveResponseCompression.Common.Enums;

/// <summary>
/// Specifies the accuracy level for bandwidth estimation by adjusting the duration of the
/// measurement.
/// </summary>
public enum BandwidthAccuracy
{
    /// <summary>
    /// Estimates bandwidth within approximately up to 5 seconds, providing a quick assessment
    /// with moderate accuracy. Suitable for scenarios where speed is prioritized over precision.
    /// </summary>
    Quick = 5,

    /// <summary>
    /// Balances duration and accuracy by estimating bandwidth within approximately up to 10 seconds,
    /// offering a good compromise between speed and accuracy.
    /// </summary>
    Balanced = 10,

    /// <summary>
    /// Provides a more precise bandwidth estimate by extending the measurement to approximately
    /// up to 15 seconds, prioritizing accuracy over speed.
    /// </summary>
    High = 15,

    /// <summary>
    /// Delivers the highest precision by measuring for approximately up to 20 seconds, ideal for
    /// scenarios where estimation accuracy is critical.
    /// </summary>
    Highest = 20
}