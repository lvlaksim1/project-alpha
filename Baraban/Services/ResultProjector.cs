using System.Data;
using System.Text.Json;
using Baraban.Models;

namespace Baraban.Services;

public static class ResultProjector
{
    private static readonly string[] DrumIdNames = ["offerDrumId", "drumId", "id"];
    private static readonly string[] OfferIdNames = ["offerId"];
    private static readonly string[] TitleNames =
    [
        "discountName",
        "name",
        "title",
        "discountDescription",
        "description",
        "moreDescription"
    ];

    public static DataTable Build(
        DrumDefinition drum,
        IReadOnlyDictionary<string, HttpRunResult> results,
        IReadOnlyDictionary<string, string> variables)
    {
        var table = CreateTable();
        var mapping = drum.Result;
        if (mapping is null || !results.TryGetValue(mapping.SourceRequestId, out var source))
            return table;

        try
        {
            using var document = JsonDocument.Parse(source.ResponseBody);
            var items = FindPrizeItems(document.RootElement, mapping);
            variables.TryGetValue(mapping.WinnerVariable, out var winner);

            var index = 1;
            foreach (var item in items)
            {
                if (item.ValueKind != JsonValueKind.Object)
                    continue;

                var drumId = FirstValue(
                    item,
                    [mapping.DrumIdPath, .. DrumIdNames]);

                var offerId = FirstValue(
                    item,
                    [mapping.OfferIdPath, .. OfferIdNames]);

                var titleParts = mapping.TitlePaths
                    .Select(path => ScalarByPath(item, path))
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (titleParts.Count == 0)
                {
                    var fallbackTitle = FirstValue(item, TitleNames);
                    if (!string.IsNullOrWhiteSpace(fallbackTitle))
                        titleParts.Add(fallbackTitle);
                }

                var title = string.Join(" — ", titleParts);
                if (string.IsNullOrWhiteSpace(drumId) &&
                    string.IsNullOrWhiteSpace(offerId) &&
                    string.IsNullOrWhiteSpace(title))
                    continue;

                var winnerById = !string.IsNullOrWhiteSpace(winner) &&
                                 !string.IsNullOrWhiteSpace(drumId) &&
                                 string.Equals(drumId, winner, StringComparison.OrdinalIgnoreCase);
                var winnerByFlag = !string.IsNullOrWhiteSpace(mapping.WinnerFlagPath) &&
                                   IsTrue(item, mapping.WinnerFlagPath);
                var isWinner = winnerById || winnerByFlag;

                table.Rows.Add(
                    index++,
                    drumId ?? "",
                    title,
                    offerId ?? "",
                    isWinner ? "★ ТЕКУЩИЙ ПРИЗ" : "",
                    isWinner);
            }
        }
        catch (JsonException)
        {
        }

        return table;
    }

    public static string ResolvePrizeTitle(DataTable? table, string? drumId)
    {
        if (table is null || string.IsNullOrWhiteSpace(drumId))
            return "";

        foreach (DataRow row in table.Rows)
        {
            if (string.Equals(
                    Convert.ToString(row["offerDrumId"]),
                    drumId,
                    StringComparison.OrdinalIgnoreCase))
                return Convert.ToString(row["Приз"]) ?? "";
        }

        return "";
    }

    private static DataTable CreateTable()
    {
        var table = new DataTable();
        table.Columns.Add("#", typeof(int));
        table.Columns.Add("offerDrumId", typeof(string));
        table.Columns.Add("Приз", typeof(string));
        table.Columns.Add("offerId", typeof(string));
        table.Columns.Add("Результат", typeof(string));
        table.Columns.Add("IsWinner", typeof(bool));
        return table;
    }

    private static IReadOnlyList<JsonElement> FindPrizeItems(JsonElement root, ResultMapping mapping)
    {
        var configured = JsonPath.Select(root, mapping.ItemsPath);
        if (configured is { ValueKind: JsonValueKind.Array } configuredArray)
        {
            var configuredItems = configuredArray.EnumerateArray().ToList();
            if (ScoreArray(configuredItems) > 0)
                return configuredItems;
        }

        List<JsonElement>? best = null;
        var bestScore = 0;
        Visit(root, 0);
        return best ?? [];

        void Visit(JsonElement element, int depth)
        {
            if (depth > 7)
                return;

            if (element.ValueKind == JsonValueKind.Array)
            {
                var items = element.EnumerateArray().ToList();
                var score = ScoreArray(items);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = items;
                }

                foreach (var child in items)
                    Visit(child, depth + 1);

                return;
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                    Visit(property.Value, depth + 1);
            }
        }
    }

    private static int ScoreArray(IReadOnlyList<JsonElement> items)
    {
        if (items.Count == 0)
            return 0;

        var score = 0;
        var objectCount = 0;

        foreach (var item in items)
        {
            if (item.ValueKind != JsonValueKind.Object)
                continue;

            objectCount++;
            if (!string.IsNullOrWhiteSpace(FirstValue(item, DrumIdNames)))
                score += 20;
            if (!string.IsNullOrWhiteSpace(FirstValue(item, OfferIdNames)))
                score += 6;
            if (!string.IsNullOrWhiteSpace(FirstValue(item, TitleNames)))
                score += 8;
        }

        if (objectCount == 0)
            return 0;

        return score + objectCount;
    }

    private static string? FirstValue(JsonElement root, IEnumerable<string> candidates)
    {
        foreach (var candidate in candidates.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            var byPath = ScalarByPath(root, candidate);
            if (!string.IsNullOrWhiteSpace(byPath))
                return byPath;

            var leaf = candidate.Split('.').Last();
            var recursive = FindScalarByPropertyName(root, leaf, 0);
            if (!string.IsNullOrWhiteSpace(recursive))
                return recursive;
        }

        return null;
    }

    private static string? ScalarByPath(JsonElement root, string path)
    {
        var value = JsonPath.Scalar(root, path);
        return string.IsNullOrWhiteSpace(value)
            ? null
            : JsonTextFormatter.DecodeJsonEscapesInPlainText(value);
    }

    private static string? FindScalarByPropertyName(JsonElement element, string name, int depth)
    {
        if (depth > 5)
            return null;

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    var scalar = ScalarValue(property.Value);
                    if (!string.IsNullOrWhiteSpace(scalar))
                        return scalar;
                }
            }

            foreach (var property in element.EnumerateObject())
            {
                var nested = FindScalarByPropertyName(property.Value, name, depth + 1);
                if (!string.IsNullOrWhiteSpace(nested))
                    return nested;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in element.EnumerateArray())
            {
                var nested = FindScalarByPropertyName(child, name, depth + 1);
                if (!string.IsNullOrWhiteSpace(nested))
                    return nested;
            }
        }

        return null;
    }

    private static bool IsTrue(JsonElement root, string path)
    {
        var element = JsonPath.Select(root, path);
        if (element is null)
            return false;

        return element.Value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.String => bool.TryParse(element.Value.GetString(), out var value) && value,
            JsonValueKind.Number => element.Value.TryGetInt32(out var number) && number != 0,
            _ => false
        };
    }

    private static string? ScalarValue(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.String => JsonTextFormatter.DecodeJsonEscapesInPlainText(element.GetString() ?? ""),
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null
        };
}
