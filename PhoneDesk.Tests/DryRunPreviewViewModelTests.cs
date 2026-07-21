using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PhoneDesk.Models;
using PhoneDesk.Planning;
using PhoneDesk.Services;
using PhoneDesk.Services.Interfaces;
using PhoneDesk.Services.ScriptBuilders;
using PhoneDesk.Tests.TestSupport;
using PhoneDesk.ViewModels;

namespace PhoneDesk.Tests
{
    /// <summary>
    /// Covers the dry-run preview wiring in <see cref="BulkOperationsViewModel"/> and <see cref="WizardViewModel"/>:
    /// plan generation performs no PowerShell execution (issue #68 zero-mutation guarantee), the bulk execute
    /// gate blocks on invalid rows unless the operator opts into skipping them, and skipping runs only the
    /// valid rows.
    /// </summary>
    public class DryRunPreviewViewModelTests
    {
        private const string Header =
            "Customer,CustomerGroupName,MsFallbackDomain,RaaAnrName,LanguageId,TimeZoneId,UsageLocation,PhoneNumber,PhoneNumberType,OpeningHours1Start,OpeningHours1End,OpeningHours2Start,OpeningHours2End";
        private const string ValidRow =
            "contoso,reception,@contoso.onmicrosoft.com,haupt,de-DE,W. Europe Standard Time,CH,+41441234567,DirectRouting,08:00,12:00,13:00,17:00";
        private const string InvalidPhoneRow =
            "fabrikam,empfang,@fabrikam.onmicrosoft.com,haupt,de-DE,W. Europe Standard Time,CH,not-a-number,DirectRouting,08:00,12:00,13:00,17:00";

        private static IPowerShellSanitizationService VerbatimSanitizer()
        {
            var mock = new Mock<IPowerShellSanitizationService>();
            mock.Setup(s => s.SanitizeString(It.IsAny<string>())).Returns<string>(x => x);
            mock.Setup(s => s.SanitizeIdentifier(It.IsAny<string>())).Returns<string>(x => x);
            return mock.Object;
        }

        private static IDryRunPlanBuilder RealPlanBuilder() =>
            new DryRunPlanBuilder(new ValidationService(new Mock<ISessionManager>().Object));

        private static BulkOperationsViewModel CreateBulkViewModel(ViewModelTestHarness harness) =>
            new(
                harness.PowerShellContextService.Object,
                harness.PowerShellCommandService.Object,
                harness.LoggingService.Object,
                harness.SessionManager.Object,
                harness.NavigationService.Object,
                harness.ErrorHandlingService.Object,
                harness.ValidationService.Object,
                harness.SharedStateService.Object,
                harness.DialogService.Object,
                new BulkOperationsScriptBuilder(harness.PowerShellCommandService.Object, VerbatimSanitizer()),
                RealPlanBuilder(),
                new DryRunPlanExporter());

        private static WizardViewModel CreateWizardViewModel(ViewModelTestHarness harness) =>
            new(
                harness.PowerShellContextService.Object,
                harness.PowerShellCommandService.Object,
                harness.LoggingService.Object,
                harness.SessionManager.Object,
                harness.NavigationService.Object,
                harness.ErrorHandlingService.Object,
                harness.ValidationService.Object,
                harness.SharedStateService.Object,
                harness.DialogService.Object,
                RealPlanBuilder(),
                new DryRunPlanExporter());

        private static void VerifyNoPowerShellExecuted(ViewModelTestHarness harness) =>
            harness.PowerShellContextService.Verify(
                p => p.ExecuteCommandWithDetailsAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<IProgress<PowerShellProgress>?>(), It.IsAny<CancellationToken>()),
                Times.Never);

        [Fact]
        public void BulkGeneratePlan_PopulatesPlan_WithoutExecutingPowerShell()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateBulkViewModel(harness);
            vm.CsvContent = Header + "\n" + ValidRow;
            vm.ParseCsvCommand.Execute(null);

            vm.GeneratePlanCommand.Execute(null);

            Assert.NotNull(vm.Plan);
            Assert.Equal(1, vm.Plan!.EntryCount);
            Assert.True(vm.Plan.IsFullyValid);
            VerifyNoPowerShellExecuted(harness);
        }

        [Fact]
        public async Task BulkExecuteAll_InvalidRow_SkipOff_BlocksAndDoesNotExecute()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateBulkViewModel(harness);
            vm.CsvContent = Header + "\n" + ValidRow + "\n" + InvalidPhoneRow;
            vm.ParseCsvCommand.Execute(null);
            vm.SkipInvalidRows = false;

            await vm.ExecuteAllCommand.ExecuteAsync(null);

            Assert.Contains("invalid", vm.StatusMessage, StringComparison.OrdinalIgnoreCase);
            Assert.NotNull(vm.Plan);
            Assert.Equal(1, vm.Plan!.InvalidEntryCount);
            VerifyNoPowerShellExecuted(harness);
        }

        [Fact]
        public async Task BulkExecuteAll_InvalidRow_SkipOn_ExecutesValidRowsOnly()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateBulkViewModel(harness);
            vm.CsvContent = Header + "\n" + ValidRow + "\n" + InvalidPhoneRow;
            vm.ParseCsvCommand.Execute(null);
            vm.SkipInvalidRows = true;
            harness.SetExecutionResult("SUCCESS: processed");

            await vm.ExecuteAllCommand.ExecuteAsync(null);

            // Only the single valid row runs.
            Assert.Equal(1, vm.TotalCount);
            Assert.Equal(1, vm.ProcessedCount);
            harness.PowerShellContextService.Verify(
                p => p.ExecuteCommandWithDetailsAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<IProgress<PowerShellProgress>?>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task BulkExecuteAll_AllValid_ExecutesNormally()
        {
            var harness = new ViewModelTestHarness();
            var vm = CreateBulkViewModel(harness);
            vm.CsvContent = Header + "\n" + ValidRow;
            vm.ParseCsvCommand.Execute(null);
            harness.SetExecutionResult("SUCCESS: processed");

            await vm.ExecuteAllCommand.ExecuteAsync(null);

            Assert.Contains("completed successfully", vm.StatusMessage);
            harness.PowerShellContextService.Verify(
                p => p.ExecuteCommandWithDetailsAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<IProgress<PowerShellProgress>?>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public void WizardGeneratePlan_PopulatesPlanWithResolvedObjects_WithoutExecutingPowerShell()
        {
            var harness = new ViewModelTestHarness();
            var sharedVars = new PhoneManagerVariables
            {
                Customer = "contoso",
                CustomerGroupName = "reception",
                MsFallbackDomain = "@contoso.onmicrosoft.com",
                RaaAnrName = "haupt",
                LanguageId = "de-DE",
                TimeZoneId = "W. Europe Standard Time",
                UsageLocation = "CH",
                RaaAnr = "+41441234567",
                PhoneNumberType = "DirectRouting"
            };
            harness.SharedStateService.SetupGet(s => s.Variables).Returns(sharedVars);
            var vm = CreateWizardViewModel(harness);

            vm.GeneratePlanCommand.Execute(null);

            Assert.NotNull(vm.Plan);
            Assert.Single(vm.Plan!.Entries);
            Assert.True(vm.Plan.Entries[0].IsValid);
            Assert.Contains(vm.Plan.Entries[0].Objects, o => o.Type == PlannedObjectType.AutoAttendant);
            VerifyNoPowerShellExecuted(harness);
        }
    }
}
