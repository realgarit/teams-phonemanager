using Moq;
using PhoneDesk.Services;
using PhoneDesk.Services.Interfaces;
using PhoneDesk.Tests.TestSupport;
using PhoneDesk.ViewModels;

namespace PhoneDesk.Tests
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
            Assert.Contains("PhoneDesk", vm.WelcomeMessage);
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
