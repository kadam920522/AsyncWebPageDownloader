namespace AsyncWebPageDownloader;

internal sealed class Downloader
{
    private readonly HttpClient _httpClient;

    public Downloader(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<DownloadResult> DownloadUrlAsync(Uri url)
    {
        try
        {
            var responseMessage = await _httpClient.GetAsync(url);
            return new DownloadResult()
            {
                Url = url,
                HttpResponseMessage = responseMessage
            };
        }
        catch (Exception ex)
        {
            return new DownloadResult()
            {
                Url = url,
                Exception = ex
            };
        }
    }
}