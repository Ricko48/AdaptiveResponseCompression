using AdaptiveResponseCompression.Common.Enums;
using System.Net;

namespace AdaptiveResponseCompression.Client.Options;

public class AdaptiveCompressionOptions
{
    private BandwidthAccuracy _bandwidthAccuracy = BandwidthAccuracy.Quick;

    /// <summary>
    /// Specifies the accuracy level for bandwidth estimationMethod by adjusting the duration of the
    /// measurement.
    /// Default value is 'Quick' (up to 5 seconds).
    /// </summary>
    public BandwidthAccuracy BandwidthAccuracy
    {
        get => _bandwidthAccuracy;
        set
        {
            if (!Enum.IsDefined(typeof(BandwidthAccuracy), value))
            {
                throw new ArgumentException(
                    $"Value {value} is not defined in enum {nameof(Common.Enums.BandwidthAccuracy)}");
            }
            _bandwidthAccuracy = value;
        }
    }

    /// <summary>
    /// Represents supported decompression methods.
    /// If the app is running in the browser (e.g. Blazor WebAssembly), this option will be ignored and browser's
    /// decompression algorithms will be used.
    /// Default and recommended to use 'All' since server is adaptively choosing the most efficient encoding.
    /// </summary>
    public DecompressionMethods DecompressionMethods { get; set; } = DecompressionMethods.All;

    /// <summary>
    /// Defines whether the response's sent timestamp header should be used for latency estimationMethod. If false, Round Trip Time (RTT)
    /// will be used instead for latency estimationMethod. Using response's sent timestamp can result in more accurate latency
    /// estimationMethod, but requires time synchronization between client and server. This option is disabled by default.
    /// </summary>
    public bool UseResponseSentTime { get; set; } = false;
}
