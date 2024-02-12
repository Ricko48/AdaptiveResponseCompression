using AdaptiveResponseCompression.Common.Constants;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace ApiServerDemo.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void Configure(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // swagger documentation
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "AdaptiveResponseCompression API", Version = "v1" });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            options.IncludeXmlComments(xmlPath);
        });

        // ToDo Specify your Blazor app URLs for CORS
        const string httpBlazorLocalhost = "http://localhost:5162";
        const string httpsBlazorLocalhost = "https://localhost:7148";

        // CORS configuration for Blazor app
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins",
                corsBuilder =>
                {
                    corsBuilder.WithOrigins(httpBlazorLocalhost, httpsBlazorLocalhost)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders(BandwidthEstimationHeaders.SentTimestamp);
                });
        });
    }
}