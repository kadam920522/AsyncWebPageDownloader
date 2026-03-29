using Spectre.Console;
using Spectre.Console.Rendering;

namespace AsyncWebPageDownloader;

internal sealed class SpinnerColumnDecorator : ProgressColumn
{
    private readonly SpinnerColumn _spinnerColumn;
    private readonly IReadOnlyDictionary<int, string> _statusByTaskId;

    public SpinnerColumnDecorator(SpinnerColumn spinnerColumn, IReadOnlyDictionary<int, string> statusByTaskId)
    {
        _spinnerColumn = spinnerColumn;
        _statusByTaskId = statusByTaskId;
    }
    
    public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
    {
        if (task.IsFinished)
        {
            return new Markup(_statusByTaskId[task.Id]);
        }
        
        return _spinnerColumn.Render(options, task, deltaTime);
    }
}