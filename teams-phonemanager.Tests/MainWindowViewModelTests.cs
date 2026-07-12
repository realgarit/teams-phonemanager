using System.Collections.ObjectModel;
using Moq;
using teams_phonemanager.Services;
using teams_phonemanager.Services.Interfaces;
using teams_phonemanager.Tests.TestSupport;
using teams_phonemanager.ViewModels;

namespace teams_phonemanager.Tests
{
    public class MainWindowViewModelTests
    {
        private readonly Mock<IPageViewModelFactory> _pageViewModelFactory = new();
        private readonly Mock<IUpdateCheckService> _updateCheckService = new();

        public MainWindowViewModelTests()
        {
            // A sentinel placeholder is enough: MainWindowViewModel treats CurrentViewModel as an opaque
            // object resolved via the factory, it never inspects the concrete type itself.
            _pageViewModelFactory.Setup(f => f.Create(It.IsAny<string>())).Returns((ViewModelBase)null!);
            _updateCheckService.Setup(u => u.CheckForUpdateAsync(It.IsAny<CancellationToken>())).ReturnsAsync((UpdateInfo?)null);
        }

        private MainWindowViewModel CreateViewModel(ViewModelTestHarness harness)
        {
            // MainWindowViewModel wires a CollectionChanged handler onto ILoggingService.LogEntries in
            // its constructor; the shared harness leaves LogEntries unstubbed (unused by the other VMs),
            // so this VM needs its own backing collection.
            harness.LoggingService.SetupGet(l => l.LogEntries).Returns(new ObservableCollection<string>());

            return new MainWindowViewModel(
                harness.PowerShellContextService.Object,
                harness.PowerShellCommandService.Object,
                harness.LoggingService.Object,
                harness.SessionManager.Object,
                harness.NavigationService.Object,
                harness.ErrorHandlingService.Object,
                harness.ValidationService.Object,
                harness.SharedStateService.Object,
                harness.DialogService.Object,
                _pageViewModelFactory.Object,
                _updateCheckService.Object);
        }

        [Fact]
        public void Construction_ResolvesInitialPageFromFactoryAndLogsStartup()
        {
            var harness = new ViewModelTestHarness();

            CreateViewModel(harness);

            _pageViewModelFactory.Verify(f => f.Create(ConstantsService.Pages.Welcome), Times.Once);
            harness.LoggingService.Verify(l => l.Log("Application started", LogLevel.Info), Times.Once);
        }

        [Fact]
        public void Construction_ChecksForUpdatesOnStartup()
        {
            var harness = new ViewModelTestHarness();

            CreateViewModel(harness);

            _updateCheckService.Verify(u => u.CheckForUpdateAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Construction_UpdateAvailable_ShowsUpdateBanner()
        {
            var harness = new ViewModelTestHarness();
            _updateCheckService.Setup(u => u.CheckForUpdateAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateInfo("2.0.0", "https://example.com/release"));

            var vm = CreateViewModel(harness);

            Assert.True(vm.IsUpdateBannerVisible);
            Assert.Contains("2.0.0", vm.UpdateBannerMessage);
        }

        [Fact]
        public void DismissUpdateBannerCommand_HidesBanner()
        {
            var harness = new ViewModelTestHarness();
            _updateCheckService.Setup(u => u.CheckForUpdateAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateInfo("2.0.0", "https://example.com/release"));
            var vm = CreateViewModel(harness);

            vm.DismissUpdateBannerCommand.Execute(null);

            Assert.False(vm.IsUpdateBannerVisible);
        }

        [Fact]
        public void NavigateToCommand_DelegatesToNavigationService()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);

            vm.NavigateToCommand.Execute(ConstantsService.Pages.M365Groups);

            harness.NavigationService.Verify(n => n.NavigateTo(ConstantsService.Pages.M365Groups), Times.Once);
        }

        [Fact]
        public void CurrentPageChanged_ResolvesNewViewModelFromFactory()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);
            _pageViewModelFactory.Invocations.Clear();

            vm.CurrentPage = ConstantsService.Pages.Holidays;

            _pageViewModelFactory.Verify(f => f.Create(ConstantsService.Pages.Holidays), Times.Once);
        }

        [Fact]
        public void ToggleSettingsCommand_TogglesIsSettingsOpen()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);

            vm.ToggleSettingsCommand.Execute(null);
            Assert.True(vm.IsSettingsOpen);

            vm.ToggleSettingsCommand.Execute(null);
            Assert.False(vm.IsSettingsOpen);
        }

        [Fact]
        public void CloseSettingsCommand_ClosesSettingsPanel()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);
            vm.IsSettingsOpen = true;

            vm.CloseSettingsCommand.Execute(null);

            Assert.False(vm.IsSettingsOpen);
        }

        [Fact]
        public void ToggleLogDialogCommand_TogglesIsLogDialogOpen()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);

            vm.ToggleLogDialogCommand.Execute(null);
            Assert.True(vm.IsLogDialogOpen);

            vm.ToggleLogDialogCommand.Execute(null);
            Assert.False(vm.IsLogDialogOpen);
        }

        [Fact]
        public void CloseLogDialogCommand_ClosesLogDialog()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);
            vm.IsLogDialogOpen = true;

            vm.CloseLogDialogCommand.Execute(null);

            Assert.False(vm.IsLogDialogOpen);
        }

        [Fact]
        public void ClearLogCommand_ClearsLoggingService()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);

            vm.ClearLogCommand.Execute(null);

            harness.LoggingService.Verify(l => l.Clear(), Times.Once);
        }

        [Fact]
        public void OpenUpdatePageCommand_NoReleaseUrl_DoesNotThrow()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);

            var exception = Record.Exception(() => vm.OpenUpdatePageCommand.Execute(null));

            Assert.Null(exception);
        }

        [Fact]
        public void SkipScriptPreview_ProxiesToSharedStateService()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);

            vm.SkipScriptPreview = true;

            harness.SharedStateService.VerifySet(s => s.SkipScriptPreview = true, Times.Once);
        }

        [Fact]
        public void RefreshConnectionStatus_RaisesPropertyChangedForConnectionProperties()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);
            var raisedProperties = new List<string>();
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != null)
                {
                    raisedProperties.Add(e.PropertyName);
                }
            };

            vm.RefreshConnectionStatus();

            Assert.Contains(nameof(MainWindowViewModel.IsTeamsConnected), raisedProperties);
            Assert.Contains(nameof(MainWindowViewModel.IsGraphConnected), raisedProperties);
            Assert.Contains(nameof(MainWindowViewModel.ConnectionStatusText), raisedProperties);
        }

        [Fact]
        public void Dispose_DetachesEventHandlersWithoutThrowing()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);

            var exception = Record.Exception(() => vm.Dispose());

            Assert.Null(exception);
        }
    }
}
