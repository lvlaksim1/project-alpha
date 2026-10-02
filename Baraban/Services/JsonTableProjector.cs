using System.Data;
using System.Text.Json;

namespace Baraban.Services;

public static class JsonTableProjector
{
    public static DataTable Build(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new DataTable();

        try
        {
            using var document = JsonDocument.Parse(json);
            var candidate = FindBestTableElement(document.RootElement);
            return candidate.ValueKind switch
            {
                JsonValueKind.Array => BuildArray(candidate),
                JsonValueKind.Object => BuildObject(candidate),
                _ => BuildScalar(candidate)
            };
        }
        catch (JsonException)
        {
            return new DataTable();
        }
    }

    private static JsonElement FindBestTableElement(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
            return root;

        if (root.ValueKind != JsonValueKind.Object)
            return root;

        JsonElement? best = null;
        var bestScore = -1;
        Visit(root, 0);
        return best ?? root;

        void Visit(JsonElement element, int depth)
        {
            if (depth > 6)
                return;

            if (element.ValueKind == JsonValueKind.Array)
            {
                var count = element.GetArrayLength();
                if (count > 0)
                {
                    var objectCount = element.EnumerateArray().Count(x => x.ValueKind == JsonValueKind.Object);
                    var score = objectCount * 10 + count;
                    if (score > bestScore)
                    {
                        best = element;
                        bestScore = score;
                    }
                }

                foreach (var child in element.EnumerateArray())
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

    private static DataTable BuildArray(JsonElement array)
    {
        var items = array.EnumerateArray().ToList();
        if (items.Count == 0)
            return new DataTable();

        if (items.All(x => x.ValueKind == JsonValueKind.Object))
        {
            var flattened = items.Select(FlattenObject).ToList();
            var columns = flattened
                .SelectMany(x => x.Keys)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => ColumnPriority(x))
                .ThenBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var table = new DataTable();
            foreach (var column in columns)
                table.Columns.Add(column, typeof(string));

            foreach (var item in flattened)
            {
                var row = table.NewRow();
                foreach (var pair in item)
                    row[pair.Key] = pair.Value;
                table.Rows.Add(row);
            }

            return table;
        }

        var scalarTable = new DataTable();
        scalarTable.Columns.Add("#", typeof(int));
        scalarTable.Columns.Add("Значение", typeof(string));
        var index = 1;
        foreach (var item in items)
            scalarTable.Rows.Add(index++, DisplayValue(item));
        return scalarTable;
    }

    private static DataTable BuildObject(JsonElement obj)
    {
        var flattened = FlattenObject(obj);
        var table = new DataTable();
        table.Columns.Add("Поле", typeof(string));
        table.Columns.Add("Значение", typeof(string));

        foreach (var pair in flattened)
            table.Rows.Add(pair.Key, pair.Value);

        return table;
    }

    private static DataTable BuildScalar(JsonElement value)
    {
        var table = new DataTable();
        table.Columns.Add("Значение", typeof(string));
        table.Rows.Add(DisplayValue(value));
        return table;
    }

    private static Dictionary<string, string> FlattenObject(JsonElement obj)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        Flatten(obj, "", values, 0);
        return values;
    }

    private static void Flatten(JsonElement element, string prefix, IDictionary<string, string> output, int depth)
    {
        if (depth > 4)
        {
            if (!string.IsNullOrWhiteSpace(prefix))
                output[prefix] = DisplayValue(element);
            return;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var path = string.IsNullOrWhiteSpace(prefix) ? property.Name : $"{prefix}.{property.Name}";
                Flatten(property.Value, path, output, depth + 1);
            }
            return;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            if (!string.IsNullOrWhiteSpace(prefix))
                output[prefix] = DisplayValue(element);
            return;
        }

        if (!string.IsNullOrWhiteSpace(prefix))
            output[prefix] = DisplayValue(element);
    }

    private static int ColumnPriority(string name)
    {
        var leaf = name.Split('.').Last();
        return leaf.ToLowerInvariant() switch
        {
            "offerdrumid" => 0,
            "offerid" => 1,
            "name" => 2,
            "title" => 3,
            "discountname" => 4,
            "description" => 5,
            _ => 10
        };
    }

    private static string DisplayValue(JsonElement value) =>
        value.ValueKind switch
        {
            JsonValueKind.Object or JsonValueKind.Array => JsonSerializer.Serialize(value, JsonTextFormatter.PrettyOptions),
            JsonValueKind.String => JsonTextFormatter.DecodeJsonEscapesInPlainText(value.GetString() ?? string.Empty),
            JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
            _ => value.GetRawText()
        };
}
