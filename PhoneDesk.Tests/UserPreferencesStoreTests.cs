using System.Text.Json;
using PhoneDesk.Localization;
using PhoneDesk.Services;

namespace PhoneDesk.Tests;

public sealed class UserPreferencesStoreTests : IDisposable
{
    private readonly string _settingsDirectory = Path.Combine(
        Path.GetTempPath(),
        "phonedesk-tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public void LoadLanguage_ReturnsNullWhenSettingsFileIsMissing()
    {
        var store = CreateStore();

        Assert.Null(store.LoadLanguage());
    }

    [Fact]
    public void LoadLanguage_ReturnsNullWhenSettingsFileIsMalformed()
    {
        Directory.CreateDirectory(_settingsDirectory);
        File.WriteAllText(Path.Combine(_settingsDirectory, "settings.json"), "{ this is not json");

        var store = CreateStore();

        Assert.Null(store.LoadLanguage());
    }

    [Fact]
    public void LoadLanguage_ReturnsNullWhenStoredValueIsUnknown()
    {
        Directory.CreateDirectory(_settingsDirectory);
        File.WriteAllText(Path.Combine(_settingsDirectory, "settings.json"), """{ "language": "Spanish" }""");

        var store = CreateStore();

        Assert.Null(store.LoadLanguage());
    }

    [Fact]
    public void SaveLanguage_WritesLanguageAndLoadLanguageReadsItBack()
    {
        var store = CreateStore();

        store.SaveLanguage(AppLanguage.German);

        Assert.Equal(AppLanguage.German, store.LoadLanguage());

        var json = File.ReadAllText(Path.Combine(_settingsDirectory, "settings.json"));
        using var document = JsonDocument.Parse(json);
        Assert.Equal("German", document.RootElement.GetProperty("language").GetString());
    }

    [Fact]
    public void SaveLanguage_WritesAtomicallyWithoutLeavingTemporaryFile()
    {
        var store = CreateStore();

        store.SaveLanguage(AppLanguage.English);

        Assert.True(File.Exists(Path.Combine(_settingsDirectory, "settings.json")));
        Assert.False(File.Exists(Path.Combine(_settingsDirectory, "settings.json.tmp")));
    }

    private UserPreferencesStore CreateStore() => new(_settingsDirectory);

    [Fact]
    public void IntroductionDismissalSurvivesLanguageChangesAndNewStoreInstances()
    {
        var store = CreateStore();
        Assert.False(store.LoadIntroductionDismissed());
        store.SaveLanguage(AppLanguage.German);
        store.SaveIntroductionDismissed();
        Assert.Equal(AppLanguage.German, CreateStore().LoadLanguage());
        Assert.True(CreateStore().LoadIntroductionDismissed());
        store.SaveLanguage(AppLanguage.English);
        Assert.True(CreateStore().LoadIntroductionDismissed());
        Assert.Equal(AppLanguage.English, CreateStore().LoadLanguage());
    }

    [Fact]
    public void LegacyLanguageOnlySettingsStillOfferIntroductionOnce()
    {
        Directory.CreateDirectory(_settingsDirectory);
        File.WriteAllText(Path.Combine(_settingsDirectory, "settings.json"), """{"language":"German"}""");
        Assert.False(CreateStore().LoadIntroductionDismissed());
        CreateStore().SaveIntroductionDismissed();
        Assert.Equal(AppLanguage.German, CreateStore().LoadLanguage());
        Assert.True(CreateStore().LoadIntroductionDismissed());
    }

    public void Dispose()
    {
        if (Directory.Exists(_settingsDirectory))
        {
            Directory.Delete(_settingsDirectory, recursive: true);
        }
    }
}
