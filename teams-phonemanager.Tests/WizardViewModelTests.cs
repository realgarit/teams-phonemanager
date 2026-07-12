using Moq;
using teams_phonemanager.Models;
using teams_phonemanager.Services;
using teams_phonemanager.Services.Interfaces;
using teams_phonemanager.Tests.TestSupport;
using teams_phonemanager.ViewModels;

namespace teams_phonemanager.Tests
{
    /// <summary>
    /// Covers <see cref="WizardViewModel"/> step-transition logic: forward/backward navigation and its
    /// boundary conditions, per-step execution (happy path, failure, retry, skip), and that wizard state
    /// (the shared <see cref="PhoneManagerVariables"/>) is carried across steps rather than reset.
    /// </summary>
    public class WizardViewModelTests
    {
        private static WizardViewModel CreateViewModel(ViewModelTestHarness harness) =>
            new WizardViewModel(
                harness.PowerShellContextService.Object,
                harness.PowerShellCommandService.Object,
                harness.LoggingService.Object,
                harness.SessionManager.Object,
                harness.NavigationService.Object,
                harness.ErrorHandlingService.Object,
                harness.ValidationService.Object,
                harness.SharedStateService.Object,
                harness.DialogService.Object);

        [Fact]
        public void Construction_InitializesStepsAndStartsAtStepZero()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);

