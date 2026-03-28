using System.CommandLine;
using AsyncWebPageDownloader;

namespace AsyncWebPageDownloaderTests;

public class CliTests
{
    [Fact]
    public void WhenArgsContainsValidUrls()
    {
        var args = new[] { "https://www.google.com", "https://www.yahoo.com" };
        
        var cliParseResult = Cli.ParseArguments(args);
        
        Assert.True(cliParseResult.IsSuccessful);
        Assert.Equivalent(new[] { new Uri("https://www.google.com"), new Uri("https://www.yahoo.com") }, cliParseResult.ParsedUrls);
    }
    
    [Fact]
    public async Task WhenArgsContainsInvalidUrl()
    {
        var args = new[] { "https://www.google.com", "//www.yahoo.com" };
        await using var errorStringWriter = new StringWriter();
        
        var cliParseResult = Cli.ParseArguments(args, new InvocationConfiguration()
        {
            Error = errorStringWriter
        });
        
        Assert.False(cliParseResult.IsSuccessful);
        Assert.Equal("Invalid URL: //www.yahoo.com", errorStringWriter.ToString().Trim());
        Assert.Empty(cliParseResult.ParsedUrls);
    }
    
    [Fact]
    public void WhenArgsContainsFileOptionWithValidUrls()
    {
        var args = new[] { "--file", "./TestData/valid_url.txt" };
        
        var cliParseResult = Cli.ParseArguments(args);
        
        Assert.True(cliParseResult.IsSuccessful);
        Assert.Equivalent(new[]
        {
            new Uri("https://www.yahoo.com/"),
            new Uri("https://www.youtube.com/"),
            new Uri("https://www.nosuch.domain.com/"),
            new Uri("https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/Content-Length"),
            new Uri("https://www.amazon.nl/")
        },cliParseResult.ParsedUrls);
    }

    [Fact]
    public async Task WhenArgsContainsFileOptionWithInvalidUrls()
    {
        var args = new[] { "--file", "./TestData/invalid_url.txt" };
        await using var errorStringWriter = new StringWriter();
        
        var cliParseResult = Cli.ParseArguments(args, new InvocationConfiguration()
        {
            Error = errorStringWriter
        });
        
        Assert.False(cliParseResult.IsSuccessful);
        Assert.Equal("Invalid URL in file: notvalid/url", errorStringWriter.ToString().Trim());
        Assert.Empty(cliParseResult.ParsedUrls);
    }

    [Fact]
    public void WhenArgsContainsBothUrlsAndFileOptionWithValidUrls()
    {
        var args = new[] { "http://validurl1.com", "http://validurl2.com", "--file", "./TestData/valid_url.txt" };
        
        var cliParseResult = Cli.ParseArguments(args);
        
        Assert.True(cliParseResult.IsSuccessful);
        Assert.Equivalent(new[]
        {
            new Uri("http://validurl1.com"),
            new Uri("http://validurl2.com"),
            new Uri("https://www.yahoo.com/"),
            new Uri("https://www.youtube.com/"),
            new Uri("https://www.nosuch.domain.com/"),
            new Uri("https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/Content-Length"),
            new Uri("https://www.amazon.nl/")
        },cliParseResult.ParsedUrls);
    }

    [Fact]
    public void WhenArgsIsEmpty()
    {
        var cliParseResult = Cli.ParseArguments([]);
        
        Assert.True(cliParseResult.IsSuccessful);
        Assert.Empty(cliParseResult.ParsedUrls);
    }
}