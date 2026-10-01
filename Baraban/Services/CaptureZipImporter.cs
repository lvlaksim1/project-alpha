using System.IO;
using System.IO.Compression;
using System.Text.Json;
using Baraban.Models;

namespace Baraban.Services;

public sealed class CaptureZipImporter(SessionStore store)
{
    private const string ExpectedFormat = "browser-session-capture";
    private const int SupportedFormatVersion = 2;

    public CaptureImportResult Import(string path, SessionProfile session)
    {
        using var archive = ZipFile.OpenRead(path);

        var manifestEntry = FindEntry(archive, "session-manifest.json")
            ?? throw new InvalidDataException("ZIP не похож на поддерживаемую запись браузерной сессии: отсутствует session-manifest.json.");

        var manifest = ReadJson(manifestEntry);
        var format = GetString(manifest.RootElement, "format") ?? "";
        var formatVersion = GetInt32(manifest.RootElement, "formatVersion");

        if (!format.Equals(ExpectedFormat, StringComparison.Ordinal))
            throw new InvalidDataException($"Неподдерживаемый формат сессии: '{format}'. Ожидается '{ExpectedFormat}'.");
        if (formatVersion != SupportedFormatVersion)
            throw new InvalidDataException($"Неподдерживаемая версия формата: {formatVersion}. Поддерживается версия {SupportedFormatVersion}.");

        session.Headers.Clear();
        session.RequestHeaders.Clear();
        session.Cookies.Clear();
        session.BrowserStorage = null;
        session.CaptureSource = ParseCaptureSource(manifest.RootElement);

        var structuredCookies = ImportStructuredCookies(archive, session);
        var storage = ImportBrowserStorage(archive, session);
        var requests = ImportRequestProfiles(archive, session, importCookieFallback: structuredCookies == 0);

        store.Save(session);

        return new CaptureImportResult(
            Format: format,
            FormatVersion: formatVersion,
            ExtensionVersion: session.CaptureSource?.ExtensionVersion ?? "",
            RequestsObserved: requests.RequestsObserved,
            RequestProfiles: requests.RequestProfiles,
            CookiesImported: session.Cookies.Count,
            LocalStorageKeys: storage.LocalStorageKeys,
            SessionStorageKeys: storage.SessionStorageKeys,
            RestoreUrl: session.BrowserStorage?.Url ?? session.CaptureSource?.StartUrl ?? "");
    }

    private static CaptureSourceInfo ParseCaptureSource(JsonElement root)
    {
        var result = new CaptureSourceInfo
        {
            Format = GetString(root, "format") ?? "",
            FormatVersion = GetInt32(root, "formatVersion"),
            ImportedUtc = DateTimeOffset.UtcNow
        };

        if (!root.TryGetProperty("session", out var session) || session.ValueKind != JsonValueKind.Object)
            return result;

        result.ExtensionVersion = GetString(session, "extensionVersion") ?? "";
        result.StartUrl = GetString(session, "startUrl") ?? "";
        result.StartTime = GetDateTimeOffset(session, "startTime");
        result.EndTime = GetDateTimeOffset(session, "endTime");
        return result;
    }

    private static int ImportStructuredCookies(ZipArchive archive, SessionProfile session)
    {
        var entry = FindEntry(archive, "browser/end/cookies.json")
            ?? FindEntry(archive, "browser/start/cookies.json");
        if (entry is null)
            return 0;

        using var document = ReadJson(entry);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            return 0;

        foreach (var item in document.RootElement.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
                continue;

            var name = GetString(item, "name");
            var value = GetString(item, "value");
            var domain = GetString(item, "domain");
            if (string.IsNullOrWhiteSpace(name) || value is null || string.IsNullOrWhiteSpace(domain))
                continue;

            var isSession = GetBoolean(item, "session");
            var expires = GetDouble(item, "expires");
            DateTimeOffset? expiresUtc = null;
            if (!isSession && expires is > 0)
            {
                try { expiresUtc = DateTimeOffset.FromUnixTimeSeconds((long)Math.Floor(expires.Value)); }
                catch { expiresUtc = null; }
            }

            session.Cookies.Add(new StoredCookie
            {
                Name = name,
                Value = value,
                Domain = domain,
                Path = GetString(item, "path") ?? "/",
                ExpiresUtc = expiresUtc,
                IsHttpOnly = GetBoolean(item, "httpOnly"),
                IsSecure = GetBoolean(item, "secure"),
                SameSite = GetString(item, "sameSite") ?? ""
            });
        }

        return session.Cookies.Count;
    }

    private static (int LocalStorageKeys, int SessionStorageKeys) ImportBrowserStorage(ZipArchive archive, SessionProfile session)
    {
        var entry = FindEntry(archive, "browser/end/page-snapshot.json")
            ?? FindEntry(archive, "browser/start/page-snapshot.json");
        if (entry is null)
            return (0, 0);

        using var document = ReadJson(entry);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            return (0, 0);

        var state = new BrowserStorageState
        {
            Url = GetString(root, "url") ?? session.CaptureSource?.StartUrl ?? "",
            CapturedAt = GetDateTimeOffset(root, "capturedAt")
        };

        ReadStringMap(root, "localStorage", state.LocalStorage);
        ReadStringMap(root, "sessionStorage", state.SessionStorage);
        session.BrowserStorage = state;
        return (state.LocalStorage.Count, state.SessionStorage.Count);
    }

