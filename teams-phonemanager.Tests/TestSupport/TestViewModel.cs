using teams_phonemanager.Services;
using teams_phonemanager.Services.Interfaces;
using teams_phonemanager.ViewModels;

namespace teams_phonemanager.Tests.TestSupport
{
    /// <summary>
    /// Minimal concrete <see cref="ViewModelBase"/> subclass that exposes the protected execution helpers
    /// as public passthroughs, purely so the base-class behavior (session-expiry gate, throttle-retry seam,
    /// cancellation handling, preview/confirm dialogs) can be exercised directly without going through any
    /// particular feature ViewModel.
    /// </summary>
    public class TestViewModel : ViewModelBase
    {
        public TestViewModel(ViewModelTestHarness harness)
            : base(
                harness.PowerShellContextService.Object,
                harness.PowerShellCommandService.Object,
                harness.LoggingService.Object,
                harness.SessionManager.Object,
                harness.NavigationService.Object,
                harness.ErrorHandlingService.Object,
                harness.ValidationService.Object,
                harness.SharedStateService.Object,
                harness.DialogService.Object)
        {
        }

        public Task<OperationResult<string>> RunExecuteAsync(string command, string context = "", bool allowThrottleRetry = false)
            => ExecutePowerShellCommandAsync(command, null, context, allowThrottleRetry);

        public Task<OperationResult<string>?> RunPreviewAndExecuteAsync(string command, string context = "")
            => PreviewAndExecuteAsync(command, context);

        public Task<OperationResult<string>?> RunConfirmAndExecuteAsync(string command, string confirmMessage, string context = "")
            => ConfirmAndExecuteAsync(command, confirmMessage, context);

        public void RunCancel() => CancelOperation();
    }
}
