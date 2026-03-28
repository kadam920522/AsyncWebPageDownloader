namespace AsyncWebPageDownloader;

public sealed record CliParseResult(
    bool IsSuccessful, 
    IReadOnlyList<Uri> ParsedUrls,
    DirectoryInfo DestinationFolder);