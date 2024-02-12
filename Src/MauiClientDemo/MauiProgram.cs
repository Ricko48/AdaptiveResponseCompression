using AdaptiveResponseCompression.Client.Extensions;
using AdaptiveResponseCompression.Common.Enums;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MauiClientDemo;

public static class Config
{
    public const string Host = "ApiServerDemo IPAddress/url"; // ToDo configure server address here!
}

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton<MainPage>();

        builder.Services.AddAdaptiveResponseCompressionClient(options =>
        {
            // specify supported encodings, it is best to use 'All' since server is adaptively choosing the most efficient encoding
            options.DecompressionMethods = DecompressionMethods.All;

            // specify accuracy of the bandwidth estimationMethod
            options.BandwidthAccuracy = BandwidthAccuracy.Balanced;

            // Use response's sent timestamp header for latency estimationMethod
            // In case when server's or client's time is not synchronized, this option should not be enabled for accurate latency estimation
            options.UseResponseSentTime = false;
        });

        return builder.Build();
    }
}
