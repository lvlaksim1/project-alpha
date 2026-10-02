namespace Baraban.Models;

public sealed class SessionProfile
{
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Dictionary<string, string>> HostHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Dictionary<string, string>> RequestHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<StoredCookie> Cookies { get; set; } = [];
    public BrowserStorageState? BrowserStorage { get; set; }
    public CaptureSourceInfo? CaptureSource { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; } = DateTimeOffset.UtcNow;

    public static string BuildRequestKey(string method, string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return $"{method.Trim().ToUpperInvariant()} {url.Trim()}";

        return $"{method.Trim().ToUpperInvariant()} {uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
    }
}

public sealed class StoredCookie
{
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
    public string Domain { get; set; } = "";
    public string Path { get; set; } = "/";
    public DateTimeOffset? ExpiresUtc { get; set; }
    public bool IsHttpOnly { get; set; }
    public bool IsSecure { get; set; }
    public string SameSite { get; set; } = "";

    public bool IsExpired(DateTimeOffset now) => ExpiresUtc is { } expires && expires <= now;
}

public sealed class BrowserStorageState
{
    public string Url { get; set; } = "";
    public DateTimeOffset? CapturedAt { get; set; }
    public Dictionary<string, string> LocalStorage { get; set; } = new(StringComparer.Ordinal);
    public Dictionary<string, string> SessionStorage { get; set; } = new(StringComparer.Ordinal);
}

public sealed class CaptureSourceInfo
{
    public string Format { get; set; } = "";
    public int FormatVersion { get; set; }
    public string ExtensionVersion { get; set; } = "";
    public string StartUrl { get; set; } = "";
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public DateTimeOffset ImportedUtc { get; set; } = DateTimeOffset.UtcNow;
}
