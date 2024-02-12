using AdaptiveResponseCompression.Server.Options;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using AdaptiveResponseCompression.Server.CompressionProviders;
using AdaptiveResponseCompression.Server.Extensions;
using ApiServerDemo;
using ApiServerDemo.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configure();

// Never use both Adaptive and Standard response compression at the same time!

// ---BEGIN ADAPTIVE RESPONSE COMPRESSION CONFIGURATION---
builder.Services.AddAdaptiveResponseCompressionServer(options =>
{
    // enables compression for HTTPS requests
    // but represents vulnerability see https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression?view=aspnetcore-8.0#compression-with-https
    // this options is turned off by default
    options.EnableForHttps = true;

    // in case of bigger bandwidth than specified value, standard compression will be used for better performance
    // default value is 122 kilobytes
    options.AdaptiveCompressionMaxBandwidth = 50000; // KB/s

    // it is possible to add specific compression providers
    // the order of the providers reflects their priority in case of Standard compression when client supports multiple compression algorithms
    // when not specified, the Brotli, Gzip and Deflate compression providers are used (in this order)
    // in case of Adaptive compression, the order of the providers is not important (the most efficient will be chosen)
    options.Providers.Add<AdaptiveBrotliCompressionProvider>();
    options.Providers.Add<AdaptiveGzipCompressionProvider>();
    options.Providers.Add<AdaptiveDeflateCompressionProvider>();

    // it is possible to specify the allowed compression levels for Adaptive compression
    // when not specified, all levels are allowed (recommended for the most efficient Adaptive compression)
    options.AdaptiveCompressionLevels =
    [
        CompressionLevel.Fastest,
        CompressionLevel.Optimal,
        CompressionLevel.NoCompression,
        CompressionLevel.SmallestSize
    ];

    // it is possible to specify allowed MIME types to be compressed
    // Default configurations contains following types:
    options.MimeTypes =
    [
        "text/plain",
        "application/json",
        "text/css",
        "application/javascript",
        "text/javascript",
        "text/html",
        "application/xml",
        "text/xml",
        "text/json",
        "application/wasm"
    ];

    // it is possible to specify not allowed MIME types to be compressed
    options.ExcludedMimeTypes = ["image/png", "image/gif"];

    // sets the max memory usage in percentage for which will be adaptive compression triggered
    // default value is 100%
    options.AdaptiveCompressionMaxMemory = 80; // percentage
});

// it is possible to configure default compression level for adaptive compression providers
builder.Services.Configure<BrotliAdaptiveCompressionOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.Configure<GzipAdaptiveCompressionOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.Configure<DeflateAdaptiveCompressionOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
// ---END ADAPTIVE RESPONSE COMPRESSION CONFIGURATION---

// ---BEGIN STANDARD RESPONSE COMPRESSION CONFIGURATION---
//builder.Services.AddResponseCompression(options =>
//{
//    options.EnableForHttps = true;
//    options.Providers.Add<BrotliCompressionProvider>();
//    options.Providers.Add<GzipCompressionProvider>();
//    options.Providers.Add<DeflateCompressionProvider>();
//});

//// it is possible to configure compression level for Standard compression providers
//builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
//{
//    options.Level = CompressionLevel.Fastest;
//});
//builder.Services.Configure<GzipCompressionProviderOptions>(options =>
//{
//    options.Level = CompressionLevel.Fastest;
//});
//builder.Services.Configure<DeflateCompressionProviderOptions>(options =>
//{
//    options.Level = CompressionLevel.Fastest;
//});
// ---END STANDARD RESPONSE COMPRESSION CONFIGURATION---

var app = builder.Build();
app.Configure();

// ---BEGIN ADAPTIVE RESPONSE COMPRESSION CONFIGURATION---
app.UseAdaptiveResponseCompressionServer();
// ---END ADAPTIVE RESPONSE COMPRESSION CONFIGURATION---

// ---BEGIN STANDARD RESPONSE COMPRESSION CONFIGURATION---
//app.UseResponseCompression();
// ---END STANDARD RESPONSE COMPRESSION CONFIGURATION---

app.Run();