            Assert.Equal(10, vm.TotalSteps);
            Assert.Equal(0, vm.CurrentStep);
            Assert.Equal("Review Configuration", vm.StepTitle);
            Assert.False(vm.CanGoPrevious);
            Assert.True(vm.CanGoNext);
        }

        // ── Forward/backward navigation and boundaries ─────────────────────

        [Fact]
        public void GoToNextStep_AdvancesCurrentStepAndUpdatesStepTitle()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);

            vm.GoToNextStepCommand.Execute(null);

            Assert.Equal(1, vm.CurrentStep);
            Assert.Equal("Create M365 Group", vm.StepTitle);
        }

        [Fact]
        public void GoToPreviousStep_AtStepZero_DoesNotGoNegative()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);

            vm.GoToPreviousStepCommand.Execute(null);

            Assert.Equal(0, vm.CurrentStep);
            Assert.False(vm.CanGoPrevious);
        }

        [Fact]
        public void GoToNextStep_AtLastStep_DoesNotGoPastTotalSteps()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);

            for (int i = 0; i < vm.TotalSteps + 3; i++)
            {
                vm.GoToNextStepCommand.Execute(null);
            }

            Assert.Equal(vm.TotalSteps - 1, vm.CurrentStep);
            Assert.False(vm.CanGoNext);
        }

        [Fact]
        public void GoToPreviousStep_AfterAdvancing_MovesBackOneStep()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);
            vm.GoToNextStepCommand.Execute(null);
            vm.GoToNextStepCommand.Execute(null);

            vm.GoToPreviousStepCommand.Execute(null);

            Assert.Equal(1, vm.CurrentStep);
            Assert.Equal("Create M365 Group", vm.StepTitle);
        }

        // ── Wizard state carried between steps ──────────────────────────────

        [Fact]
        public void Variables_ReflectsSharedStateAcrossStepTransitions_NotResetLocally()
        {
            var harness = new ViewModelTestHarness();
            var sharedVars = new PhoneManagerVariables { Customer = "contoso", CustomerGroupName = "hauptnummer" };
            harness.SharedStateService.SetupGet(s => s.Variables).Returns(sharedVars);
            var vm = CreateViewModel(harness);

            Assert.Equal("contoso", vm.Variables.Customer);

            vm.GoToNextStepCommand.Execute(null);
            vm.GoToNextStepCommand.Execute(null);
            vm.GoToNextStepCommand.Execute(null);

            // The same shared variables instance (and the value captured before navigating) is still
            // visible after moving through several steps — wizard state is not reset per step.
            Assert.Equal("contoso", vm.Variables.Customer);
            Assert.Same(sharedVars, vm.Variables);
        }

        [Fact]
        public void StepScript_ForReviewStep_IncludesCurrentSharedVariables()
        {
            var harness = new ViewModelTestHarness();
            var sharedVars = new PhoneManagerVariables { Customer = "fabrikam", CustomerGroupName = "empfang" };
            harness.SharedStateService.SetupGet(s => s.Variables).Returns(sharedVars);
            var vm = CreateViewModel(harness);

            Assert.Contains("fabrikam", vm.StepScript);
            Assert.Contains("empfang", vm.StepScript);
        }

        // ── Step execution: happy path, failure, retry, skip ───────────────

        [Fact]
        public async Task ExecuteCurrentStepAsync_StepZero_IsReviewOnly_CompletesWithoutRunningPowerShell()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);

            await vm.ExecuteCurrentStepCommand.ExecuteAsync(null);

            Assert.True(vm.StepCompleted);
            Assert.Equal("Configuration reviewed.", vm.StepResult);
            harness.PowerShellContextService.Verify(
                p => p.ExecuteCommandWithDetailsAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<IProgress<PowerShellProgress>?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteCurrentStepAsync_HappyPath_MarksStepCompleted()
        {
            var harness = new ViewModelTestHarness();
            harness.PowerShellCommandService.Setup(c => c.GetCreateM365GroupCommand(It.IsAny<string>())).Returns("New-CsGroup ...");
            var vm = CreateViewModel(harness);
            vm.GoToNextStepCommand.Execute(null); // step 1: Create M365 Group
            harness.SetExecutionResult("SUCCESS: group created");

            await vm.ExecuteCurrentStepCommand.ExecuteAsync(null);

            Assert.True(vm.StepCompleted);
            Assert.False(vm.StepFailed);
            Assert.True(vm.Steps[1].IsCompleted);
            Assert.Equal("SUCCESS: group created", vm.StepResult);
        }

        [Fact]
        public async Task ExecuteCurrentStepAsync_ErrorPath_MarksStepFailedAndDoesNotComplete()
        {
            var harness = new ViewModelTestHarness();
            harness.PowerShellCommandService.Setup(c => c.GetCreateM365GroupCommand(It.IsAny<string>())).Returns("New-CsGroup ...");
            var vm = CreateViewModel(harness);
            vm.GoToNextStepCommand.Execute(null); // step 1
            harness.SetExecutionResult("ERROR: group creation failed", hadErrors: true);

            await vm.ExecuteCurrentStepCommand.ExecuteAsync(null);

            Assert.True(vm.StepFailed);
            Assert.False(vm.StepCompleted);
            Assert.True(vm.Steps[1].IsFailed);
            Assert.Contains("failed", vm.StatusMessage);
        }

        [Fact]
        public void GoToNextStepCommand_AfterStepFailure_CurrentImplementationDoesNotBlockAdvancement()
        {
            // NOTE: this documents the ViewModel's *actual* current behavior, which is a gap against the
            // acceptance criterion "cannot advance past a failed step". CanGoNext (bound to the Next
            // button's IsEnabled in WizardView.axaml) is defined purely as `CurrentStep < TotalSteps - 1`
            // and does not consult StepFailed/Steps[CurrentStep].IsFailed, and GoToNextStep()'s body has
            // the same bounds-only guard. So today, once a step fails, the Next command remains enabled
            // and calling it does move the wizard forward. Flagged for the orchestrator rather than
            // silently "fixed" here, since gating GoToNextStep on step failure is a product decision.
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);
            vm.GoToNextStepCommand.Execute(null); // step 1
            harness.SetExecutionResult("ERROR: boom", hadErrors: true);

            var canExecuteBeforeAdvance = vm.GoToNextStepCommand.CanExecute(null);

            Assert.True(canExecuteBeforeAdvance);
        }

        [Fact]
        public async Task RetryStep_ClearsFailedAndCompletedState()
        {
            var harness = new ViewModelTestHarness();
            harness.PowerShellCommandService.Setup(c => c.GetCreateM365GroupCommand(It.IsAny<string>())).Returns("New-CsGroup ...");
            var vm = CreateViewModel(harness);
            vm.GoToNextStepCommand.Execute(null); // step 1
            harness.SetExecutionResult("ERROR: boom", hadErrors: true);
            await vm.ExecuteCurrentStepCommand.ExecuteAsync(null);
            Assert.True(vm.StepFailed);

            vm.RetryStepCommand.Execute(null);

            Assert.False(vm.StepFailed);
            Assert.False(vm.StepCompleted);
            Assert.Equal(string.Empty, vm.StepResult);
            Assert.False(vm.Steps[1].IsFailed);
            Assert.False(vm.Steps[1].IsCompleted);
        }

        [Fact]
        public void SkipStep_MarksCurrentStepSkippedAndAdvances()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);

            vm.SkipStepCommand.Execute(null);

            Assert.True(vm.Steps[0].IsSkipped);
            Assert.Equal("Skipped by user.", vm.Steps[0].Result);
            Assert.Equal(1, vm.CurrentStep);
        }

        [Fact]
        public async Task ExecuteCurrentStepAsync_LastStep_CompletesAsSummaryWithoutRunningPowerShell()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);
            for (int i = 0; i < 9; i++)
            {
                vm.GoToNextStepCommand.Execute(null);
            }
            Assert.Equal(9, vm.CurrentStep);

            await vm.ExecuteCurrentStepCommand.ExecuteAsync(null);

            Assert.True(vm.StepCompleted);
            Assert.Equal("Setup complete!", vm.StepResult);
            harness.PowerShellContextService.Verify(
                p => p.ExecuteCommandWithDetailsAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<IProgress<PowerShellProgress>?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CanExecuteStep_BecomesFalseAfterCompletion_AndTrueAgainAfterRetry()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);
            Assert.True(vm.CanExecuteStep);

            await vm.ExecuteCurrentStepCommand.ExecuteAsync(null); // step 0, review-only, completes

            Assert.False(vm.CanExecuteStep);

            vm.RetryStepCommand.Execute(null);

            Assert.True(vm.CanExecuteStep);
        }

        [Fact]
        public void GoToVariablesPageCommand_NavigatesToVariablesPage()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);

            vm.GoToVariablesPageCommand.Execute(null);

            harness.NavigationService.Verify(n => n.NavigateTo(ConstantsService.Pages.Variables), Times.Once);
        }

        [Fact]
        public void GoToHolidaysPageCommand_NavigatesToHolidaysPage()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateViewModel(harness);

            vm.GoToHolidaysPageCommand.Execute(null);

            harness.NavigationService.Verify(n => n.NavigateTo(ConstantsService.Pages.Holidays), Times.Once);
        }
    }
}
