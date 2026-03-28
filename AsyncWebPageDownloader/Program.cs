using AsyncWebPageDownloader;

var cliParseResult = Cli.ParseArguments(args);
if (!cliParseResult.IsSuccessful) return;

using var httpClient = ResilientHttpClientProvider.GetHttpClient();

var downloadTasks = cliParseResult.ParsedUrls.Select(url => DownloadUrlAsync(url, httpClient)).ToList();

if(cliParseResult.ParsedUrls.Count > 0) Directory.CreateDirectory(cliParseResult.DestinationFolder.FullName);

await foreach (var downloadTask in Task.WhenEach(downloadTasks))
{
    var result = await downloadTask;
    await SaveResultAsync(result, cliParseResult.DestinationFolder);
}

static async Task SaveResultAsync(DownloadResult downloadResult, DirectoryInfo destinationFolder)
{
    var fileName = $"{downloadResult.Url.Host.Replace(".", "_")}_{DateTime.Now:yyyyMMddHHmmssfff}";
    if (downloadResult.Exception is not null)
    {
        await File.WriteAllTextAsync(Path.Combine(destinationFolder.FullName, $"{fileName}_ERR.txt"),
            downloadResult.Exception.Message);
        
        return;
    }

    var httpResponseMessage = downloadResult.HttpResponseMessage!;
    var fileExtension = GetFileExtension(httpResponseMessage);
    var statusCode = ((int)httpResponseMessage.StatusCode).ToString();
    fileName = $"{fileName}_{statusCode}{fileExtension}";
    
    await using var fileStream = File.Create(Path.Combine(destinationFolder.FullName, fileName));
    await httpResponseMessage.Content.CopyToAsync(fileStream);
}

static string GetFileExtension(HttpResponseMessage response)
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

static async Task<DownloadResult> DownloadUrlAsync(Uri url, HttpClient httpClient)
{
    try
    {
        var responseMessage = await httpClient.GetAsync(url);
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
//
//
//
// //
// // Console.WriteLine("All tasks are completed in {0} seconds", stopWatch.Elapsed.TotalSeconds);
// //
// // string GetMessage(HttpResponseMessage responseMessage)
// // {
// //     var elapsedTime = stopWatch.Elapsed.ToString("g");
// //     var messageBuilder = new StringBuilder();
// //     messageBuilder
// //         .AppendLine($"""Fetching "{responseMessage.RequestMessage.RequestUri.ToString()}" is completed""")
// //         .AppendLine($"Elapsed time: {elapsedTime}")
// //         .AppendLine($"Status code: {responseMessage.StatusCode}")
// //         .AppendLine($"Content length: {responseMessage.Content.Headers.ContentLength}")
// //         .AppendLine($"Content type: {responseMessage.Content.Headers.ContentType}");
// //
// //     return messageBuilder.ToString();
// // }
// //
// // string GetErrorMessage(Exception exception)
// // {
// //     var elapsedTime = stopWatch.Elapsed.ToString("g");
// //     var messageBuilder = new StringBuilder();
// //     messageBuilder
// //         .AppendLine($"Error fetching page: {exception.GetType().FullName} - {exception.Message}")
// //         .AppendLine($"Elapsed time: {elapsedTime}");
// //
// //     return messageBuilder.ToString();
// // }

// using Spectre.Console;
//
// AnsiConsole.Progress()
//     .Columns(
//         new TaskDescriptionColumn(),
//         new SpinnerColumn())
//     .Start(ctx =>
//     {
//         var task = ctx.AddTask("Compiling")
//             .IsIndeterminate();
//   
//         while (!ctx.IsFinished)
//         {
//             task.Increment(1);
//             Thread.Sleep(40);
//         }
//
//         if (task.IsFinished) task.Description("Compiling [red]completed![/]");
//     });