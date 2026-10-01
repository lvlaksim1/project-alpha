namespace Baraban.Models;

public sealed class SessionProfile
{
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Dictionary<string, string>> RequestHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<StoredCookie> Cookies { get; set; } = [];
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
    public string SameSite { get; set; } = "Lax";

    public bool IsExpired(DateTimeOffset now) => ExpiresUtc is { } expires && expires <= now;
}
