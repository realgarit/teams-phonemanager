using Moq;
using teams_phonemanager.Services;
using teams_phonemanager.Services.Interfaces;
using teams_phonemanager.Tests.TestSupport;
using teams_phonemanager.ViewModels;

namespace teams_phonemanager.Tests
{
    public class WelcomeViewModelTests
    {
        private static WelcomeViewModel CreateViewModel(ViewModelTestHarness harness) =>
            new WelcomeViewModel(
                harness.PowerShellContextService.Object,
                harness.PowerShellCommandService.Object,
                harness.LoggingService.Object,
                harness.SessionManager.Object,
                harness.NavigationService.Object,
                harness.ErrorHandlingService.Object,
                harness.ValidationService.Object);

        [Fact]
        public void Construction_SetsWelcomeMessageAndLogsPageLoad()
        {
            var harness = new ViewModelTestHarness();

            var vm = CreateViewModel(harness);

            Assert.False(string.IsNullOrWhiteSpace(vm.WelcomeMessage));
            Assert.Contains("Teams Phone Manager", vm.WelcomeMessage);
            harness.LoggingService.Verify(l => l.Log("Welcome page loaded", LogLevel.Info), Times.Once);
        }

        [Fact]
        public void NavigateToGetStartedCommand_NavigatesToGetStartedPage()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);

            vm.NavigateToGetStartedCommand.Execute(null);

            harness.NavigationService.Verify(n => n.NavigateTo("GetStarted"), Times.Once);
        }
    }
}
