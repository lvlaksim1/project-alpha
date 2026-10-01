using System.Text.RegularExpressions;

namespace Baraban.Services;

public static partial class TemplateResolver
{
    [GeneratedRegex("\\{\\{(?<name>[A-Za-z0-9_.-]+)\\}\\}")]
    private static partial Regex VariableRegex();

    public static string Resolve(string value, IReadOnlyDictionary<string, string> variables) =>
        VariableRegex().Replace(value ?? "", m =>
        {
            var key = m.Groups["name"].Value;
            return variables.TryGetValue(key, out var result) ? result : m.Value;
        });
}
