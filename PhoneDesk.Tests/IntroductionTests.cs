using System.Collections.ObjectModel;
using Moq;
using PhoneDesk.Localization;
using PhoneDesk.Services;
using PhoneDesk.Services.Interfaces;
using PhoneDesk.Tests.TestSupport;
using PhoneDesk.ViewModels;

namespace PhoneDesk.Tests;

public class IntroductionTests
{
    private sealed class Preferences : IUserPreferencesStore
    {
        public bool Dismissed { get; private set; }
        public int SaveCount { get; private set; }
        public AppLanguage? LoadLanguage() => AppLanguage.English;
        public void SaveLanguage(AppLanguage language) { }
        public bool LoadIntroductionDismissed() => Dismissed;
        public void SaveIntroductionDismissed() { Dismissed = true; SaveCount++; }
    }

    private static MainWindowViewModel Create(Preferences preferences, ViewModelTestHarness h)
    {
        h.LoggingService.SetupGet(l => l.LogEntries).Returns(new ObservableCollection<string>());
        var update = new Mock<IUpdateCheckService>();
        update.Setup(u => u.CheckForUpdateAsync(It.IsAny<CancellationToken>())).ReturnsAsync((UpdateInfo?)null);
        return new MainWindowViewModel(h.PowerShellContextService.Object, h.PowerShellCommandService.Object,
            h.LoggingService.Object, h.SessionManager.Object, new NavigationService(h.LoggingService.Object),
            h.ErrorHandlingService.Object, h.ValidationService.Object, h.SharedStateService.Object, h.DialogService.Object,
            Mock.Of<IPageViewModelFactory>(), update.Object, Mock.Of<IUpdateInstallerService>(),
            Mock.Of<IBundledModuleVersionService>(), translationService: h.TranslationService, userPreferences: preferences);
    }

    [Fact]
    public void TourFollowsBeginnerPathWithoutConnectingOrProvisioning()
    {
        var prefs = new Preferences();
        var h = new ViewModelTestHarness();
        using var vm = Create(prefs, h);
        Assert.True(vm.IsIntroductionVisible);
        Assert.Equal("Welcome", vm.CurrentPage);
        foreach (var page in new[] { "GetStarted", "Variables" })
        {
            vm.NextIntroductionCommand.Execute(null);
            Assert.Equal(page, vm.CurrentPage);
            Assert.True(vm.IsIntroductionVisible);
        }
        vm.NextIntroductionCommand.Execute(null);
        Assert.False(vm.IsIntroductionVisible);
        Assert.Equal("Wizard", vm.CurrentPage);
        Assert.True(prefs.Dismissed);
        Assert.Equal(1, prefs.SaveCount);
        h.PowerShellContextService.VerifyNoOtherCalls();
        h.PowerShellCommandService.VerifyNoOtherCalls();
        using var restarted = Create(prefs, new ViewModelTestHarness());
        Assert.False(restarted.IsIntroductionVisible);
    }

    [Fact]
    public void SkipPersistsAndReplayDoesNotReEnableAutomaticDisplay()
    {
        var prefs = new Preferences();
        using var vm = Create(prefs, new ViewModelTestHarness());
        vm.DismissIntroductionCommand.Execute(null);
        vm.NextIntroductionCommand.Execute(null);
        Assert.Equal("Welcome", vm.CurrentPage);
        vm.IsSettingsOpen = true;
        vm.ShowIntroductionCommand.Execute(null);
        Assert.False(vm.IsSettingsOpen);
        Assert.True(vm.IsIntroductionVisible);
        Assert.True(prefs.Dismissed);
        using var restarted = Create(prefs, new ViewModelTestHarness());
        Assert.False(restarted.IsIntroductionVisible);
    }

    [Fact]
    public void ManualNavigationUpdatesTourAndLeavingSetupDismissesIt()
    {
        var prefs = new Preferences();
        using var vm = Create(prefs, new ViewModelTestHarness());
        vm.NavigateToCommand.Execute(ConstantsService.Pages.GetStarted);
        Assert.Equal(2, vm.IntroductionStep);
        Assert.True(vm.IsIntroductionVisible);
        vm.NavigateToCommand.Execute("Dashboard");
        Assert.False(vm.IsIntroductionVisible);
        Assert.True(prefs.Dismissed);
    }

    [Fact]
    public void ActiveTourRelocalizesWithoutResettingProgress()
    {
        var h = new ViewModelTestHarness();
        using var vm = Create(new Preferences(), h);
        vm.NextIntroductionCommand.Execute(null);
        var english = vm.IntroductionBody;
        h.TranslationService.CurrentLanguage = AppLanguage.German;
        Assert.NotEqual(english, vm.IntroductionBody);
        Assert.Equal(2, vm.IntroductionStep);
        Assert.Contains("Konfiguration", vm.IntroductionNextText);
    }
}
