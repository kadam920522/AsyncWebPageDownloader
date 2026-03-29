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
    .StartAsync(ctx => DownloadRunner.RunAsync(
        cliParseResult.ParsedUrls, 
        cliParseResult.DestinationFolder, 
        new DownloadRunnerContext(ctx, statusByTaskId)));