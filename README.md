# Adaptive Response Compression

This library was designed and developed within the diploma thesis:

**ONDREJKA, Richard. Application Layer Adaptive Data Compression. Online. Master's thesis. Brno: Masaryk University, Faculty of Informatics. Available from: https://is.muni.cz/th/yt5s8/.**


This library offers an adaptive approach for response compression in [ASP.NET framework](https://dotnet.microsoft.com/en-us/apps/aspnet). It is built on top of the official [Response Compression](https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression) in .NET8. The adaptivity works in the way that it always chooses the most optimal combination of the compression level and algorithm for each response based on the client's bandwidth, response content type, and computational power of the server to lower the response latency as much as possible.

To facilitate the client's download bandwidth estimation, the library consists of the server-side as well as the client-side library.

## AdaptiveResponseCompression.Client

### Registration

Example of registering client-side library into the Dependency Injection:

```
var services = new ServiceCollection();

// registers services
services.AddAdaptiveResponseCompressionClient();

serviceProvider = services.BuildServiceProvider();
```

### Sending the HTTP(s) request

To trigger adaptive response compression on the server, the client must send HTTP/HTTPS requests using a specialized HTTP(s) client interface. This thread-safe client can be resolved from the dependency injection by the interface `IAdaptiveCompressionClient`. It offers the method `SendAsync` which sends the HTTP(s) request to the server which supports adaptive response compression. This client includes in the request headers the current client's download bandwidth for the requested server. If the server does not support adaptive response compression, the client acts as an [HttpClient](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient).

```
public class Demo : IDemo
{
    private readonly IAdaptiveCompressionClient _client;

    public Demo(IAdaptiveCompressionClient client)
    {
        _client = client;
    }

    public async Task TestClientAsync()
    {
        const string url = "server's url";
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var timeout = TimeSpan.FromSeconds(120);
        using var response = await _client.SendAsync(request, timeout);
    }
}
```

### Bandwidth estimation

To trigger adaptive response compression on the server, the client's download bandwidth has to be estimated before sending the requset to the server. The client's download bandwidth ca be estimated by resolving the `IBandwidthService` from the dependency injection and calling method `UpdateBandwidthForHostAsync`.

```
public class Demo : IDemo
{
    private readonly IBandwidthService _service;

    public Demo(IBandwidthService service)
    {
        _service = service;
    }

    public async Task UpdateBandwidthForHostAsync()
    {
        // updates bandwidth for a specific host
        const string host = "server's URL";
        await _service.UpdateBandwidthForHostAsync(host);
    }
}
```

### Bandwidth estimation accuracy

The accuracy of the bandwidth estimation can be set up in the configuration. More accurate estimation lasts longer than the less accurate estimation. Thus the enum `BandwidthAccuracy` was introduced.

```
public enum BandwidthAccuracy
{
    /// <summary>
    /// Estimates bandwidth within approximately up to 5 seconds, providing a quick assessment
    /// with moderate accuracy. Suitable for scenarios where speed is prioritized over precision.
    /// </summary>
    Quick = 5,

    /// <summary>
    /// Balances duration and accuracy by estimating bandwidth within approximately up to 10 seconds,
    /// offering a good compromise between speed and accuracy.
    /// </summary>
    Balanced = 10,

    /// <summary>
    /// Provides a more precise bandwidth estimate by extending the measurement to approximately
    /// up to 15 seconds, prioritizing accuracy over speed.
    /// </summary>
    High = 15,

    /// <summary>
    /// Delivers the highest precision by measuring for approximately up to 20 seconds, ideal for
    /// scenarios where estimation accuracy is critical.
    /// </summary>
    Highest = 20
}
```

Example of configuring bandwidth estimation accuracy:

```
services.AddAdaptiveResponseCompressionClient(options =>
{
    options.BandwidthAccuracy = BandwidthAccuracy.Balanced;
});
```

DO NOT FORGET THAT BANDWITH IS ESTIMATED ACCURATELY UP TO THE CONFIGURED THRESHOLD `AdaptiveCompressionMaxBandwidth` CONFIGURED ON THE SERVER (more in the [Maximal client's bandwidth](#maximal-clients-bandwidth) section).

### Supported (de)compression algorithms

Supported (de)compression algorithms can be set in the options using [DecompressionMethods](https://learn.microsoft.com/en-us/dotnet/api/system.net.decompressionmethods) enumeration:

```
services.AddAdaptiveResponseCompressionClient(options =>
{
    options.DecompressionMethods = DecompressionMethods.All;
});
```

### Latency measurement

To estimate the client's bandwidth, latency needs to be measured. The library offers two approaches for the latency measurement:

#### 1. Response sent time

The server appends a custom header, named `X-Sent-Timestamp`, to the response. This header contains a UNIX timestamp, measured in milliseconds, indicating the precise time immediately before the response is sent back to the client. The value is then used for the latency computation. However, this method necessitates synchronized time between the client and server, a condition that may not always be achievable.

```
services.AddAdaptiveResponseCompressionClient(options =>
{
    options.UseResponseSentTime = true;
});
```

#### 2. Round trip time

Using the round trip time approach compromises bandwidth measurement accuracy by including the additional transit time of the response in the calculated latency, it provides a viable means for approximating bandwidth in scenarios where time synchronization between the client and server is unattainable.

```
services.AddAdaptiveResponseCompressionClient(options =>
{
    options.UseResponseSentTime = false;
});
```

## AdaptiveResponseCompression.Server

The server-side library offers identical configuration options as existing [Response Compression](https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression) while extending them with additional functionality options.

### Dependency Injection Registration

Example of registering server-side library into Dependency Injection for a simple ASP.NET app:

```
var builder = WebApplication.CreateBuilder();

// registers services
builder.Services.AddAdaptiveResponseCompressionServer(options =>
{
    options.EnableForHttps = true;
});

var app = builder.Build();

// registers middlewares
app.UseAdaptiveResponseCompressionServer();

app.Map("/", () => "Hello world!");

app.Run();
```

### Maximal client's bandwidth

Since adaptive response compression is more efficient than the existing [Response Compression](https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression) mainly in the lower bandwidth environments, the maximal bandwidth threshold can be set up in the configuration. The default value is 122 kilobytes per second (1 megabit per second).

IThe client's download bandwidth will be estimated accurately up to this configured threshold. If the actual bandwidth is much bigger than this threshold, the acccuraccy of the estimation will highly decrease, but the estimated value will be still higher than the configured threshold so the Adaptive Response Compression will not be triggered.

```
builder.Services.AddAdaptiveResponseCompressionServer(options =>
{
    options.AdaptiveCompressionMaxBandwidth = 100; // in kilo-bytes per second
});
```

### Compression levels

It is possible to configure which compression levels can be used for the adaptive response compression. By default, all values of the [CompressionLevel](https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.compressionlevel) enumeration are used.

```
builder.Services.AddAdaptiveResponseCompressionServer(options =>
{
    options.AdaptiveCompressionLevels =
    [
        CompressionLevel.Fastest,
        CompressionLevel.Optimal,
        CompressionLevel.NoCompression,
        CompressionLevel.SmallestSize
    ];
});
```

### Maximal memory

The main disadvantage of adaptive response compression is the high memory usage mainly in the moments of high server load. To prevent using all memory and thus to ensure the responsiveness of the server, the threshold `AdaptiveCompressionMaxMemory` was introduced. This configuration ensures that the standard [Response Compression](https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression) is used instead of the adaptive when memory usage reaches the configured threshold. The default value is equal to 100%.

```
builder.Services.AddAdaptiveResponseCompressionServer(options =>
{
    options.AdaptiveCompressionMaxMemory = 80; // percentage
});
```

### Compression providers

The principle of specifying the compression providers is inspired by the standard [Response Compression](https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression). The library implementation contains defined compression providers for Brotli, GZip, and Deflate compression algorithms. Since the library switches between adaptive and standard response compression, these compression providers are used for both approaches. The order in which are compression providers configured represents their priority when choosing the compression provider in the case of the standard [Response Compression](https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression).

```
builder.Services.AddAdaptiveResponseCompressionServer(options =>
{
    options.Providers.Add<AdaptiveBrotliCompressionProvider>();
    options.Providers.Add<AdaptiveGzipCompressionProvider>();
    options.Providers.Add<AdaptiveDeflateCompressionProvider>();
});
```

To implement a custom compression provider, the interface `IAdaptiveCompressionProvider` must be implemented.

```
public class CustomAdaptiveCompressionProvider : IAdaptiveCompressionProvider
{
    public string EncodingName => "custom";

    public bool SupportsFlush => true;

    // used for the standard compression
    public Stream CreateStream(Stream outputStream)
    {
        // return a compression stream wrapper
    }

    // used for the adaptive compression
    public Stream CreateStream(Stream outputStream, CompressionLevel level)
    {
        // return a compression stream wrapper with passed compression level
    }
}
```

Implemented compression provider can be then configured in the same way as predefined compression providers.

```
builder.Services.AddAdaptiveResponseCompressionServer(options =>
{
    options.Providers.Add<AdaptiveBrotliCompressionProvider>();
    options.Providers.Add<CustomAdaptiveCompressionProvider>();
});
```

### Compression providers configuration

It is possible to configure compression levels for compression providers used in the case of the standard [Response Compression](https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression). By default, all compression providers are configured to use [Fastest compression level](https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.compressionlevel).

```
builder.Services.Configure<BrotliAdaptiveCompressionOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.Configure<GzipAdaptiveCompressionOptions>(options =>
{
    options.Level = CompressionLevel.SmallestSize;
});

builder.Services.Configure<DeflateAdaptiveCompressionOptions>(options =>
{
    options.Level = CompressionLevel.NoCompression;
});
```

### ResponseCompressionAttribute

The library offers `ResponseCompressionAttribute` which server for specifying response compression type for specific endpoint actions. The attributes accepts a parameter of type `ResponseCompressionMethod` which holds information whether, `Adaptive`, `Standard` or `None` compression should be used. If endpoint is not marked with the attribute, the standard response compression is used.

```
[ResponseCompression(ResponseCompressionMethod.Adaptive)]
[HttpGet("endpointPath")]
public IActionResult GetAsync()
{
    // rest of the action method
}
```

```
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
```

# ApiServerDemo

This project represents a demo for the utilization of the server-side adaptive response compression library implemented as [ASP.NET](https://dotnet.microsoft.com/en-us/apps/aspnet) REST API. The `ApiServerDemo` should be tested by running against the `WFClientDemo`, `BlazorWebAssemblyClientDemo` or `MauiClientDemo` applications.

Either Adaptive or Standard response compression should be configured in the `Program.cs` file.
The sample JSON files of different sizes are used for testing the response compression were generated using online tool [Mockaroo](https://www.mockaroo.com/) and stored in the `Src/ApiServerDemo/Data/Json` folder.

# WFClientDemo

This project represents a demo for the utilization of the client-side adaptive response compression library implemented as a Windows Forms application. It offers testing of the bandwidth estimation as well as downloading JSON files of varying sizes with Adaptive and Standard response compression.

The AdaptiveResponseCompression.Client library can be configured in the file `Program.cs`.
The usage of the `IAdaptiveCompressionClient` and `IBandwidthService` is implemented in the file `Form1.cs`.

The `WFClientDemo` must be tested by running against the `ApiServerDemo` project instance. To configure URL for your own running `ApiServerDemo` instance, configure URL address (or IP address + port number) in the `Program.cs` file in the `Config` class.

```
public static class Config
{
    public const string Host = "url";
}
```

[NetLimiter](https://www.netlimiter.com/) can be used for bandwdith throttling either on the client's or server's machine. Be aware of potential inaccuracy of the bandwidth limitters.

It is not recommended to run both `WFClientDemo` and `ApiServerDemo` on the same computer while testing to ensure that bandwidth throttling and bandwidth estimation works correctly.

# MauiClientemo

This project represents a demo for the utilization of the client-side adaptive response compression library implemented as a Maui Android application. It offers testing of the bandwidth estimation as well as downloading JSON files of varying sizes with Adaptive and Standard response compression.

The AdaptiveResponseCompression.Client library can be configured in the file `MauiProgram.cs`.
The usage of the `IAdaptiveCompressionClient` and `IBandwidthService` is implemented in the file `MainPage.xaml.cs`.

The `MauiClientemo` must be tested by running against the `ApiServerDemo` project instance. To configure URL for your own running `ApiServerDemo` instance, configure URL address (or IP address + port number) in the `MauiProgram.cs` file in the `Config` class.

```
public static class Config
{
    public const string Host = "url";
}
```

It is recommended to run both `MauiClientemo` on a real Android device for testing purposes

If you want to throttle bandwidth on the client side for the `MauiClientemo` instance running on Android device, [NetThrottle - Network Tool](https://play.google.com/store/apps/details?id=com.network.speedbooster) can be downloaded and installed from [Google Play](https://play.google.com/store/games).

# BlazorWebAssemblyClientDemo

This project represents a demo for the utilization of the client-side adaptive response compression library implemented as a Blazor Web Assembly running in browser. It offers testing of the bandwidth estimation as well as downloading JSON files of varying sizes with Adaptive and Standard response compression.

The AdaptiveResponseCompression.Client library can be configured in the file `Program.cs`.
The usage of the `IAdaptiveCompressionClient` and `IBandwidthService` is implemented in the file `Pages/Home.razor`.

The `BlazorWebAssemblyClientDemo` must be tested by running against the `ApiServerDemo` project instance. To configure URL for your own running `ApiServerDemo` instance, configure URL address (or IP address + port number) in the `Program.cs` file in the `Config` class.

```
public static class Config
{
    public const string Host = "url";
}
```

It is not recommended to run both `BlazorWebAssemblyClientDemo` and `ApiServerDemo` on the same computer while testing to ensure that bandwidth throttling and bandwidth estimation works correctly.

[NetLimiter](https://www.netlimiter.com/) can be used for bandwdith throttling either on the client's or server's machine. Be aware of potential inaccuracy of the bandwidth limitters. In case of throttling the bandwidth on the client'side, do not forget to throttle bandwidth for the actual browser where your app is running.

For the purposes of testing against the `ApiServerDemo`, CORS policies are configured in the `ApiServerDemo` project. For this reason, do not change application IP addresses in the `launchSettings.json` in the BlazorWebAssemblyClientDemo project.

# EvaluationScripts

This project contains the evaluation scripts for both the standard [Response Compression](https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression) and the adaptive response compression. The evaluation must be running against the running `ApiServerDemo` instance.

It is not recommended to run both `ApiServerDemo` and `EvaluationScripts` on the same computer while testing for getting relevant results.

To get the correct results, it is recommended to run the evaluation scripts as well as the `ApiServerDemo` project in `Release` configuration.

Before running either Adaptive Response Compression evaluation, the `ApiServerDemo` has to be configured with the Adaptive Response Compression. On the other hand, if Standard Response Compression Evaluation is going to be triggered, the Standard Response Compression has to be configured in the `ApiServerDemo`. The Standard Response Compression evaluation should be run for each possible configuration of the compression levels on the server.

[NetLimiter](https://www.netlimiter.com/) can be used for the bandwidth throttling for simulating conditions with lower bandwidth (Be aware of some bandwidth limiter's inaccuracies). Do not forget that Adaptive Respnse Compression is triggered just in case when actual client's bandwdith does not reach a configired threshold for the maximal client's download bandwidth configured in the `ApiServerDemo` project ([Maximal client's bandwidth](#maximal-clients-bandwidth)).

The evaluation can be run by uncommenting either the adaptive or standard response compression evaluation lines in `Program.cs` file.

For the adaptive response compression evaluation:

```
await AdaptiveResponseCompressionEvaluation.RunAsync();
```

For the standard [Response Compression](https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression) evaluation:

```
await StandardResponseCompressionEvaluation.RunAsync();
```

Before running the evaluation, the `Config` class located in the `program.cs` must be filled with all the necessary information for the correct evaluation.

```
public static class Config
{
    /// <summary>
    /// Insert your server's URL here.
    /// </summary>
    public const string ServerUrl = "url";

    /// <summary>
    /// Insert full folder path where report file should be created.
    /// </summary>
    public const string FolderPath = @".";

    /// <summary>
    /// Insert configured compression level for standard compression
    /// (will be used for naming the sheet in a .xlsx file for standard compression).
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
    /// Specify how many requests should be sent for a single combination of the data type and size to compute average RTT.
    /// </summary>
    public const int NumberOfRequests = 3;

    /// <summary>
    /// File sizes used for the evaluation.
    /// You might want to exclude bigger files in case of low bandwidth and standard compression.
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
```
