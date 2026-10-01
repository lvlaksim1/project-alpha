using System.IO;
using System.Text.Json;
using Baraban.Models;

namespace Baraban.Services;

public sealed class DrumRepository
{
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public IReadOnlyList<DrumDefinition> Load(string directory)
    {
        Directory.CreateDirectory(directory);
        var result = new List<DrumDefinition>();
        foreach (var file in Directory.EnumerateFiles(directory, "*.json", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var definition = JsonSerializer.Deserialize<DrumDefinition>(File.ReadAllText(file), _json);
                if (definition is not null && !string.IsNullOrWhiteSpace(definition.Id))
                    result.Add(definition);
            }
            catch
            {
                // A broken drum file must not prevent the rest of the application from loading.
            }
        }
        return result.OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
    }
}
