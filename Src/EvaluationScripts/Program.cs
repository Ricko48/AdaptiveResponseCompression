using System.IO.Compression;
using EvaluationScripts;

// Ensure that your ServerDemo project is running and serverUrl is correct
// either Adaptive or Standard compression evaluation can be running at the same time
// before each run, set up correct response compression in Program.cs of the ApiServerDemo project before the run

// before running Adaptive Response Compression evaluation, ensure that AdaptiveResponseCompression is correctly registered
// in Program.cs of the ApiServerDemo project
await AdaptiveResponseCompressionEvaluation.RunAsync(); // ToDo uncomment here

// before running Standard Response Compression evaluation, ensure that ResponseCompression is correctly registered
// in program.cs of the ServerDemo project
// also ensure that all three default encodings Brotli, GZip and Deflate are set in the options in ApiServerDemo project in Program.cs
//await StandardResponseCompressionEvaluation.RunAsync();  // ToDo uncomment here

// ToDo configure evaluation first!
namespace EvaluationScripts
{
    public static class Config
    {
        /// <summary>
        /// Insert your server's url here.
        /// </summary>
        public const string Host = "ApiServerDemo IPAddress/url";

        /// <summary>
        /// Insert full folder path where report file should be created.
        /// </summary>
        public const string FolderPath = @".";

        /// <summary>
        /// Insert configured compression level for standard compression
        /// (will be used for naming the sheet in .xlsx file for standard compression).
        /// Not needed for adaptive compression since it uses all the compression levels.
        /// </summary>
        public static CompressionLevel CompressionLevel = CompressionLevel.Fastest;

        /// <summary>
        /// Insert your limited download bandwidth in KB/s (will be used for naming the sheet in the
        /// .xlsx file for standard compression).
        /// Not needed for adaptive compression since it estimates bandwidth by itself.
        /// </summary>
        public const double DownloadBandwidthKBps = 1000;

        /// <summary>
        /// Specify how many requests should be sent for a single combination on fhe data type and size to compute average RTT.
        /// </summary>
        public const int NumberOfRequests = 3;

        /// <summary>
        /// File sizes used for the evaluation.
        /// You might want to exclude bigger files in case of the low bandwidth and standard compression.
        /// </summary>
        public static string[] FileSizes { get; } =
        [
            //"1B",
            //"500B",
            "1KB",
            //"5KB",
            "10KB",
            //"50KB",
            "100KB",
            //"500KB",
            "1MB",
            //"5MB",
            "10MB",
            //"50MB",
            "100MB",
        ];
    }
}
