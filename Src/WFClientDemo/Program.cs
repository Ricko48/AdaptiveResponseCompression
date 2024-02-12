using System.Net;
using AdaptiveResponseCompression.Client.Extensions;
using AdaptiveResponseCompression.Common.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WFClientDemo;

public static class Config
{
    public const string Host = "ApiServerDemo IPAddress/url"; // ToDo configure server address here!
}

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static async Task Main()
    {
        await Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                // Adds AdaptiveResponseCompression for the client
                services.AddAdaptiveResponseCompressionClient(options =>
                {
                    // specify supported encodings, it is best to use 'All' since server is adaptively choosing the most efficient encoding
                    options.DecompressionMethods = DecompressionMethods.All;

                    // specify accuracy of the bandwidth estimationMethod
                    options.BandwidthAccuracy = BandwidthAccuracy.Balanced;

                    // Use response's sent timestamp header for latency estimationMethod
                    // In case when server's or client's time is not synchronized, this option should not be enabled for accurate latency estimation
                    options.UseResponseSentTime = false;
                });

                services.AddSingleton<Form1>();
                services.AddHostedService<StartProgram>();

            })
            .Build()
            .RunAsync();
    }
}
