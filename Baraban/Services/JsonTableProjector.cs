using System.Data;
using System.Text.Json;

namespace Baraban.Services;

public static class JsonTableProjector
{
    private static readonly JsonSerializerOptions PrettyOptions = new()
    {
        WriteIndented = true
    };

    public static DataTable Build(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new DataTable();

        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind switch
            {
                JsonValueKind.Array => BuildArray(document.RootElement),
                JsonValueKind.Object => BuildObject(document.RootElement),
                _ => BuildScalar(document.RootElement)
            };
        }
        catch (JsonException)
        {
            return new DataTable();
        }
    }

    private static DataTable BuildArray(JsonElement array)
    {
        var items = array.EnumerateArray().ToList();
        if (items.Count == 0)
            return new DataTable();

        if (items.All(x => x.ValueKind == JsonValueKind.Object))
        {
            var table = new DataTable();
            var columns = new List<string>();

            foreach (var item in items)
            {
                foreach (var property in item.EnumerateObject())
                {
                    if (!columns.Contains(property.Name, StringComparer.Ordinal))
                        columns.Add(property.Name);
                }
            }

            foreach (var column in columns)
                table.Columns.Add(column, typeof(string));

            foreach (var item in items)
            {
                var row = table.NewRow();
                foreach (var property in item.EnumerateObject())
                    row[property.Name] = DisplayValue(property.Value);
                table.Rows.Add(row);
            }

            return table;
        }

        var scalarTable = new DataTable();
        scalarTable.Columns.Add("#", typeof(int));
        scalarTable.Columns.Add("Значение", typeof(string));
        var index = 0;
        foreach (var item in items)
            scalarTable.Rows.Add(index++, DisplayValue(item));
        return scalarTable;
    }

    private static DataTable BuildObject(JsonElement obj)
    {
        var table = new DataTable();
        table.Columns.Add("Поле", typeof(string));
        table.Columns.Add("Значение", typeof(string));

        foreach (var property in obj.EnumerateObject())
            table.Rows.Add(property.Name, DisplayValue(property.Value));

        return table;
    }

    private static DataTable BuildScalar(JsonElement value)
    {
        var table = new DataTable();
        table.Columns.Add("Значение", typeof(string));
        table.Rows.Add(DisplayValue(value));
        return table;
    }

    private static string DisplayValue(JsonElement value) =>
        value.ValueKind switch
        {
            JsonValueKind.Object or JsonValueKind.Array => JsonSerializer.Serialize(value, PrettyOptions),
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
            _ => value.GetRawText()
        };
}
