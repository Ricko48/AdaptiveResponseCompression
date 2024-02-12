using AdaptiveResponseCompression.Server.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace AdaptiveResponseCompression.Server.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseAdaptiveResponseCompressionServer(this WebApplication app)
    {
        app
            .UseMiddleware<BandwidthEstimationMiddleware>()
            .UseMiddleware<AdaptiveResponseCompressionMiddleware>();

        return app;
    }
}
