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

        MigrateKnownModuleCorrections(userDirectory);
    }

    private void MigrateKnownModuleCorrections(string userDirectory)
    {
        var path = Path.Combine(userDirectory, "alfa-online-supercashback-wheel-2026-10.json");
        if (!File.Exists(path))
            return;

        try
        {
            var definition = JsonSerializer.Deserialize<DrumDefinition>(File.ReadAllText(path), _json);
            if (definition is null ||
                !definition.Id.Equals("alfa-online-supercashback-wheel-2026-10", StringComparison.OrdinalIgnoreCase) ||
                definition.Result is null ||
                !definition.Result.WinnerFlagPath.Equals("isWinner", StringComparison.OrdinalIgnoreCase))
                return;

            // v0.3.10 incorrectly treated offers[].isWinner as a possible winner authority.
            // Preserve every other user edit and only remove that obsolete fallback.
            definition.Result.WinnerFlagPath = "";

            var writeOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            File.WriteAllText(path, JsonSerializer.Serialize(definition, writeOptions));
        }
        catch
        {
            // Never block application startup because of a migration attempt.
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
