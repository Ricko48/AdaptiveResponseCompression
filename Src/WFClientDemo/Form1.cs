using AdaptiveResponseCompression.Client.HttpClients.Interfaces;
using AdaptiveResponseCompression.Client.Options;
using AdaptiveResponseCompression.Client.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;

namespace WFClientDemo;

public partial class Form1 : Form
{
    private readonly IAdaptiveCompressionClient _adaptiveCompressionClient;
    private readonly IBandwidthService _bandwidthService;
    private readonly AdaptiveCompressionOptions _options;
    private readonly TimeSpan _timeout = TimeSpan.FromMinutes(10);
    private bool _isSendingRequest;
    private readonly HttpClient _client;

    public Form1(
        IAdaptiveCompressionClient adaptiveCompressionClient,
        IBandwidthService bandwidthService,
        IOptions<AdaptiveCompressionOptions> options)
    {
        _adaptiveCompressionClient = adaptiveCompressionClient;
        _bandwidthService = bandwidthService;
        _options = options.Value;

        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All // ToDo you can configure compression algorithm for Standard compression
        };

        _client = new(handler);
        _client.Timeout = _timeout;

        InitializeComponent();

        fileSizeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        fileSizeComboBox.Items.AddRange(TestData.FileSizes);
        fileSizeComboBox.SelectedIndex = 8;
    }

    /// <summary>
    /// Sends http request using adaptive compression.
    /// </summary>
    private async void AdaptiveCompressionButtonClicked(object sender, EventArgs e)
    {
        if (_isSendingRequest)
        {
            MessageBox.Show("Wait for the already running request to finish.");
            return;
        }

        _isSendingRequest = true;

        if (await _bandwidthService.GetBandwidthForHostAsync(Config.Host) == null)
        {
            MessageBox.Show("Adaptive compression will not be triggered because download bandwidth was not estimated yet. Standard compression will be used instead.");
        }

        adaptiveCompressionLabel.Text = "Downloading file...";

        var requestMessage = GetRequestMessage();

        HttpResponseMessage response;

        var stopWatch = Stopwatch.StartNew();
        try
        {
            response = await _adaptiveCompressionClient.SendAsync(requestMessage, _timeout);
            stopWatch.Stop();
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            adaptiveCompressionLabel.Text = "Adaptive compression - Error...";
            MessageBox.Show($@"An error occurred: {ex.Message}");
            return;
        }
        finally
        {
            _isSendingRequest = false;
        }

        adaptiveCompressionLabel.Text =
            $"Adaptive compression response status code: {response.StatusCode} in {stopWatch.ElapsedMilliseconds} ms";

        response.Dispose();
    }

    /// <summary>
    /// Sends http request using standard compression.
    /// </summary>
    private async void StandardCompressionButtonClicked(object sender, EventArgs e)
    {
        if (_isSendingRequest)
        {
            MessageBox.Show("Wait for the already running request to finish.");
            return;
        }

        _isSendingRequest = true;

        standardCompressionLabel.Text = "Downloading file...";

        var requestMessage = GetRequestMessage();

        HttpResponseMessage responseMessage;
        var stopWatch = Stopwatch.StartNew();

        try
        {
            responseMessage = await _client.SendAsync(requestMessage);
            stopWatch.Stop();
            responseMessage.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            standardCompressionLabel.Text = "Standard compression - Error...";
            MessageBox.Show($"An error occurred: {ex.Message}");
            return;
        }
        finally
        {
            _isSendingRequest = false;
        }

        standardCompressionLabel.Text =
            $@"Standard compression response status code: {responseMessage.StatusCode} in {stopWatch.ElapsedMilliseconds} ms";

        responseMessage.Dispose();
    }

    private async void EstimateBandwidthButtonClicked(object sender, EventArgs e)
    {
        if (_isSendingRequest)
        {
            MessageBox.Show("Wait for the already running request to finish.");
            return;
        }

        _isSendingRequest = true;

        bandwidthLabel.Text = $"Sending bandwidth estimation request with '{_options.BandwidthAccuracy}' accuracy...";

        try
        {
            var downloadBandwidth = await _bandwidthService.UpdateBandwidthForHostAsync(Config.Host);
            bandwidthLabel.Text = $"Download bandwidth: {(downloadBandwidth / 1024 * 1000).ToString("0.##")} KB/s";
        }
        catch (Exception ex)
        {
            bandwidthLabel.Text = "Error...";
            MessageBox.Show($"An error occurred while estimating bandwidth: {ex.Message}");
        }
        finally
        {
            _isSendingRequest = false;
        }
    }

    private HttpRequestMessage GetRequestMessage()
    {
        var builder = new UriBuilder(Config.Host)
        {
            Path = $"api/testData/json/{fileSizeComboBox.SelectedItem}"
        };

        var message = new HttpRequestMessage(HttpMethod.Get, builder.Uri);

        // disables cache to get relevant results
        message.Headers.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true,
            NoStore = true,
            MustRevalidate = true
        };

        message.Headers.Pragma.ParseAdd("no-cache");

        return message;
    }
}
