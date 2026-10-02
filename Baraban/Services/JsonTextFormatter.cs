using System.Text.Encodings.Web;
using System.Text.Json;

namespace Baraban.Services;

public static class JsonTextFormatter
{
    public static JsonSerializerOptions PrettyOptions { get; } = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
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
            return DecodeJsonEscapesInPlainText(text);
        }
    }

    public static string DecodeJsonEscapesInPlainText(string text)
    {
        if (string.IsNullOrEmpty(text) || !text.Contains("\\u", StringComparison.OrdinalIgnoreCase))
            return text;

        try
        {
            var wrapped = "\"" + text
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\\u", "__UNICODE_ESCAPE__", StringComparison.OrdinalIgnoreCase)
                .Replace("__UNICODE_ESCAPE__", "\\u") + "\"";
            return JsonSerializer.Deserialize<string>(wrapped) ?? text;
        }
        catch
        {
            return text;
        }
    }
}
