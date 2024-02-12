using ClosedXML.Excel;
using System.Diagnostics;
using System.Net;

namespace EvaluationScripts;

public static class StandardResponseCompressionEvaluation
{
    private static readonly TimeSpan Timeout = TimeSpan.FromHours(30);
    private const string FileName = "standard_response_compression_evaluation.xlsx";

    public static async Task RunAsync()
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(Config.NumberOfRequests, 0, nameof(Config.NumberOfRequests));

        Console.WriteLine($"--STANDARD RESPONSE COMPRESSION EVALUATION - compression level: {Config.CompressionLevel} - bandwidth: {Config.DownloadBandwidthKBps} KBps--");

        if (!Directory.Exists(Config.FolderPath))
        {
            Directory.CreateDirectory(Config.FolderPath);
        }

        var filePath = Path.Combine(Config.FolderPath, FileName);

        using var workbook = File.Exists(filePath) ? new XLWorkbook(filePath) : new XLWorkbook();
        var worksheet = workbook.AddWorksheet($"{Config.DownloadBandwidthKBps} KBps - {Config.CompressionLevel}");
        workbook.SaveAs(filePath);

        for (var i = 0; i < Config.FileSizes.Length; i++)
        {
            worksheet.Cell($"A{i + 3}").Value = Config.FileSizes[i];
        }

        await EvaluateAsync(worksheet, workbook, DecompressionMethods.Brotli, 'B');
        await EvaluateAsync(worksheet, workbook, DecompressionMethods.GZip, 'C');
        await EvaluateAsync(worksheet, workbook, DecompressionMethods.Deflate, 'D');
    }

    private static async Task EvaluateAsync(IXLWorksheet worksheet, XLWorkbook workbook, DecompressionMethods encoding, char column)
    {
        Console.WriteLine($"\nEncoding: {encoding}\n");

        using var handler = new HttpClientHandler();
        handler.AutomaticDecompression = encoding;

        using var httpClient = new HttpClient(handler);
        httpClient.Timeout = Timeout;

        worksheet.Cell($"{column}1").Value = encoding.ToString();
        worksheet.Cell($"{column}2").Value = "Json";

        for (var i = 0; i < Config.FileSizes.Length; i++)
        {
            var fileSize = Config.FileSizes[i];
            var cell = $"{column}{i + 3}";
            await EvaluateForDataSizeAsync(worksheet, httpClient, cell, fileSize);

            workbook.Save();
        }
    }

    private static async Task EvaluateForDataSizeAsync(
        IXLWorksheet worksheet,
        HttpClient httpClient,
        string cell,
        string fileSize)
    {
        var latencies = new List<double>();

        for (var j = 0; j < Config.NumberOfRequests; j++)
        {
            Console.WriteLine($"{j + 1}. Downloading file - host: {Config.Host} - type: json - size: {fileSize}");
            var latency = await MeasureLatencyForDataSizeAsync(httpClient, fileSize);
            latencies.Add(latency);
        }

        var averageTime = latencies.Average();
        worksheet.Cell(cell).Value = EvaluationHelper.TruncateToTwoDecimals(averageTime);
    }

    private static async Task<double> MeasureLatencyForDataSizeAsync(HttpClient httpClient, string fileSize)
    {
        using var request = EvaluationHelper.GetRequestMessage(fileSize);
        var stopwatch = Stopwatch.StartNew();
        using var response = await httpClient.SendAsync(request);
        stopwatch.Stop();
        response.EnsureSuccessStatusCode();
        return stopwatch.Elapsed.TotalMilliseconds;
    }
}