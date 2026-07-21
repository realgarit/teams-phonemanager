using Moq;
using PhoneDesk.Services;
using PhoneDesk.Services.Interfaces;
using PhoneDesk.Tests.TestSupport;

namespace PhoneDesk.Tests
{
    public class ViewModelBaseTests
    {
        [Fact]
        public async Task ExecutePowerShellCommandAsync_HappyPath_ReturnsSuccessResult()
        {
            var harness = new ViewModelTestHarness();
            harness.SetExecutionResult("SUCCESS: done");
            var vm = new TestViewModel(harness);

            var result = await vm.RunExecuteAsync("Get-Thing", "Context");

            Assert.True(result.IsSuccess);
            Assert.Equal(OperationErrorCategory.None, result.Category);
            harness.ErrorHandlingService.Verify(e => e.HandlePowerShellError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutePowerShellCommandAsync_ErrorPath_ReportsErrorAndReturnsFailure()
        {
            var harness = new ViewModelTestHarness();
            harness.SetExecutionResult("ERROR: boom");
            var vm = new TestViewModel(harness);

            var result = await vm.RunExecuteAsync("Get-Thing", "Context");

            Assert.False(result.IsSuccess);
            harness.ErrorHandlingService.Verify(e => e.HandlePowerShellError("Get-Thing", It.IsAny<string>(), "Context"), Times.Once);
        }

        [Fact]
        public async Task ExecutePowerShellCommandAsync_SessionExpired_ShortCircuitsWithoutCallingPowerShell()
        {
            var harness = new ViewModelTestHarness();
            harness.SetSessionExpired();
            var vm = new TestViewModel(harness);

            var result = await vm.RunExecuteAsync("Get-Thing", "Context");

            Assert.False(result.IsSuccess);
            Assert.Equal(OperationErrorCategory.AuthSession, result.Category);
            harness.SessionManager.Verify(s => s.ResetSession(), Times.Once);
            harness.ErrorHandlingService.Verify(e => e.HandleConnectionError(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            harness.PowerShellContextService.Verify(
                p => p.ExecuteCommandWithDetailsAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<IProgress<PowerShellProgress>?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecutePowerShellCommandAsync_Cancelled_ReturnsCancelledCategoryWithoutErrorDialog()
        {
            var harness = new ViewModelTestHarness();
            harness.SetCancelled();
            var vm = new TestViewModel(harness);

            var result = await vm.RunExecuteAsync("Get-Thing", "Context");

            Assert.False(result.IsSuccess);
            Assert.Equal(OperationErrorCategory.Cancelled, result.Category);
            // Cancellation is not an error: no error dialog should be raised.
            harness.ErrorHandlingService.Verify(e => e.HandlePowerShellError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutePowerShellCommandAsync_AllowThrottleRetryFalse_DoesNotUseRetryPolicy()
        {
            var harness = new ViewModelTestHarness();
            harness.SetExecutionResult("SUCCESS");
            var vm = new TestViewModel(harness);

            var result = await vm.RunExecuteAsync("Get-Thing", "Context", allowThrottleRetry: false);

            Assert.True(result.IsSuccess);
            // Single call: the non-retry path calls ExecuteCommandWithDetailsAsync exactly once.
            harness.PowerShellContextService.Verify(
                p => p.ExecuteCommandWithDetailsAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<IProgress<PowerShellProgress>?>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecutePowerShellCommandAsync_AllowThrottleRetryTrue_RetriesOnThrottlingThenSucceeds()
        {
            var harness = new ViewModelTestHarness();
            var throttled = new PowerShellExecutionResult { Output = "ERROR: 429 too many requests", HadErrors = true };
            var succeeded = new PowerShellExecutionResult { Output = "SUCCESS", HadErrors = false };

            harness.PowerShellContextService
                .SetupSequence(p => p.ExecuteCommandWithDetailsAsync(
                    It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<IProgress<PowerShellProgress>?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(throttled)
                .ReturnsAsync(succeeded);

            var vm = new TestViewModel(harness);

            var result = await vm.RunExecuteAsync("Get-Thing", "Context", allowThrottleRetry: true);

            Assert.True(result.IsSuccess);
            harness.PowerShellContextService.Verify(
                p => p.ExecuteCommandWithDetailsAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<IProgress<PowerShellProgress>?>(), It.IsAny<CancellationToken>()),
                Times.AtLeast(2));
        }

        [Fact]
        public async Task PreviewAndExecuteAsync_UserCancelsPreview_ReturnsNullAndDoesNotExecute()
        {
            var harness = new ViewModelTestHarness();
            harness.SharedStateService.SetupGet(s => s.SkipScriptPreview).Returns(false);
            harness.DialogService.Setup(d => d.ShowScriptPreviewAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
            var vm = new TestViewModel(harness);

            var result = await vm.RunPreviewAndExecuteAsync("Remove-Thing", "Context");

            Assert.Null(result);
            harness.PowerShellContextService.Verify(
                p => p.ExecuteCommandWithDetailsAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<IProgress<PowerShellProgress>?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ConfirmAndExecuteAsync_UserConfirms_Executes()
        {
            var harness = new ViewModelTestHarness();
            harness.SharedStateService.SetupGet(s => s.SkipDeleteConfirmation).Returns(false);
            var vm = new TestViewModel(harness);

            var result = await vm.RunConfirmAndExecuteAsync("Remove-Thing", "Are you sure?", "Context");

            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);
        }

        [Fact]
        public void CancelOperation_WithNoOperationRunning_DoesNotThrow()
        {
            var harness = new ViewModelTestHarness();
            var vm = new TestViewModel(harness);

            var exception = Record.Exception(() => vm.RunCancel());

            Assert.Null(exception);
        }
    }
}
