namespace AsyncWebPageDownloader;

internal sealed class DownloadResult
{
    public required Uri Url { get; init; }
    public HttpResponseMessage? HttpResponseMessage { get; init; }
    public Exception? Exception { get; init; }
}