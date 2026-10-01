namespace Baraban.Models;

public sealed class DrumDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string LoginUrl { get; set; } = "";
    public bool Archived { get; set; }
    public Dictionary<string, string> Variables { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<HttpRequestDefinition> Requests { get; set; } = [];
    public ResultMapping? Result { get; set; }

    public override string ToString() => Archived ? $"[АРХИВ] {Name}" : Name;
}

public sealed class HttpRequestDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Method { get; set; } = "GET";
    public string Url { get; set; } = "";
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public string Body { get; set; } = "";
    public Dictionary<string, string> Captures { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public bool AutoRun { get; set; } = true;
    public bool IsConfirmation { get; set; }

    public override string ToString() => Name;
}

public sealed class ResultMapping
{
    public string SourceRequestId { get; set; } = "";
    public string ItemsPath { get; set; } = "$";
    public string DrumIdPath { get; set; } = "offerDrumId";
    public string OfferIdPath { get; set; } = "offerId";
    public List<string> TitlePaths { get; set; } = [];
    public string WinnerVariable { get; set; } = "offerWinId";
    public bool RequireOfferId { get; set; } = true;
}

public sealed record HttpRunResult(
    string RequestId,
    int StatusCode,
    string ReasonPhrase,
    string ResponseBody,
    Dictionary<string, string> ResponseHeaders,
    DateTimeOffset CompletedUtc);
