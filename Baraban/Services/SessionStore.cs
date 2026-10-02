using System.IO;
using System.Text.Json;
using Baraban.Models;

namespace Baraban.Services;

public sealed class SessionStore
{
    private readonly JsonSerializerOptions _json = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public string RootDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Baraban");

    public string WebViewDataDirectory => Path.Combine(RootDirectory, "WebView2");
    private string ProfilesDirectory => Path.Combine(RootDirectory, "AuthProfiles");
    private string ProfilesIndexPath => Path.Combine(RootDirectory, "profiles.json");
    private string LegacySessionPath => Path.Combine(RootDirectory, "session.bin");

    public SessionStore()
    {
        Directory.CreateDirectory(RootDirectory);
        Directory.CreateDirectory(ProfilesDirectory);
        EnsureProfiles();
    }

    public string ActiveProfileName
    {
        get
        {
            var index = LoadIndex();
            return index.Profiles.First(x => x.Id == index.ActiveProfileId).Name;
        }
    }

    public IReadOnlyList<string> ListProfiles() =>
        LoadIndex().Profiles.Select(x => x.Name).OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase).ToList();

    public SessionProfile Load()
    {
        var index = LoadIndex();
        return LoadById(index.ActiveProfileId);
    }

    public void Save(SessionProfile profile)
    {
        var index = LoadIndex();
        SaveById(index.ActiveProfileId, profile);
    }

    public void SaveAs(string name, SessionProfile profile)
    {
        name = NormalizeName(name);
        var index = LoadIndex();
        var entry = index.Profiles.FirstOrDefault(x =>
            x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));

        if (entry is null)
        {
            entry = new AuthProfileEntry
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = name
            };
            index.Profiles.Add(entry);
        }
        else
        {
            entry.Name = name;
        }

        index.ActiveProfileId = entry.Id;
        SaveIndex(index);
        SaveById(entry.Id, profile);
    }

    public bool SetActiveProfile(string name)
    {
        name = NormalizeName(name);
        var index = LoadIndex();
        var entry = index.Profiles.FirstOrDefault(x =>
            x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        if (entry is null)
            return false;

        index.ActiveProfileId = entry.Id;
        SaveIndex(index);
        return true;
    }

    public bool DeleteProfile(string name)
    {
        name = NormalizeName(name);
        var index = LoadIndex();
        var entry = index.Profiles.FirstOrDefault(x =>
            x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        if (entry is null || index.Profiles.Count <= 1)
            return false;

        index.Profiles.Remove(entry);
        var path = ProfilePath(entry.Id);
        if (File.Exists(path))
            File.Delete(path);

        if (index.ActiveProfileId == entry.Id)
            index.ActiveProfileId = index.Profiles[0].Id;

        SaveIndex(index);
        return true;
    }

    public string ExportEditable(SessionProfile profile) => JsonSerializer.Serialize(profile, _json);

    public SessionProfile ImportEditable(string json) =>
        JsonSerializer.Deserialize<SessionProfile>(json, _json)
        ?? throw new InvalidDataException("Не удалось разобрать JSON сессии.");

    public void Clear() => Save(new SessionProfile());

    private void EnsureProfiles()
    {
        AuthProfilesIndex? index = null;
        if (File.Exists(ProfilesIndexPath))
        {
            try
            {
                index = JsonSerializer.Deserialize<AuthProfilesIndex>(
                    File.ReadAllText(ProfilesIndexPath), _json);
            }
            catch
            {
            }
        }

        if (index is not null && index.Profiles.Count > 0)
        {
            if (!index.Profiles.Any(x => x.Id == index.ActiveProfileId))
            {
                index.ActiveProfileId = index.Profiles[0].Id;
                SaveIndex(index);
            }
            return;
        }

        index = new AuthProfilesIndex
        {
            ActiveProfileId = "default",
            Profiles =
            [
                new AuthProfileEntry { Id = "default", Name = "Основной" }
            ]
        };

        var defaultPath = ProfilePath("default");
        if (File.Exists(LegacySessionPath))
        {
            File.Copy(LegacySessionPath, defaultPath, true);
        }
        else
        {
            SaveById("default", new SessionProfile());
        }

        SaveIndex(index);
    }

    private SessionProfile LoadById(string id)
    {
        var path = ProfilePath(id);
        if (!File.Exists(path))
            return new SessionProfile();

        try
        {
            var encrypted = File.ReadAllBytes(path);
            var json = DpapiStore.Unprotect(encrypted);
            return JsonSerializer.Deserialize<SessionProfile>(json, _json) ?? new SessionProfile();
        }
        catch
        {
            return new SessionProfile();
        }
    }

    private void SaveById(string id, SessionProfile profile)
    {
        profile.UpdatedUtc = DateTimeOffset.UtcNow;
        Directory.CreateDirectory(ProfilesDirectory);
        var json = JsonSerializer.Serialize(profile, _json);
        File.WriteAllBytes(ProfilePath(id), DpapiStore.Protect(json));
    }

    private AuthProfilesIndex LoadIndex()
    {
        EnsureProfiles();
        try
        {
            var index = JsonSerializer.Deserialize<AuthProfilesIndex>(
                File.ReadAllText(ProfilesIndexPath), _json);
            if (index is not null && index.Profiles.Count > 0)
                return index;
        }
        catch
        {
        }

        throw new InvalidDataException("Не удалось загрузить список профилей авторизации.");
    }

    private void SaveIndex(AuthProfilesIndex index)
    {
        Directory.CreateDirectory(RootDirectory);
        File.WriteAllText(ProfilesIndexPath, JsonSerializer.Serialize(index, _json));
    }

    private string ProfilePath(string id) => Path.Combine(ProfilesDirectory, id + ".bin");

    private static string NormalizeName(string name)
    {
        name = (name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidDataException("Введите имя профиля авторизации.");
        if (name.Length > 80)
            throw new InvalidDataException("Имя профиля слишком длинное.");
        return name;
    }

    private sealed class AuthProfilesIndex
    {
        public string ActiveProfileId { get; set; } = "default";
        public List<AuthProfileEntry> Profiles { get; set; } = [];
    }

    private sealed class AuthProfileEntry
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
    }
}
