using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.RegularExpressions;

namespace AsyncWebPageDownloader;

internal static partial class Cli
{
    private static readonly Regex HttpUrlRegex = GetHttpUrlRegex();
    
    private static readonly Argument<string[]> UrlsArgument = new("URLs")
    {
        Description = "Web page URLs to download. Example: https://www.google.com",
        Validators = { ValidateUrls },
        Arity = ArgumentArity.ZeroOrMore,
    };
    
    private static readonly Option<FileInfo?> FileOption = new("--file")
    {
        Description = "Path to a text file containing URLs to download",
        Validators = { ValidateFile },
        Arity = ArgumentArity.ZeroOrOne
    };
    
    private static readonly Option<DirectoryInfo> DestinationFolderOption = new("--destination-folder")
    {
        Description = "Path to a folder where web pages will be saved",
        DefaultValueFactory = _ => new DirectoryInfo(Directory.GetCurrentDirectory()),
        
        Arity = ArgumentArity.ZeroOrOne
    };
    
    private static readonly RootCommand RootCommand = new("AsyncWebPageDownloader is a command line tool to download multiple web pages asynchronously.")
    {
        Arguments =
        {
            UrlsArgument
        },
        Options =
        {
            FileOption,
            DestinationFolderOption
        }
    };
    
    public static CliParseResult ParseArguments(string[] args, InvocationConfiguration? configuration = null)
    {
        var parseResult = RootCommand.Parse(args);
        var status = parseResult.Invoke(configuration);
        var destinationFolder = parseResult.GetValue(DestinationFolderOption)!;
        
        if (status != 0) return new CliParseResult(IsSuccessful: false, [], destinationFolder);
        
        var parsedUrls = GetParsedUrls(parseResult);
        return new CliParseResult(IsSuccessful: true, parsedUrls, destinationFolder);
    }

    private static void ValidateFile(OptionResult result)
    {
        var fileInfo = result.GetValueOrDefault<FileInfo?>();
        if (fileInfo is null)
        {
            return;
        }
        
        if (!fileInfo.Exists)
        {
            result.AddError($"File not found: {fileInfo.FullName}");
            return;
        }

        foreach (var url in File.ReadLines(fileInfo.FullName))
        {
            if (!IsUrlValid(url))
            {
                result.AddError($"Invalid URL in file: {url}");
            }
        }
    }

    private static void ValidateUrls(ArgumentResult result)
    {
        var urls = result.GetValueOrDefault<string[]>();
        if (urls is null or [])
        {
            return;
        }
        
        foreach (var url in urls)
        {
            if (!IsUrlValid(url))
            {
                result.AddError($"Invalid URL: {url}");
            }
        }
    }

    private static bool IsUrlValid(string url) =>
        HttpUrlRegex.IsMatch(url) && Uri.TryCreate(url, UriKind.Absolute, out _);
    
    private static IReadOnlyCollection<Uri> GetParsedUrls(ParseResult parseResult)
    {
        var urls = new HashSet<Uri>();

        var urlArguments = parseResult.GetValue(UrlsArgument);
        if (urlArguments is not null)
        {
            AddParsedUrls(urls, urlArguments);
        }

        var urlsFileInfo = parseResult.GetValue(FileOption);
        if (urlsFileInfo is not null)
        {
            AddParsedUrls(urls, File.ReadLines(urlsFileInfo.FullName));
        }

        return urls;
    }

    private static void AddParsedUrls(HashSet<Uri> parsedUrls, IEnumerable<string> urls)
    {
        foreach (var url in urls)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                parsedUrls.Add(uri);
            }
        }
    }

    [GeneratedRegex(@"^(https?://)?([\w-]+(\.[\w-]+)+)(/[\w-./?%&=]*)?$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex GetHttpUrlRegex();
}