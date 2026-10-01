using System.Text.Json;

namespace Baraban.Services;

public static class JsonPath
{
    public static JsonElement? Select(JsonElement root, string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || path == "$")
            return root;

        var current = root;
        var normalized = path.Trim();
        if (normalized.StartsWith("$.", StringComparison.Ordinal))
            normalized = normalized[2..];

        foreach (var segment in normalized.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (current.ValueKind == JsonValueKind.Object && current.TryGetProperty(segment, out var property))
            {
                current = property;
                continue;
            }

            if (current.ValueKind == JsonValueKind.Array && int.TryParse(segment.Trim('[', ']'), out var index) && index >= 0 && index < current.GetArrayLength())
            {
                current = current[index];
                continue;
            }

            return null;
        }

        return current;
    }

    public static string? Scalar(JsonElement root, string path)
    {
        var element = Select(root, path);
        if (element is null)
            return null;

        return element.Value.ValueKind switch
        {
            JsonValueKind.String => element.Value.GetString(),
            JsonValueKind.Number => element.Value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => null,
            _ => element.Value.GetRawText()
        };
    }
}
