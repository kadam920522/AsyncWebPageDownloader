using Spectre.Console;

namespace AsyncWebPageDownloader;

internal sealed class DownloadRunnerContext
{
    private readonly Dictionary<Uri, ProgressTask> _progressTaskByUrl = new();
    private readonly ProgressContext _progressContext;
    private readonly Dictionary<int, string> _statusByTaskId;

    public DownloadRunnerContext(ProgressContext progressContext, Dictionary<int, string> statusByTaskId)
    {
        _progressContext = progressContext;
        _statusByTaskId = statusByTaskId;
    }

    public void StartDownloadProgress(Uri url)
    {
        _progressTaskByUrl[url] = _progressContext.AddTask($"Downloading [blue]{url}[/]").IsIndeterminate();
    }
    
    public void FinishDownloadProgress(DownloadResult downloadResult, PersistResult persistResult)
    {
        var progressTask = _progressTaskByUrl[downloadResult.Url];
        
        SetStatus(progressTask.Id, downloadResult);
        progressTask.Description($"{downloadResult.Url} -> {persistResult.FileName}");
        progressTask.Value(100);
    }
    
    private void SetStatus(int taskId, DownloadResult result)
    {
        if (result.Exception is not null || !result.HttpResponseMessage!.IsSuccessStatusCode)
        {
            _statusByTaskId[taskId] = "[red]Failed![/]";
            return;
        }
    
        _statusByTaskId[taskId] = "[green]Success![/]";
    }
}