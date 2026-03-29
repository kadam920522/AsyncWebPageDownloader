using AsyncWebPageDownloader;
using Spectre.Console;

var cliParseResult = Cli.ParseArguments(args);
if (!cliParseResult.IsSuccessful) return;
if (cliParseResult.ParsedUrls.Count == 0)
{
    Console.WriteLine("No URLs found");
}

Directory.CreateDirectory(cliParseResult.DestinationFolder.FullName);

var statusByTaskId = new Dictionary<int, string>();

await AnsiConsole.Progress()
    .Columns(
        new TaskDescriptionColumn()
        {
            Alignment = Justify.Left
        },
        new SpinnerColumnDecorator(new SpinnerColumn(), statusByTaskId))
    .StartAsync(async ctx =>
    {
        using var httpClient = ResilientHttpClientProvider.GetHttpClient();
        var downloader = new Downloader(httpClient);

        var progressTasksByUri = new Dictionary<Uri, ProgressTask>();
        var downloadTasks = new List<Task<DownloadResult>>();
        foreach (var url in cliParseResult.ParsedUrls)
        {
            progressTasksByUri[url] = ctx.AddTask($"Downloading [blue]{url}[/]").IsIndeterminate();
            downloadTasks.Add(downloader.DownloadUrlAsync(url));
        }

        var resultPersistor = new DownloadResultPersistor(cliParseResult.DestinationFolder);

        await foreach (var downloadTask in Task.WhenEach(downloadTasks))
        {
            var result = await downloadTask;
            
            var progressTask = progressTasksByUri[result.Url];
            SetStatus(statusByTaskId, progressTask.Id, result);
            progressTask.Description($"Persisting [blue]{result.Url}[/]");
            
            var persistResult = await resultPersistor.PersistAsync(result);
            
            progressTask.Description($"{result.Url} -> {persistResult.FileName}");
            progressTask.Value(100);
        }
    });

static void SetStatus(Dictionary<int, string> statusByTaskId, int taskId, DownloadResult result)
{
    if (result.Exception is not null || !result.HttpResponseMessage!.IsSuccessStatusCode)
    {
        statusByTaskId[taskId] = "[red]Failed![/]";
        return;
    }
    
    statusByTaskId[taskId] = "[green]Success![/]";
}