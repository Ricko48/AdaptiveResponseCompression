namespace AdaptiveResponseCompression.Client.HttpClients.Interfaces;

internal interface IBandwidthEstimationClient
{
    Task<double> EstimateDownloadBandwidthAsync(string host);
}
