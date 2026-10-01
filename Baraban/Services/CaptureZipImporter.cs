using System.IO;
using System.IO.Compression;
using System.Text.Json;
using Baraban.Models;

namespace Baraban.Services;

public sealed class CaptureZipImporter(SessionStore store)
{
    public CaptureImportResult Import(string path, SessionProfile session)
    {
        using var archive = ZipFile.OpenRead(path);
        var entry = archive.Entries.FirstOrDefault(x =>
            x.FullName.EndsWith("requests.json", StringComparison.OrdinalIgnoreCase));
        if (entry is null)
            throw new InvalidDataException("В ZIP не найден requests.json.");

        using var stream = entry.Open();
        using var document = JsonDocument.Parse(stream);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException("requests.json должен содержать массив запросов.");

        var profiles = 0;
        var cookiesBefore = session.Cookies.Count;

        foreach (var item in document.RootElement.EnumerateArray())
        {
            if (!item.TryGetProperty("request", out var request) || request.ValueKind != JsonValueKind.Object)
                continue;

            var method = GetString(request, "method");
            var url = GetString(request, "url");
            if (string.IsNullOrWhiteSpace(method) || string.IsNullOrWhiteSpace(url) ||
                !Uri.TryCreate(url, UriKind.Absolute, out var uri))
                continue;

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            ReadHeaders(request, "headers", headers);

            if (item.TryGetProperty("requestExtraInfo", out var extra) && extra.ValueKind == JsonValueKind.Object)
                ReadHeaders(extra, "headers", headers);

            if (headers.TryGetValue("Cookie", out var cookieHeader) && !string.IsNullOrWhiteSpace(cookieHeader))
                ImportCookieHeader(cookieHeader, uri.Host, session);

            var kept = headers
                .Where(x => ShouldImportHeader(x.Key))
                .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

            if (kept.Count == 0)
                continue;

            var key = SessionProfile.BuildRequestKey(method, url);
            session.RequestHeaders[key] = kept;
            profiles++;

            foreach (var pair in kept.Where(x => IsReusableGlobalHeader(x.Key)))
                session.Headers[pair.Key] = pair.Value;
        }

        store.Save(session);
        return new CaptureImportResult(profiles, Math.Max(0, session.Cookies.Count - cookiesBefore));
    }

    private static void ReadHeaders(JsonElement parent, string propertyName, IDictionary<string, string> result)
    {
        if (!parent.TryGetProperty(propertyName, out var headers) || headers.ValueKind != JsonValueKind.Object)
            return;

        foreach (var property in headers.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.String)
                result[property.Name] = property.Value.GetString() ?? "";
        }
    }

    private static string? GetString(JsonElement parent, string propertyName) =>
        parent.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static void ImportCookieHeader(string cookieHeader, string domain, SessionProfile session)
    {
        foreach (var part in cookieHeader.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var index = part.IndexOf('=');
            if (index <= 0)
                continue;

            var name = part[..index].Trim();
            var value = part[(index + 1)..].Trim();
            session.Cookies.RemoveAll(c =>
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                c.Domain.Equals(domain, StringComparison.OrdinalIgnoreCase));
            session.Cookies.Add(new StoredCookie
            {
                Name = name,
                Value = value,
                Domain = domain,
                Path = "/"
            });
        }
    }

    private static bool ShouldImportHeader(string header) =>
        header.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
        || header.Equals("X-CSRF-Token", StringComparison.OrdinalIgnoreCase)
        || header.StartsWith("X-GIB-", StringComparison.OrdinalIgnoreCase)
        || header.Equals("Origin", StringComparison.OrdinalIgnoreCase)
        || header.Equals("Referer", StringComparison.OrdinalIgnoreCase)
        || header.Equals("Accept-Language", StringComparison.OrdinalIgnoreCase)
        || header.Equals("User-Agent", StringComparison.OrdinalIgnoreCase)
        || header.StartsWith("sec-ch-", StringComparison.OrdinalIgnoreCase);

    private static bool IsReusableGlobalHeader(string header) =>
        header.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
        || header.Equals("X-CSRF-Token", StringComparison.OrdinalIgnoreCase)
        || header.Equals("Accept-Language", StringComparison.OrdinalIgnoreCase)
        || header.Equals("User-Agent", StringComparison.OrdinalIgnoreCase);
}

public sealed record CaptureImportResult(int RequestProfiles, int CookiesAdded);