    private static (int RequestsObserved, int RequestProfiles) ImportRequestProfiles(
        ZipArchive archive,
        SessionProfile session,
        bool importCookieFallback)
    {
        var entry = FindEntry(archive, "requests.json")
            ?? throw new InvalidDataException("В ZIP отсутствует requests.json.");

        using var document = ReadJson(entry);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException("requests.json должен содержать массив запросов.");

        var requestsObserved = 0;
        var profileKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in document.RootElement.EnumerateArray())
        {
            if (!item.TryGetProperty("request", out var request) || request.ValueKind != JsonValueKind.Object)
                continue;

            var method = GetString(request, "method");
            var url = GetString(request, "url");
            if (string.IsNullOrWhiteSpace(method) || string.IsNullOrWhiteSpace(url) ||
                !Uri.TryCreate(url, UriKind.Absolute, out var uri))
                continue;

            requestsObserved++;

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            ReadHeaders(request, "headers", headers);
            if (item.TryGetProperty("requestExtraInfo", out var extra) && extra.ValueKind == JsonValueKind.Object)
                ReadHeaders(extra, "headers", headers);

            if (importCookieFallback &&
                headers.TryGetValue("Cookie", out var cookieHeader) &&
                !string.IsNullOrWhiteSpace(cookieHeader))
            {
                ImportCookieHeader(cookieHeader, uri.Host, session);
            }

            if (!ShouldPersistRequestProfile(uri, headers))
                continue;

            var kept = headers
                .Where(x => ShouldKeepHeader(x.Key))
                .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

            if (kept.Count == 0)
                continue;

            var key = SessionProfile.BuildRequestKey(method, url);
            session.RequestHeaders[key] = kept;
            profileKeys.Add(key);

            foreach (var pair in kept.Where(x => IsReusableGlobalHeader(x.Key)))
                session.Headers[pair.Key] = pair.Value;
        }

        return (requestsObserved, profileKeys.Count);
    }

    private static ZipArchiveEntry? FindEntry(ZipArchive archive, string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/');
        return archive.Entries.FirstOrDefault(entry =>
            entry.FullName.Equals(normalized, StringComparison.OrdinalIgnoreCase) ||
            entry.FullName.EndsWith("/" + normalized, StringComparison.OrdinalIgnoreCase));
    }

    private static JsonDocument ReadJson(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        return JsonDocument.Parse(stream);
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

    private static void ReadStringMap(JsonElement parent, string propertyName, IDictionary<string, string> result)
    {
        if (!parent.TryGetProperty(propertyName, out var map) || map.ValueKind != JsonValueKind.Object)
            return;

        foreach (var property in map.EnumerateObject())
        {
            result[property.Name] = property.Value.ValueKind == JsonValueKind.String
                ? property.Value.GetString() ?? ""
                : property.Value.GetRawText();
        }
    }

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

    private static bool ShouldPersistRequestProfile(Uri uri, IReadOnlyDictionary<string, string> headers) =>
        uri.AbsolutePath.Contains("/api/", StringComparison.OrdinalIgnoreCase)
        || headers.Keys.Any(IsSessionHeader);

    private static bool ShouldKeepHeader(string header) =>
        !header.Equals("Cookie", StringComparison.OrdinalIgnoreCase)
        && !header.Equals("Host", StringComparison.OrdinalIgnoreCase)
        && !header.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)
        && !header.Equals("Connection", StringComparison.OrdinalIgnoreCase)
        && !header.Equals("Accept-Encoding", StringComparison.OrdinalIgnoreCase)
        && !header.StartsWith(":", StringComparison.Ordinal);

    private static bool IsSessionHeader(string header) =>
        header.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
        || header.Equals("X-CSRF-Token", StringComparison.OrdinalIgnoreCase)
        || header.StartsWith("X-GIB-", StringComparison.OrdinalIgnoreCase)
        || header.StartsWith("X-B3-", StringComparison.OrdinalIgnoreCase)
        || header.Equals("X-Request-ID", StringComparison.OrdinalIgnoreCase);

    private static bool IsReusableGlobalHeader(string header) =>
        header.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
        || header.Equals("X-CSRF-Token", StringComparison.OrdinalIgnoreCase)
        || header.Equals("Accept-Language", StringComparison.OrdinalIgnoreCase)
        || header.Equals("User-Agent", StringComparison.OrdinalIgnoreCase);

    private static string? GetString(JsonElement parent, string propertyName) =>
        parent.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static int GetInt32(JsonElement parent, string propertyName) =>
        parent.TryGetProperty(propertyName, out var value) && value.TryGetInt32(out var result)
            ? result
            : 0;

    private static double? GetDouble(JsonElement parent, string propertyName) =>
        parent.TryGetProperty(propertyName, out var value) && value.TryGetDouble(out var result)
            ? result
            : null;

    private static bool GetBoolean(JsonElement parent, string propertyName) =>
        parent.TryGetProperty(propertyName, out var value) &&
        (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False) &&
        value.GetBoolean();

    private static DateTimeOffset? GetDateTimeOffset(JsonElement parent, string propertyName)
    {
        var text = GetString(parent, propertyName);
        return DateTimeOffset.TryParse(text, out var parsed) ? parsed : null;
    }
}

public sealed record CaptureImportResult(
    string Format,
    int FormatVersion,
    string ExtensionVersion,
    int RequestsObserved,
    int RequestProfiles,
    int CookiesImported,
    int LocalStorageKeys,
    int SessionStorageKeys,
    string RestoreUrl);
