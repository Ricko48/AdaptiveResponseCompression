using AdaptiveResponseCompression.Server.CompressionProviders;
using AdaptiveResponseCompression.Server.CompressionProviders.Interfaces;
using AdaptiveResponseCompression.Server.Helpers;
using AdaptiveResponseCompression.Server.Helpers.Interfaces;
using AdaptiveResponseCompression.Server.MappingProfiles;
using AdaptiveResponseCompression.Server.Options;
using AdaptiveResponseCompression.Server.Services;
using AdaptiveResponseCompression.Server.Services.Interfaces;
using AdaptiveResponseCompression.Server.Stores;
using AdaptiveResponseCompression.Server.Stores.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AdaptiveResponseCompression.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAdaptiveResponseCompressionServer(this IServiceCollection services)
    {
        services
            .AddSingleton<ICompressionLevelComputer, CompressionLevelComputer>()
            .AddSingleton<IMetricsComputeTaskQueue, MetricsComputeTaskQueue>()
            .AddSingleton<IMetricsComputeTaskQueueHelper, MetricsComputeTaskQueueHelper>()
            .AddHostedService<BackgroundMetricsComputer>()
            .AddSingleton<ICompressionService, CompressionService>()
            .AddSingleton<IMetricsComputeTaskQueue, MetricsComputeTaskQueue>()
            .AddSingleton<IMemoryProfiler, MemoryProfiler>()
            .TryAddSingleton<IAdaptiveResponseCompressionProvider, AdaptiveResponseCompressionProvider>();
        
        // automapper
        services.AddAutoMapper(typeof(CompressionMetricsMappingProfile));

        // registers MetricsStore for two different interfaces
        services
            .AddSingleton<MetricsStore>()
            .AddSingleton<IMetricsStore>(provider => provider.GetRequiredService<MetricsStore>())
            .AddSingleton<IMetricsStoreHelper>(provider => provider.GetRequiredService<MetricsStore>());

        return services;
    }

    public static IServiceCollection AddAdaptiveResponseCompressionServer(this IServiceCollection services, Action<AdaptiveResponseCompressionOptions> configureOptions)
    {
        services
            .AddAdaptiveResponseCompressionServer()
            .Configure(configureOptions);

        return services;
    }
}
