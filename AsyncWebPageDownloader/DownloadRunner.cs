namespace AsyncWebPageDownloader;

internal static class DownloadRunner
{
    public static async Task RunAsync(IReadOnlyCollection<Uri> urls, DirectoryInfo destinationFolder, DownloadRunnerContext context)
    {
        using var httpClient = ResilientHttpClientProvider.GetHttpClient();
        var downloader = new Downloader(httpClient);
        var resultPersistor = new DownloadResultPersistor(destinationFolder);

        var downloadTasks = new List<Task<DownloadResult>>();
        foreach (var url in urls)
        {
            context.StartDownloadProgress(url);
            
            downloadTasks.Add(downloader.DownloadUrlAsync(url));
        }

        await foreach (var downloadTask in Task.WhenEach(downloadTasks))
        {
            var downloadResult = await downloadTask;
            
            var persistResult = await resultPersistor.PersistAsync(downloadResult);
            
            context.FinishDownloadProgress(downloadResult, persistResult);
        }
    }
}