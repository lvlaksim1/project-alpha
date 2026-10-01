using System.IO;
using System.Text.Json;
using Baraban.Models;

namespace Baraban.Services;

public sealed class SessionStore
{
    private readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    public string RootDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Baraban");

    public string WebViewDataDirectory => Path.Combine(RootDirectory, "WebView2");
    private string SessionPath => Path.Combine(RootDirectory, "session.bin");

    public SessionStore() => Directory.CreateDirectory(RootDirectory);

    public SessionProfile Load()
    {
        if (!File.Exists(SessionPath))
            return new SessionProfile();

        try
        {
            var encrypted = File.ReadAllBytes(SessionPath);
            var json = DpapiStore.Unprotect(encrypted);
            return JsonSerializer.Deserialize<SessionProfile>(json, _json) ?? new SessionProfile();
        }
        catch
        {
            return new SessionProfile();
        }
    }

    public void Save(SessionProfile profile)
    {
        profile.UpdatedUtc = DateTimeOffset.UtcNow;
        Directory.CreateDirectory(RootDirectory);
        var json = JsonSerializer.Serialize(profile, _json);
        File.WriteAllBytes(SessionPath, DpapiStore.Protect(json));
    }

    public string ExportEditable(SessionProfile profile) => JsonSerializer.Serialize(profile, _json);

    public SessionProfile ImportEditable(string json) =>
        JsonSerializer.Deserialize<SessionProfile>(json, _json)
        ?? throw new InvalidDataException("Не удалось разобрать JSON сессии.");

    public void Clear()
    {
        if (File.Exists(SessionPath))
            File.Delete(SessionPath);
    }
}
