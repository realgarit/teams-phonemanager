using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PhoneDesk.Localization;
using PhoneDesk.Services.Interfaces;

namespace PhoneDesk.Services;

public sealed class UserPreferencesStore : IUserPreferencesStore
{
    private const string SettingsFileName = "settings.json";
    private readonly string _settingsFilePath;

    public UserPreferencesStore()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PhoneDesk"))
    {
    }

    public UserPreferencesStore(string settingsDirectory)
    {
        _settingsFilePath = Path.Combine(settingsDirectory, SettingsFileName);
    }

    private readonly object _writeLock = new();

    private UserPreferencesDocument ReadDocument()
    {
        try
        {
            using var stream = File.OpenRead(_settingsFilePath);
            return JsonSerializer.Deserialize<UserPreferencesDocument>(stream) ?? new();
        }
        catch { return new(); }
    }

    public AppLanguage? LoadLanguage()
    {
        var value = ReadDocument().Language;
        return Enum.TryParse<AppLanguage>(value, ignoreCase: false, out var language) && Enum.IsDefined(language)
            ? language : null;
    }

    public bool LoadIntroductionDismissed() => ReadDocument().IntroductionDismissed;

    public void SaveIntroductionDismissed()
    {
        lock (_writeLock) SaveDocument(ReadDocument() with { IntroductionDismissed = true });
    }

    public void SaveLanguage(AppLanguage language)
    {
        lock (_writeLock) SaveDocument(ReadDocument() with { Language = language.ToString() });
    }

    private void SaveDocument(UserPreferencesDocument document)
    {
        var settingsDirectory = Path.GetDirectoryName(_settingsFilePath);
        if (string.IsNullOrWhiteSpace(settingsDirectory))
        {
            return;
        }

        var temporaryFilePath = _settingsFilePath + ".tmp";

        try
        {
            Directory.CreateDirectory(settingsDirectory);

            var json = JsonSerializer.Serialize(document);

            using (var stream = new FileStream(temporaryFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                writer.Write(json);
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }

            File.Move(temporaryFilePath, _settingsFilePath, overwrite: true);
        }
        catch
        {
        }
        finally
        {
            try
            {
                if (File.Exists(temporaryFilePath))
                {
                    File.Delete(temporaryFilePath);
                }
            }
            catch
            {
            }
        }
    }

    private sealed record UserPreferencesDocument
    {
        [JsonPropertyName("language")]
        public string? Language { get; init; }

        [JsonPropertyName("introductionDismissed")]
        public bool IntroductionDismissed { get; init; }
    }
}
