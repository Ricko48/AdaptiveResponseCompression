using AdaptiveResponseCompression.Client.Extensions;
using AdaptiveResponseCompression.Client.HttpClients.Interfaces;
using AdaptiveResponseCompression.Client.Services.Interfaces;
using AdaptiveResponseCompression.Common.Enums;
using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace EvaluationScripts;

public class AdaptiveResponseCompressionEvaluation
{
    private readonly IAdaptiveCompressionClient _adaptiveCompressionClient;
    private readonly IBandwidthService _bandwidthService;
    private readonly TimeSpan _timeout = TimeSpan.FromMinutes(30);
    private const string FileName = "adaptive_response_compression_evaluation.xlsx";

    public AdaptiveResponseCompressionEvaluation(
        IAdaptiveCompressionClient adaptiveCompressionClient,
        IBandwidthService bandwidthService)
    {
        _adaptiveCompressionClient = adaptiveCompressionClient;
        _bandwidthService = bandwidthService;
    }

    public static async Task RunAsync()
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(Config.NumberOfRequests, 0, nameof(Config.NumberOfRequests));
        var evaluation = Build();
        await evaluation.RunInternalAsync();
    }

    private async Task RunInternalAsync()
    {
        Console.WriteLine("--ADAPTIVE RESPONSE COMPRESSION EVALUATION--");

        Console.WriteLine("Estimating download bandwidth");

        var downloadBandwidth = await _bandwidthService.UpdateBandwidthForHostAsync(Config.Host);
        var bandwidthInKBps = EvaluationHelper.ConvertToKBps(downloadBandwidth);

        Console.WriteLine($"Estimated bandwidth: {bandwidthInKBps} KBps");

        Console.WriteLine("Triggering metrics computation for each content type and size combination " +
                          "(this may take a while, especially in case of low download bandwidth)");

        await TriggerMetricsComputationAsync();

        Console.WriteLine("Waiting for the metrics to be computed (60 seconds)");

        await Task.Delay(60000);

        if (!Directory.Exists(Config.FolderPath))
        {
            Directory.CreateDirectory(Config.FolderPath);
        }

        var filePath = Path.Combine(Config.FolderPath, FileName);

        using var workbook = File.Exists(filePath) ? new XLWorkbook(filePath) : new XLWorkbook();
        var worksheet = workbook.AddWorksheet($"Bandwidth - {bandwidthInKBps} KBps");

        workbook.SaveAs(filePath);

        await EvaluateAsync(worksheet, workbook);
    }

    private async Task EvaluateAsync(IXLWorksheet worksheet, XLWorkbook workbook)
    {
        const char column = 'B';
        worksheet.Cell($"{column}1").Value = "Json";

        for (var i = 0; i < Config.FileSizes.Length; i++)
        {
            var fileSize = Config.FileSizes[i];
            var averageLatency = await EvaluateForDataSizeAsync(fileSize);

            worksheet.Cell($"A{i + 2}").Value = fileSize;
            worksheet.Cell($"{column}{i + 2}").Value = averageLatency;

            workbook.Save();
        }
    }

    private async Task<double> EvaluateForDataSizeAsync(string fileSize)
    {
        var latencies = new List<double>();

        for (var j = 0; j < Config.NumberOfRequests; j++)
        {
            Console.WriteLine($"{j + 1}. Downloading file - host: {Config.Host} - type: Json - size: {fileSize}");
            var latency = await MeasureLatencyForDataSizeAsync(fileSize);
            latencies.Add(latency);
        }

        var averageLatency = latencies.Average();
        return EvaluationHelper.TruncateToTwoDecimals(averageLatency);
    }

    private async Task<double> MeasureLatencyForDataSizeAsync(string fileSize)
    {
        using var request = EvaluationHelper.GetRequestMessage(fileSize);
        var stopwatch = Stopwatch.StartNew();
        using var response = await _adaptiveCompressionClient.SendAsync(request, _timeout);
        stopwatch.Stop();
        response.EnsureSuccessStatusCode();
        return stopwatch.Elapsed.TotalMilliseconds;
    }

    private async Task TriggerMetricsComputationAsync() // ensures that metrics are computed for the evaluation
    {
        foreach (var fileSize in Config.FileSizes)
        {
            using var request = EvaluationHelper.GetRequestMessage(fileSize);
            using var response = await _adaptiveCompressionClient.SendAsync(request, _timeout);
            response.EnsureSuccessStatusCode();
            await Task.Delay(3000);
        }
    }

    private static AdaptiveResponseCompressionEvaluation Build()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<AdaptiveResponseCompressionEvaluation>();

        serviceCollection.AddAdaptiveResponseCompressionClient(options =>
        {
            // ToDo you can set up adaptive response compression here
            options.BandwidthAccuracy = BandwidthAccuracy.Highest;
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        return serviceProvider.GetRequiredService<AdaptiveResponseCompressionEvaluation>();
    }
}