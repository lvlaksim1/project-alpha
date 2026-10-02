using System.Text.Json;

namespace Baraban.Services;

public static class JsonTextFormatter
{
    private static readonly JsonSerializerOptions PrettyOptions = new()
    {
        WriteIndented = true
    };

    public static string PrettyOrOriginal(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text ?? string.Empty;

        try
        {
            using var document = JsonDocument.Parse(text);
            return JsonSerializer.Serialize(document.RootElement, PrettyOptions);
        }
        catch (JsonException)
        {
            return text;
        }
    }
}
