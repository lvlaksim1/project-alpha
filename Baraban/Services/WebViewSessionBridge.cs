using System.Text.Json;
using Baraban.Models;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Baraban.Services;

public sealed class WebViewSessionBridge(SessionStore store)
{
    private readonly HashSet<string> _excludedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Cookie", "Host", "Content-Length", "Connection", "Accept-Encoding"
    };

    private string? _storageBootstrapScriptId;

    public async Task InitializeAsync(WebView2 webView, SessionProfile session, Action<string>? observedHeader = null)
    {
        var environment = await CoreWebView2Environment.CreateAsync(null, store.WebViewDataDirectory);
        await webView.EnsureCoreWebView2Async(environment);
        webView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
        webView.CoreWebView2.WebResourceRequested += (_, args) =>
        {
            try
            {
                var observed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var header in args.Request.Headers)
                {
                    if (_excludedHeaders.Contains(header.Key) || !ShouldCapture(header.Key))
                        continue;

                    observed[header.Key] = header.Value;

                    if (IsReusableGlobalHeader(header.Key))
                        session.Headers[header.Key] = header.Value;

                    if (IsReusableHostHeader(header.Key) &&
                        Uri.TryCreate(args.Request.Uri, UriKind.Absolute, out var observedUri))
                    {
                        if (!session.HostHeaders.TryGetValue(observedUri.Host, out var hostHeaders))
                        {
                            hostHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            session.HostHeaders[observedUri.Host] = hostHeaders;
                        }

                        hostHeaders[header.Key] = header.Value;
                    }

                    observedHeader?.Invoke(header.Key);
                }

                if (observed.Count > 0 && ShouldPersistRequestProfile(args.Request.Uri, observed))
                {
                    var key = SessionProfile.BuildRequestKey(args.Request.Method, args.Request.Uri);
                    session.RequestHeaders[key] = observed;
                    store.Save(session);
                }
            }
            catch
            {
            }
        };

        await RestoreSessionToBrowserAsync(webView, session);

        webView.NavigationCompleted += async (_, _) =>
        {
            try
            {
                await ApplyBrowserStorageToCurrentPageAsync(webView, session);
                await SyncCookiesFromBrowserAsync(webView, session);
                observedHeader?.Invoke("Cookies");
            }
            catch
            {
            }
        };
    }

    public async Task RestoreSessionToBrowserAsync(WebView2 webView, SessionProfile session)
    {
        if (webView.CoreWebView2 is null)
            return;

        await RestoreCookiesToBrowserAsync(webView, session);
        await InstallStorageBootstrapAsync(webView, session);
        await ApplyBrowserStorageToCurrentPageAsync(webView, session);
    }

    public async Task SyncCookiesFromBrowserAsync(WebView2 webView, SessionProfile session, string? url = null)
    {
        if (webView.CoreWebView2 is null)
            return;

        var cookies = await webView.CoreWebView2.CookieManager.GetCookiesAsync(url ?? "");
        session.Cookies = cookies.Select(cookie => new StoredCookie
        {
            Name = cookie.Name,
            Value = cookie.Value,
            Domain = cookie.Domain,
            Path = cookie.Path,
            ExpiresUtc = cookie.IsSession
                ? null
                : new DateTimeOffset(DateTime.SpecifyKind(cookie.Expires, DateTimeKind.Utc)),
            IsHttpOnly = cookie.IsHttpOnly,
            IsSecure = cookie.IsSecure,
            SameSite = cookie.SameSite.ToString()
        }).ToList();
        store.Save(session);
    }

    public async Task RestoreCookiesToBrowserAsync(WebView2 webView, SessionProfile session)
    {
        if (webView.CoreWebView2 is null)
            return;

        var manager = webView.CoreWebView2.CookieManager;
        var now = DateTimeOffset.UtcNow;
        foreach (var stored in session.Cookies.Where(c => !c.IsExpired(now)))
        {
            if (string.IsNullOrWhiteSpace(stored.Name) || string.IsNullOrWhiteSpace(stored.Domain))
                continue;

            var cookie = manager.CreateCookie(stored.Name, stored.Value, stored.Domain, string.IsNullOrWhiteSpace(stored.Path) ? "/" : stored.Path);
            cookie.IsHttpOnly = stored.IsHttpOnly;
            cookie.IsSecure = stored.IsSecure;
            if (stored.ExpiresUtc is { } expires)
                cookie.Expires = expires.UtcDateTime;
            if (!string.IsNullOrWhiteSpace(stored.SameSite) &&
                Enum.TryParse<CoreWebView2CookieSameSiteKind>(stored.SameSite, true, out var sameSite))
            {
                cookie.SameSite = sameSite;
            }
            manager.AddOrUpdateCookie(cookie);
        }

        await Task.CompletedTask;
    }

    private async Task InstallStorageBootstrapAsync(WebView2 webView, SessionProfile session)
    {
        if (webView.CoreWebView2 is null)
            return;

        if (!string.IsNullOrWhiteSpace(_storageBootstrapScriptId))
        {
            try { webView.CoreWebView2.RemoveScriptToExecuteOnDocumentCreated(_storageBootstrapScriptId); }
            catch { }
            _storageBootstrapScriptId = null;
        }

        var script = BuildStorageBootstrapScript(session.BrowserStorage);
        if (string.IsNullOrWhiteSpace(script))
            return;

        _storageBootstrapScriptId = await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);
    }

    private static async Task ApplyBrowserStorageToCurrentPageAsync(WebView2 webView, SessionProfile session)
    {
        if (webView.CoreWebView2 is null)
            return;

        var script = BuildStorageBootstrapScript(session.BrowserStorage);
        if (string.IsNullOrWhiteSpace(script))
            return;

        try { await webView.CoreWebView2.ExecuteScriptAsync(script); }
        catch { }
    }

    private static string BuildStorageBootstrapScript(BrowserStorageState? state)
    {
        if (state is null || string.IsNullOrWhiteSpace(state.Url) ||
            !Uri.TryCreate(state.Url, UriKind.Absolute, out var uri))
            return "";

        var originJson = JsonSerializer.Serialize(uri.GetLeftPart(UriPartial.Authority));
        var localJson = JsonSerializer.Serialize(state.LocalStorage);
        var sessionJson = JsonSerializer.Serialize(state.SessionStorage);

        return "(() => {" +
               "try {" +
               "const expectedOrigin=" + originJson + ";" +
               "if (location.origin !== expectedOrigin) return;" +
               "const localValues=" + localJson + ";" +
               "for (const [k,v] of Object.entries(localValues)) localStorage.setItem(k,String(v));" +
               "const sessionValues=" + sessionJson + ";" +
               "for (const [k,v] of Object.entries(sessionValues)) sessionStorage.setItem(k,String(v));" +
               "} catch (_) {}" +
               "})();";
    }

    private static bool ShouldPersistRequestProfile(string url, IReadOnlyDictionary<string, string> headers)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
            uri.AbsolutePath.Contains("/api/", StringComparison.OrdinalIgnoreCase))
            return true;

        return headers.Keys.Any(key =>
            key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ||
            key.Equals("X-CSRF-Token", StringComparison.OrdinalIgnoreCase) ||
            key.StartsWith("X-GIB-", StringComparison.OrdinalIgnoreCase) ||
            key.StartsWith("X-B3-", StringComparison.OrdinalIgnoreCase) ||
            key.Equals("X-Request-ID", StringComparison.OrdinalIgnoreCase));
    }

    private static bool ShouldCapture(string header) =>
        header.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
        || header.Equals("X-CSRF-Token", StringComparison.OrdinalIgnoreCase)
        || header.Equals("X-XSRF-TOKEN", StringComparison.OrdinalIgnoreCase)
        || header.StartsWith("X-GIB-", StringComparison.OrdinalIgnoreCase)
        || header.StartsWith("X-B3-", StringComparison.OrdinalIgnoreCase)
        || header.Equals("X-Request-ID", StringComparison.OrdinalIgnoreCase)
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

    private static bool IsReusableHostHeader(string header) =>
        header.Equals("X-XSRF-TOKEN", StringComparison.OrdinalIgnoreCase);
}
