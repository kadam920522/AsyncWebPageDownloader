using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Http.Resilience;
using Polly;

string[] urls = [
    "https://www.yahoo.com/",
    "https://www.youtube.com/",
    "https://www.nosuch.domain.com/",
    "https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/Content-Length",
    "https://www.amazon.nl/",
    "aklfjaa  sls"
];

var retryPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddRetry(new HttpRetryStrategyOptions
    {
        BackoffType = DelayBackoffType.Exponential,
        MaxRetryAttempts = 3
    })
    .Build();

var socketHandler = new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(15)
};

var resilienceHandler = new ResilienceHandler(retryPipeline)
{
    InnerHandler = socketHandler,
};

using var httpClient = new HttpClient(resilienceHandler);

var stopWatch = new Stopwatch();
stopWatch.Start();

List<Task<HttpResponseMessage>> getAsyncTasks = [];
foreach (var url in urls)
{
    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
    {
        Console.WriteLine($"Invalid URL: {url}");
        continue;
    }
    
    getAsyncTasks.Add(httpClient.GetAsync(uri));
}

await foreach (var getAsyncTask in Task.WhenEach(getAsyncTasks))
{
    try
    {
        var response = await getAsyncTask;
        Console.WriteLine(GetMessage(response));
    }
    catch (Exception ex)
    {
        Console.WriteLine(GetErrorMessage(ex));
    }
}

Console.WriteLine("All tasks are completed in {0} seconds", stopWatch.Elapsed.TotalSeconds);

string GetMessage(HttpResponseMessage responseMessage)
{
    var elapsedTime = stopWatch.Elapsed.ToString("g");
    var messageBuilder = new StringBuilder();
    messageBuilder
        .AppendLine($"""Fetching "{responseMessage.RequestMessage.RequestUri.ToString()}" is completed""")
        .AppendLine($"Elapsed time: {elapsedTime}")
        .AppendLine($"Status code: {responseMessage.StatusCode}")
        .AppendLine($"Content length: {responseMessage.Content.Headers.ContentLength}")
        .AppendLine($"Content type: {responseMessage.Content.Headers.ContentType}");

    return messageBuilder.ToString();
}

string GetErrorMessage(Exception exception)
{
    var elapsedTime = stopWatch.Elapsed.ToString("g");
    var messageBuilder = new StringBuilder();
    messageBuilder
        .AppendLine($"Error fetching page: {exception.GetType().FullName} - {exception.Message}")
        .AppendLine($"Elapsed time: {elapsedTime}");

    return messageBuilder.ToString();
}