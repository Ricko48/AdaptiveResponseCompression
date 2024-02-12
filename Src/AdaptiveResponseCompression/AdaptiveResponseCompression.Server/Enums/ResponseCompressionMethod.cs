namespace AdaptiveResponseCompression.Server.Enums;

/// <summary>
/// Determines which response compression method will be used.
/// </summary>
public enum ResponseCompressionMethod
{
    /// <summary>
    /// Standard .NET ResponseCompression will be used.
    /// </summary>
    Standard = 0,

    /// <summary>
    /// AdaptiveResponseCompression will be used.
    /// </summary>
    Adaptive = 1,

    /// <summary>
    /// No response compression will be used.
    /// </summary>
    None = 2,
}