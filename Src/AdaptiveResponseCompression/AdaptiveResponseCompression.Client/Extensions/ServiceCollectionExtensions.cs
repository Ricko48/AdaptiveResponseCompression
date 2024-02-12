using AdaptiveResponseCompression.Client.Constants;
using AdaptiveResponseCompression.Client.HttpClients;
using AdaptiveResponseCompression.Client.HttpClients.Interfaces;
using AdaptiveResponseCompression.Client.Options;
using AdaptiveResponseCompression.Client.Services;
using AdaptiveResponseCompression.Client.Services.Interfaces;
using AdaptiveResponseCompression.Client.Stores;
using AdaptiveResponseCompression.Client.Stores.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using Blazored.LocalStorage;

namespace AdaptiveResponseCompression.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAdaptiveResponseCompressionClient(this IServiceCollection services)
    {
        return AddAdaptiveResponseCompressionInternal(services, DecompressionMethods.All);
    }

    public static IServiceCollection AddAdaptiveResponseCompressionClient(
        this IServiceCollection services,
        Action<AdaptiveCompressionOptions> configureOptions)
    {
        services.Configure(configureOptions);

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<AdaptiveCompressionOptions>>();

        return AddAdaptiveResponseCompressionInternal(services, options.Value.DecompressionMethods);
    }

    private static IServiceCollection AddAdaptiveResponseCompressionInternal(
        IServiceCollection services,
        DecompressionMethods decompressionMethods)
    {
        // bandwidth estimation http client
        var estimationClientHandler = services
            .AddHttpClient(HttpClientConstants.BandwidthEstimationClient, _ => { });

        // adaptive compression http client
        var adaptiveClientBuilder = services
            .AddHttpClient(HttpClientConstants.AdaptiveClient, _ => { });

        if (OperatingSystem.IsBrowser())
        {
            services
                .AddBlazoredLocalStorage()
                .AddScoped<IBandwidthStore, BrowserBandwidthStore>();
        }
        else
        {
            estimationClientHandler.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.None
            });

            adaptiveClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = decompressionMethods,
            });

            services.AddSingleton<IBandwidthStore, BandwidthStore>();
        }

        services
            .AddScoped<IBandwidthEstimationClient, BandwidthEstimationClient>()
            .AddScoped<IAdaptiveCompressionClient, AdaptiveCompressionClient>()
            .AddScoped<IBandwidthService, BandwidthService>();

        return services;
    }
}
