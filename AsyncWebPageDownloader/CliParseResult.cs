namespace AsyncWebPageDownloader;

internal sealed record CliParseResult(
    bool IsSuccessful, 
    IReadOnlyList<Uri> ParsedUrls,
    DirectoryInfo DestinationFolder);