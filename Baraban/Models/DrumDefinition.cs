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
    public DrumActionMapping? Actions { get; set; }
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
    public string WinnerFlagPath { get; set; } = "";
    public bool RequireOfferId { get; set; } = true;
}

public sealed record HttpRunResult(
    string RequestId,
    int StatusCode,
    string ReasonPhrase,
    string ResponseBody,
    Dictionary<string, string> ResponseHeaders,
    DateTimeOffset CompletedUtc);


public sealed class DrumActionMapping
{
    public List<DrumActionStep> PrizeOptions { get; set; } = [];
    public List<DrumActionStep> State { get; set; } = [];
    public List<DrumActionStep> Claim { get; set; } = [];
    public List<DrumActionStep> Repeat { get; set; } = [];
    public List<DrumActionStep> PaidRepeat { get; set; } = [];
    public string PrizeResultRequestId { get; set; } = "";
    public string StateResultRequestId { get; set; } = "";
    public string WinnerVariable { get; set; } = "offerWinId";
    public bool ClaimRequiresWinner { get; set; } = true;
    public string PrizeOptionsLabel { get; set; } = "Получить варианты призов";
    public string StateLabel { get; set; } = "Состояние барабана";
    public string ClaimLabel { get; set; } = "Получить приз";
    public string ClaimLabelVariable { get; set; } = "";
    public string RepeatTitleVariable { get; set; } = "";
    public string RepeatSubtitleVariable { get; set; } = "";
    public string RepeatNeedPaidVariable { get; set; } = "";
    public string MotivationVariable { get; set; } = "";
    public string PaidRepeatOrderVariable { get; set; } = "";
    public string PaidRepeatSuccessVariable { get; set; } = "";
    public string PaidRepeatPurchasedVariable { get; set; } = "";
    public string TerminalVariable { get; set; } = "";
    public string TerminalEquals { get; set; } = "";
}

public sealed class DrumActionStep
{
    public string RequestId { get; set; } = "";
    public string WhenVariable { get; set; } = "";
    public string WhenEquals { get; set; } = "";
    public bool Optional { get; set; }
}
