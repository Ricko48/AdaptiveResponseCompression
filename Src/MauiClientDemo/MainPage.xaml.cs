using AdaptiveResponseCompression.Client.HttpClients.Interfaces;
using AdaptiveResponseCompression.Client.Options;
using AdaptiveResponseCompression.Client.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;

namespace MauiClientDemo
{
    public partial class MainPage : ContentPage
    {
        private readonly IBandwidthService _bandwidthService;
        private readonly IAdaptiveCompressionClient _adaptiveClient;
        private readonly HttpClient _client;
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(10);
        private readonly AdaptiveCompressionOptions _options;

        private bool _isSendingRequest;

        public MainPage(
            IBandwidthService bandwidthService,
            IAdaptiveCompressionClient adaptiveClient,
            IOptions<AdaptiveCompressionOptions> options)
        {
            _bandwidthService = bandwidthService;
            _adaptiveClient = adaptiveClient;
            _options = options.Value;

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All // ToDo encoding for standard response compression can be configured here
            };

            _client = new HttpClient(handler);
            _client.Timeout = _timeout;

            InitializeComponent();

            FileSizePicker.Items.Add("1B");
            FileSizePicker.Items.Add("500B");
            FileSizePicker.Items.Add("1KB");
            FileSizePicker.Items.Add("5KB");
            FileSizePicker.Items.Add("10KB");
            FileSizePicker.Items.Add("50KB");
            FileSizePicker.Items.Add("100KB");
            FileSizePicker.Items.Add("500KB");
            FileSizePicker.Items.Add("1MB");
            FileSizePicker.Items.Add("5MB");
            FileSizePicker.Items.Add("10MB");
            FileSizePicker.Items.Add("50MB");
            FileSizePicker.Items.Add("100MB");

            FileSizePicker.SelectedItem = "1MB";
        }

        private async void OnEstimateBandwidthClicked(object sender, EventArgs e)
        {
            if (_isSendingRequest)
            {
                await DisplayAlert("Alert", "Wait for the already running request to finish.", "Ok");
                return;
            }

            _isSendingRequest = true;

            BandwidthLabel.Text = $"Estimating bandwidth using '{_options.BandwidthAccuracy}' accuracy...";

            double bandwidth = 0;

            try
            {
                // running on separate thread to avoid NetworkOnMainThreadException 
                await Task.Run(async () =>
                {
                    bandwidth = await _bandwidthService.UpdateBandwidthForHostAsync(Config.Host);
                });
            }
            catch (Exception ex)
            {
                BandwidthLabel.Text = "Error...";
                await DisplayAlert("Error", ex.InnerException?.Message ?? ex.Message, "Cancel");
                return;
            }
            finally
            {
                _isSendingRequest = false;
            }

            BandwidthLabel.Text = $"{(double)((long)(bandwidth * 100)) / 100} KB/s";
        }

        private async void OnDownloadFileAdaptiveCompressionClicked(object sender, EventArgs e)
        {
            if (_isSendingRequest)
            {
                await DisplayAlert("Alert", "Wait for the already running request to finish.", "Ok");
                return;
            }

            if (await _bandwidthService.GetBandwidthForHostAsync(Config.Host) == null)
            {
                var dialogResponse = await DisplayAlert(
                    "Alert",
                    "Bandwidth was not estimated yet so the Adaptive Response Compression cannot be triggered on the server. Do you want to continue?",
                    "Yes",
                    "No");

                if (!dialogResponse)
                {
                    return;
                }
            }

            _isSendingRequest = true;

            AdaptiveLabel.Text = "Downloading...";

            using var message = GetRequestMessage();

            HttpResponseMessage? response = null;

            var stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();

                // running on separate thread to avoid NetworkOnMainThreadException 
                await Task.Run(async () =>
                {
                    response = await _adaptiveClient.SendAsync(message, _timeout);
                });

                stopwatch.Stop();

                response?.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                AdaptiveLabel.Text = "Error...";
                await DisplayAlert("Error", ex.InnerException?.Message ?? ex.Message, "Cancel");
                return;
            }
            finally
            {
                _isSendingRequest = false;
            }

            AdaptiveLabel.Text = "Adaptive compression latency: " + stopwatch.ElapsedMilliseconds + " ms";

            response?.Dispose();
        }

        private async void OnDownloadFileStandardCompressionClicked(object sender, EventArgs e)
        {
            if (_isSendingRequest)
            {
                await DisplayAlert("Alert", "Wait for the already running request to finish.", "Ok");
                return;
            }

            _isSendingRequest = true;

            StandardLabel.Text = "Downloading...";

            using var message = GetRequestMessage();

            HttpResponseMessage? response = null;

            var stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();

                // running on separate thread to avoid NetworkOnMainThreadException 
                await Task.Run(async () =>
                {
                    response = await _client.SendAsync(message);
                });

                stopwatch.Stop();

                response?.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                StandardLabel.Text = "Error...";
                await DisplayAlert("Error", ex.InnerException?.Message ?? ex.Message, "Cancel");
                return;
            }
            finally
            {
                _isSendingRequest = false;
            }

            StandardLabel.Text = "Standard compression latency: " + stopwatch.ElapsedMilliseconds + " ms";

            response?.Dispose();
        }

        private HttpRequestMessage GetRequestMessage()
        {
            var builder = new UriBuilder(Config.Host)
            {
                Path = $"api/testData/json/{FileSizePicker.SelectedItem}"
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

}
