using System.Net;
using System.Net.Http;
using System.Text;
using Baraban.Models;

namespace Baraban.Services;

public sealed class HttpExecutor
{
    public async Task<HttpRunResult> SendAsync(
        HttpRequestDefinition definition,
        SessionProfile session,
        IReadOnlyDictionary<string, string> variables,
        CancellationToken cancellationToken = default)
    {
        using var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            UseCookies = false,
            AllowAutoRedirect = true
        };
        using var client = new HttpClient(handler);

        var method = new HttpMethod(TemplateResolver.Resolve(definition.Method, variables));
        var url = TemplateResolver.Resolve(definition.Url, variables);
        using var request = new HttpRequestMessage(method, url);

        foreach (var pair in session.Headers)
            TryAddHeader(request, pair.Key, TemplateResolver.Resolve(pair.Value, variables));

        var requestKey = SessionProfile.BuildRequestKey(method.Method, url);
        if (session.RequestHeaders.TryGetValue(requestKey, out var requestHeaders))
        {
            foreach (var pair in requestHeaders)
                TryAddHeader(request, pair.Key, TemplateResolver.Resolve(pair.Value, variables));
        }

        foreach (var pair in definition.Headers.Where(x => !x.Key.Equals("Cookie", StringComparison.OrdinalIgnoreCase)))
            TryAddHeader(request, pair.Key, TemplateResolver.Resolve(pair.Value, variables));

        var cookieHeader = definition.Headers.TryGetValue("Cookie", out var cookieOverride) && !string.IsNullOrWhiteSpace(cookieOverride)
            ? TemplateResolver.Resolve(cookieOverride, variables)
            : BuildCookieHeader(session, new Uri(url));
        if (!string.IsNullOrWhiteSpace(cookieHeader))
        {
            request.Headers.Remove("Cookie");
            request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
        }

        var body = TemplateResolver.Resolve(definition.Body, variables);
        if (!string.IsNullOrWhiteSpace(body) && method != HttpMethod.Get && method != HttpMethod.Head)
        {
            var mediaType = "application/json";
            if (definition.Headers.TryGetValue("Content-Type", out var definedContentType) && !string.IsNullOrWhiteSpace(definedContentType))
                mediaType = TemplateResolver.Resolve(definedContentType, variables);
            request.Content = new StringContent(body, Encoding.UTF8, mediaType.Split(';')[0]);
        }

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var headers = response.Headers.Concat(response.Content.Headers)
            .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => string.Join(", ", g.SelectMany(x => x.Value)), StringComparer.OrdinalIgnoreCase);

        return new HttpRunResult(
            definition.Id,
            (int)response.StatusCode,
            response.ReasonPhrase ?? "",
            responseBody,
            headers,
            DateTimeOffset.UtcNow);
    }

    private static string BuildCookieHeader(SessionProfile session, Uri uri)
    {
        var now = DateTimeOffset.UtcNow;
        return string.Join("; ", session.Cookies
            .Where(c => !c.IsExpired(now))
            .Where(c => DomainMatches(uri.Host, c.Domain))
            .Where(c => uri.AbsolutePath.StartsWith(string.IsNullOrWhiteSpace(c.Path) ? "/" : c.Path, StringComparison.Ordinal))
            .Select(c => $"{c.Name}={c.Value}"));
    }

    private static bool DomainMatches(string host, string cookieDomain)
    {
        var domain = (cookieDomain ?? "").TrimStart('.');
        return host.Equals(domain, StringComparison.OrdinalIgnoreCase)
            || host.EndsWith('.' + domain, StringComparison.OrdinalIgnoreCase);
    }

    private static void TryAddHeader(HttpRequestMessage request, string key, string value)
    {
        if (key.Equals("Cookie", StringComparison.OrdinalIgnoreCase) ||
            key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) ||
            key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
            key.Equals("Connection", StringComparison.OrdinalIgnoreCase) ||
            key.Equals("Accept-Encoding", StringComparison.OrdinalIgnoreCase) ||
            key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            return;

        request.Headers.Remove(key);
        request.Headers.TryAddWithoutValidation(key, value);
    }
}
