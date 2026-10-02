using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Baraban.Services;

public static partial class JsonTextFormatter
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

        return UnicodeEscapeRegex().Replace(text, match =>
        {
            var value = int.Parse(match.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return char.ConvertFromUtf32(value);
        });
    }

    [GeneratedRegex(@"\\u([0-9a-fA-F]{4})", RegexOptions.CultureInvariant)]
    private static partial Regex UnicodeEscapeRegex();
}
