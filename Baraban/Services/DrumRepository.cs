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

            // Runtime evidence proves winnerOffer.id is authoritative.
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
            changed |= EnsureVariable(definition, "paidRepeatOrder", "");
            changed |= EnsureVariable(definition, "paidRepeatSuccess", "");
            changed |= EnsureVariable(definition, "paidRepeatPurchased", "false");

            var getWheel = definition.Requests.FirstOrDefault(x =>
                x.Id.Equals("getWheelOfFortune", StringComparison.OrdinalIgnoreCase));
            if (getWheel is not null)
            {
                changed |= EnsureCapture(getWheel, "confirmed", "$.confirmed");
                changed |= EnsureCapture(getWheel, "wheelActionTitle", "$.actionButton.title");
            }

            var getWinner = definition.Requests.FirstOrDefault(x =>
                x.Id.Equals("getWheelOfFortuneWinner", StringComparison.OrdinalIgnoreCase));
            if (getWinner is not null)
            {
                changed |= EnsureCapture(getWinner, "winnerOfferId", "$.winnerOffer.id");
                changed |= EnsureCapture(getWinner, "reconfirmTitle", "$.reconfirmButton.title");
                changed |= EnsureCapture(getWinner, "reconfirmSubtitle", "$.reconfirmButton.subtitle");
                changed |= EnsureCapture(getWinner, "reconfirmNeedPaid", "$.reconfirmButton.needPaid");
                changed |= EnsureCapture(getWinner, "motivationTitle", "$.motivation.title");
                changed |= EnsureCapture(getWinner, "paidRepeatOrder", "$.reconfirmButton.modalView.order");
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
                changed |= EnsureCapture(accept, "paidRepeatOrder", "$.reconfirmButton.modalView.order");
            }

            var buyRepeat = definition.Requests.FirstOrDefault(x =>
                x.Id.Equals("buyWheelRepeat", StringComparison.OrdinalIgnoreCase));

            if (buyRepeat is null)
            {
                buyRepeat = new HttpRequestDefinition
                {
                    Id = "buyWheelRepeat",
                    Name = "ОПЛАТИТЬ повторную попытку барабана",
                    Method = "POST",
                    Url = "https://web.alfabank.ru/api/v1/loyalty-view/offer",
                    Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["Accept"] = "application/json, text/plain, */*",
                        ["Content-Type"] = "application/json",
                        ["Origin"] = "https://web.alfabank.ru",
                        ["Referer"] = "https://web.alfabank.ru/marketplace/?loyaltyType=104",
                        ["X-SCREEN-DIMENSION"] = "desktop"
                    },
                    Body = "{{paidRepeatOrder}}",
                    Captures = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["paidRepeatSuccess"] = "$.success",
                        ["wheelActionTitle"] = "$.button.title"
                    },
                    AutoRun = false,
                    IsConfirmation = true
                };
                definition.Requests.Add(buyRepeat);
                changed = true;
            }
            else
            {
                changed |= EnsureCapture(buyRepeat, "paidRepeatSuccess", "$.success");
                changed |= EnsureCapture(buyRepeat, "wheelActionTitle", "$.button.title");
            }

            definition.Actions ??= new DrumActionMapping();
            var actions = definition.Actions;

            if (actions.Repeat.Count == 0)
            {
                actions.Repeat.Add(new DrumActionStep { RequestId = "getWheelOfFortune" });
                changed = true;
            }

            if (actions.PaidRepeat.Count == 0)
            {
                actions.PaidRepeat.Add(new DrumActionStep { RequestId = "buyWheelRepeat" });
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(actions.ClaimLabel) ||
                actions.ClaimLabel.Equals("Получить приз", StringComparison.Ordinal))
            {
                actions.ClaimLabel = "Крутить скорее!";
                changed = true;
            }

            changed |= SetString(actions.ClaimLabelVariable, "wheelActionTitle",
                value => actions.ClaimLabelVariable = value);
            changed |= SetString(actions.RepeatTitleVariable, "reconfirmTitle",
                value => actions.RepeatTitleVariable = value);
            changed |= SetString(actions.RepeatSubtitleVariable, "reconfirmSubtitle",
                value => actions.RepeatSubtitleVariable = value);
            changed |= SetString(actions.RepeatNeedPaidVariable, "reconfirmNeedPaid",
                value => actions.RepeatNeedPaidVariable = value);
            changed |= SetString(actions.MotivationVariable, "motivationTitle",
                value => actions.MotivationVariable = value);
            changed |= SetString(actions.PaidRepeatOrderVariable, "paidRepeatOrder",
                value => actions.PaidRepeatOrderVariable = value);
            changed |= SetString(actions.PaidRepeatSuccessVariable, "paidRepeatSuccess",
                value => actions.PaidRepeatSuccessVariable = value);
            changed |= SetString(actions.PaidRepeatPurchasedVariable, "paidRepeatPurchased",
                value => actions.PaidRepeatPurchasedVariable = value);

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

    private static bool SetString(string current, string expected, Action<string> setter)
    {
        if (current.Equals(expected, StringComparison.Ordinal))
            return false;

        setter(expected);
        return true;
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
