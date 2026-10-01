using System.Text.Json;
using Baraban.Models;

namespace Baraban.Services;

public sealed class WorkflowRunner(HttpExecutor executor)
{
    public async Task<IReadOnlyList<HttpRunResult>> RunAsync(
        DrumDefinition drum,
        SessionProfile session,
        Dictionary<string, string> variables,
        CancellationToken cancellationToken = default)
    {
        var results = new List<HttpRunResult>();
        foreach (var request in drum.Requests.Where(x => x.AutoRun && !x.IsConfirmation))
        {
            var result = await executor.SendAsync(request, session, variables, cancellationToken);
            results.Add(result);
            Capture(request, result.ResponseBody, variables);
        }
        return results;
    }

    public static void Capture(HttpRequestDefinition request, string body, IDictionary<string, string> variables)
    {
        if (request.Captures.Count == 0 || string.IsNullOrWhiteSpace(body))
            return;

        try
        {
            using var document = JsonDocument.Parse(body);
            foreach (var capture in request.Captures)
            {
                var value = JsonPath.Scalar(document.RootElement, capture.Value);
                if (value is not null)
                    variables[capture.Key] = value;
            }
        }
        catch (JsonException)
        {
            // Capture only applies to JSON responses.
        }
    }
}
