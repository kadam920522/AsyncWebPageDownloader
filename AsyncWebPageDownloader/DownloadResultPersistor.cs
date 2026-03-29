namespace AsyncWebPageDownloader;

internal sealed class DownloadResultPersistor
{
    private readonly DirectoryInfo _destinationFolder;

    public DownloadResultPersistor(DirectoryInfo destinationFolder)
    {
        _destinationFolder = destinationFolder;
    }

    public async Task<PersistResult> PersistAsync(DownloadResult downloadResult)
    {
        var fileName = $"{downloadResult.Url.Host.Replace(".", "_")}_{DateTime.Now:yyyyMMddHHmmssfff}";
        
        if (downloadResult.Exception is not null)
        {
            fileName = $"{fileName}_ERR.txt";
            
            await File.WriteAllTextAsync(Path.Combine(_destinationFolder.FullName, fileName),
                downloadResult.Exception.Message);

            return new PersistResult(fileName);
        }

        var httpResponseMessage = downloadResult.HttpResponseMessage!;
        var fileExtension = GetFileExtension(httpResponseMessage);
        var statusCode = ((int)httpResponseMessage.StatusCode).ToString();
        fileName = $"{fileName}_{statusCode}{fileExtension}";
    
        await using var fileStream = File.Create(Path.Combine(_destinationFolder.FullName, fileName));
        await httpResponseMessage.Content.CopyToAsync(fileStream);
        
        return new PersistResult(fileName);
    }
    
    private static string GetFileExtension(HttpResponseMessage response)
    {
        var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

        return contentType switch
        {
            "text/html"               => ".html",
            "text/plain"              => ".txt",
            "text/css"                => ".css",
            "text/csv"                => ".csv",
            "text/xml"                => ".xml",
            "application/json"        => ".json",
            "application/xml"         => ".xml",
            "application/pdf"         => ".pdf",
            "application/zip"         => ".zip",
            "image/png"               => ".png",
            "image/jpeg"              => ".jpg",
            "image/gif"               => ".gif",
            "image/svg+xml"           => ".svg",
            "image/webp"              => ".webp",
            _                         => ""
        };
    }
}