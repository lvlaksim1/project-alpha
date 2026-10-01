using System.IO;
using System.Text.Json;
using Baraban.Models;

namespace Baraban.Services;

public sealed class DrumRepository
{
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public void SeedUserDirectory(string builtInDirectory, string userDirectory)
    {
        Directory.CreateDirectory(userDirectory);
        if (!Directory.Exists(builtInDirectory))
            return;

        foreach (var source in Directory.EnumerateFiles(builtInDirectory, "*.json", SearchOption.TopDirectoryOnly))
        {
            var destination = Path.Combine(userDirectory, Path.GetFileName(source));
            if (!File.Exists(destination))
                File.Copy(source, destination);
        }
    }

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
