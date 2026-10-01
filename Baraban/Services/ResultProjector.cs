using System.Data;
using System.Text.Json;
using Baraban.Models;

namespace Baraban.Services;

public static class ResultProjector
{
    public static DataTable Build(DrumDefinition drum, IReadOnlyDictionary<string, HttpRunResult> results, IReadOnlyDictionary<string, string> variables)
    {
        var table = new DataTable();
        table.Columns.Add("#", typeof(int));
        table.Columns.Add("offerDrumId", typeof(string));
        table.Columns.Add("Приз", typeof(string));
        table.Columns.Add("offerId", typeof(string));
        table.Columns.Add("Результат", typeof(string));
        table.Columns.Add("IsWinner", typeof(bool));

        var mapping = drum.Result;
        if (mapping is null || !results.TryGetValue(mapping.SourceRequestId, out var source))
            return table;

        try
        {
            using var document = JsonDocument.Parse(source.ResponseBody);
            var items = JsonPath.Select(document.RootElement, mapping.ItemsPath);
            if (items is null || items.Value.ValueKind != JsonValueKind.Array)
                return table;

            variables.TryGetValue(mapping.WinnerVariable, out var winner);
            var index = 0;
            foreach (var item in items.Value.EnumerateArray())
            {
                var offerId = JsonPath.Scalar(item, mapping.OfferIdPath);
                if (mapping.RequireOfferId && string.IsNullOrWhiteSpace(offerId))
                    continue;

                var drumId = JsonPath.Scalar(item, mapping.DrumIdPath) ?? "";
                var title = string.Join(" — ", mapping.TitlePaths
                    .Select(path => JsonPath.Scalar(item, path))
                    .Where(value => !string.IsNullOrWhiteSpace(value)));
                var isWinner = !string.IsNullOrWhiteSpace(winner) && string.Equals(drumId, winner, StringComparison.OrdinalIgnoreCase);
                table.Rows.Add(index++, drumId, title, offerId ?? "", isWinner ? "★ WINNER" : "", isWinner);
            }
        }
        catch (JsonException)
        {
        }

        return table;
    }
}
