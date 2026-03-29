namespace AsyncWebPageDownloader;

internal sealed record CliParseResult(
    bool IsSuccessful, 
    IReadOnlyCollection<Uri> ParsedUrls,
    DirectoryInfo DestinationFolder);