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
                definition.Result is null)
                return;

            var changed = false;

            // v0.3.10 incorrectly treated offers[].isWinner as a possible winner authority.
            // Runtime evidence shows winnerOffer.id from PUT /accept is authoritative.
            if (definition.Result.WinnerFlagPath.Equals("isWinner", StringComparison.OrdinalIgnoreCase))
            {
                definition.Result.WinnerFlagPath = "";
                changed = true;
            }

            changed |= EnsureVariable(definition, "wheelActionTitle", "Крутить скорее!");
            changed |= EnsureVariable(definition, "reconfirmTitle", "");
            changed |= EnsureVariable(definition, "reconfirmSubtitle", "");
            changed |= EnsureVariable(definition, "reconfirmNeedPaid", "");
            changed |= EnsureVariable(definition, "motivationTitle", "");

            var getWheel = definition.Requests.FirstOrDefault(x =>
                x.Id.Equals("getWheelOfFortune", StringComparison.OrdinalIgnoreCase));
            if (getWheel is not null)
            {
                changed |= EnsureCapture(getWheel, "confirmed", "$.confirmed");
                changed |= EnsureCapture(getWheel, "wheelActionTitle", "$.actionButton.title");
            }

            var accept = definition.Requests.FirstOrDefault(x =>
                x.Id.Equals("acceptWheelOfFortune", StringComparison.OrdinalIgnoreCase));
            if (accept is not null)
            {
                changed |= EnsureCapture(accept, "winnerOfferId", "$.winnerOffer.id");
                changed |= EnsureCapture(accept, "reconfirmTitle", "$.reconfirmButton.title");
                changed |= EnsureCapture(accept, "reconfirmSubtitle", "$.reconfirmButton.subtitle");
                changed |= EnsureCapture(accept, "reconfirmNeedPaid", "$.reconfirmButton.needPaid");
                changed |= EnsureCapture(accept, "motivationTitle", "$.motivation.title");
            }

            definition.Actions ??= new DrumActionMapping();
            var actions = definition.Actions;

            if (actions.Repeat.Count == 0)
            {
                actions.Repeat.Add(new DrumActionStep { RequestId = "getWheelOfFortune" });
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(actions.ClaimLabel) ||
                actions.ClaimLabel.Equals("Получить приз", StringComparison.Ordinal))
            {
                actions.ClaimLabel = "Крутить скорее!";
                changed = true;
            }

            if (!actions.ClaimLabelVariable.Equals("wheelActionTitle", StringComparison.Ordinal))
            {
                actions.ClaimLabelVariable = "wheelActionTitle";
                changed = true;
            }
            if (!actions.RepeatTitleVariable.Equals("reconfirmTitle", StringComparison.Ordinal))
            {
                actions.RepeatTitleVariable = "reconfirmTitle";
                changed = true;
            }
            if (!actions.RepeatSubtitleVariable.Equals("reconfirmSubtitle", StringComparison.Ordinal))
            {
                actions.RepeatSubtitleVariable = "reconfirmSubtitle";
                changed = true;
            }
            if (!actions.RepeatNeedPaidVariable.Equals("reconfirmNeedPaid", StringComparison.Ordinal))
            {
                actions.RepeatNeedPaidVariable = "reconfirmNeedPaid";
                changed = true;
            }
            if (!actions.MotivationVariable.Equals("motivationTitle", StringComparison.Ordinal))
            {
                actions.MotivationVariable = "motivationTitle";
                changed = true;
            }

            if (!changed)
                return;

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

    private static bool EnsureVariable(DrumDefinition definition, string name, string value)
    {
        if (definition.Variables.ContainsKey(name))
            return false;

        definition.Variables[name] = value;
        return true;
    }

    private static bool EnsureCapture(HttpRequestDefinition request, string name, string path)
    {
        if (request.Captures.TryGetValue(name, out var existing) &&
            existing.Equals(path, StringComparison.Ordinal))
            return false;

        request.Captures[name] = path;
        return true;
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
