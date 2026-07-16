using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using teams_phonemanager.Services.Interfaces;
using teams_phonemanager.Services;
using teams_phonemanager.Models;
using teams_phonemanager.Planning;
using teams_phonemanager.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace teams_phonemanager.ViewModels
{
    public partial class WizardViewModel : ViewModelBase
    {
        [ObservableProperty]
        private int _currentStep = 0;

        [ObservableProperty]
        private int _totalSteps = 10;

        [ObservableProperty]
        private string _stepTitle = string.Empty;

        [ObservableProperty]
        private string _stepDescription = string.Empty;

        [ObservableProperty]
        private string _stepScript = string.Empty;

        [ObservableProperty]
        private bool _stepCompleted = false;

        [ObservableProperty]
        private bool _stepFailed = false;

        [ObservableProperty]
        private string _stepResult = string.Empty;

        [ObservableProperty]
        private ObservableCollection<WizardStepInfo> _steps = new();

        private readonly IDryRunPlanBuilder? _planBuilder;
        private readonly IDryRunPlanExporter? _planExporter;

        /// <summary>
        /// The most recently generated dry-run plan for the current configuration. Null until the operator
        /// generates a preview. Generating it performs no tenant mutation and executes no PowerShell.
        /// </summary>
        [ObservableProperty]
        private DryRunPlan? _plan;

        public bool CanGoNext => CurrentStep < TotalSteps - 1 && !StepFailed;
        public bool CanGoPrevious => CurrentStep > 0;
        public bool CanExecuteStep => !IsBusy && !StepCompleted;

        public PhoneManagerVariables Variables => _sharedStateService?.Variables ?? new PhoneManagerVariables();

        public WizardViewModel(
            IPowerShellContextService powerShellContextService,
            IPowerShellCommandService powerShellCommandService,
            ILoggingService loggingService,
            ISessionManager sessionManager,
            INavigationService navigationService,
            IErrorHandlingService errorHandlingService,
            IValidationService validationService,
            ISharedStateService sharedStateService,
            IDialogService dialogService,
            IDryRunPlanBuilder? planBuilder = null,
            IDryRunPlanExporter? planExporter = null,
            IAuditLog? auditLog = null)
            : base(powerShellContextService, powerShellCommandService, loggingService,
                  sessionManager, navigationService, errorHandlingService, validationService, sharedStateService, dialogService, auditLog)
        {
            _planBuilder = planBuilder;
            _planExporter = planExporter;
            _loggingService.Log("Wizard page loaded", LogLevel.Info);
            InitializeSteps();
            UpdateCurrentStep();
        }

        private void InitializeSteps()
        {
            Steps = new ObservableCollection<WizardStepInfo>
            {
                new(0, "Review Configuration", "Verify all variables before starting the setup.", "Settings"),
                new(1, "Create M365 Group", "Create the Microsoft 365 distribution group for call queue agents.", "Execute"),
                new(2, "Create CQ Resource Account", "Create the resource account for the Call Queue.", "Execute"),
                new(3, "License CQ Resource Account", "Set usage location and assign Teams Phone license to the CQ resource account.", "Execute"),
                new(4, "Create Call Queue", "Create the Call Queue and associate it with the resource account.", "Execute"),
                new(5, "Create AA Resource Account", "Create the resource account for the Auto Attendant.", "Execute"),
                new(6, "License AA Resource Account", "Set usage location, assign license, and assign phone number to the AA resource account.", "Execute"),
                new(7, "Create Auto Attendant", "Create call flows, schedule, and the Auto Attendant (runs as one script).", "Execute"),
                new(8, "Associate AA Resource Account", "Associate the AA resource account with the Auto Attendant.", "Execute"),
                new(9, "Setup Complete", "All components have been created. You can now add holidays from the Holidays page.", "Summary"),
            };
            TotalSteps = Steps.Count;
        }

        private void UpdateCurrentStep()
        {
            if (CurrentStep >= 0 && CurrentStep < Steps.Count)
            {
                var step = Steps[CurrentStep];
                StepTitle = step.Title;
                StepDescription = step.Description;
                StepCompleted = step.IsCompleted;
                StepFailed = step.IsFailed;
                StepResult = step.Result;
                StepScript = GetScriptForStep(CurrentStep);
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanGoPrevious));
                OnPropertyChanged(nameof(CanExecuteStep));
                GoToNextStepCommand.NotifyCanExecuteChanged();
            }
        }

        private string GetScriptForStep(int step)
        {
            var vars = Variables;
            try
            {
                return step switch
                {
                    0 => FormatVariablesSummary(vars),
                    1 => _powerShellCommandService.GetCreateM365GroupCommand(vars.M365Group),
                    2 => _powerShellCommandService.GetCreateResourceAccountCommand(vars),
                    3 => BuildLicenseCqScript(vars),
                    4 => _powerShellCommandService.GetCreateCallQueueCommand(vars),
                    5 => _powerShellCommandService.GetCreateAutoAttendantResourceAccountCommand(vars),
                    6 => BuildLicenseAaScript(vars),
                    7 => _powerShellCommandService.GetCreateAutoAttendantCommand(vars),
                    8 => _powerShellCommandService.GetAssociateResourceAccountWithAutoAttendantCommand(vars.RaaaUPN, vars.AaDisplayName),
                    9 => "🎉 Setup complete! All Teams Phone components have been created.",
                    _ => string.Empty
                };
            }
            catch (Exception ex)
            {
                return $"# Error generating script preview:\n# {ex.Message}";
            }
        }

        private string FormatVariablesSummary(PhoneManagerVariables vars)
        {
            return $"""
            # ═══════════════════════════════════════
            # Configuration Review
            # ═══════════════════════════════════════
            
            # Customer:          {vars.Customer}
            # Customer Group:    {vars.CustomerGroupName}
            # Fallback Domain:   {vars.MsFallbackDomain}
            # Language:           {vars.LanguageId}
            # Time Zone:          {vars.TimeZoneId}
            # Usage Location:     {vars.UsageLocation}
            
            # ── Computed Names ──
            # M365 Group:         {vars.M365Group}
            # CQ Resource UPN:    {vars.RacqUPN}
            # CQ Display Name:    {vars.CqDisplayName}
            # AA Resource UPN:    {vars.RaaaUPN}
            # AA Display Name:    {vars.AaDisplayName}
            # Phone Number:       {vars.RaaAnr}
            # Phone Number Type:  {vars.PhoneNumberType}
            
            # ── Licensing ──
            # SKU ID:             {vars.SkuId}
            # CQ App ID:          {vars.CsAppCqId}
            # AA App ID:          {vars.CsAppAaId}
            
            # ═══════════════════════════════════════
            # Ensure all values are correct before
            # proceeding. Use the Variables page to
            # make changes.
            # ═══════════════════════════════════════
            """;
        }

        private string BuildLicenseCqScript(PhoneManagerVariables vars)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("# Step 1: Set usage location for CQ Resource Account");
            sb.AppendLine(_powerShellCommandService.GetUpdateResourceAccountUsageLocationCommand(vars.RacqUPN, vars.UsageLocation));
            sb.AppendLine();
            sb.AppendLine("# Step 2: Assign Teams Phone license");
            sb.AppendLine(_powerShellCommandService.GetAssignLicenseCommand(vars.RacqUPN, vars.SkuId));
            return sb.ToString();
        }

        private string BuildLicenseAaScript(PhoneManagerVariables vars)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("# Step 1: Set usage location for AA Resource Account");
            sb.AppendLine(_powerShellCommandService.GetUpdateAutoAttendantResourceAccountUsageLocationCommand(vars.RaaaUPN, vars.UsageLocation));
            sb.AppendLine();
            sb.AppendLine("# Step 2: Assign Teams Phone license");
            sb.AppendLine(_powerShellCommandService.GetAssignAutoAttendantLicenseCommand(vars.RaaaUPN, vars.SkuId));
            sb.AppendLine();
            sb.AppendLine("# Step 3: Assign phone number");
            sb.AppendLine(_powerShellCommandService.GetAssignPhoneNumberToAutoAttendantCommand(vars.RaaaUPN, vars.RaaAnr, vars.PhoneNumberType));
            return sb.ToString();
        }

        [RelayCommand]
        private async Task ExecuteCurrentStepAsync()
        {
            if (CurrentStep < 0 || CurrentStep >= Steps.Count)
                return;

            var step = Steps[CurrentStep];

            // Step 0 is review-only, step 9 is summary
            if (CurrentStep == 0 || CurrentStep == 9)
            {
                step.IsCompleted = true;
                StepCompleted = true;
                StepResult = CurrentStep == 0 ? "Configuration reviewed." : "Setup complete!";
                step.Result = StepResult;
                OnPropertyChanged(nameof(CanExecuteStep));
                return;
            }

            var script = GetScriptForStep(CurrentStep);
            if (string.IsNullOrEmpty(script))
            {
                StatusMessage = "No script to execute for this step.";
                return;
            }

            // Show preview and confirm before executing
            var result = await PreviewAndExecuteAsync(script, $"Wizard Step {CurrentStep}: {step.Title}");

            if (result == null)
            {
                StatusMessage = "Step cancelled.";
                return;
            }

            var output = result.Value ?? string.Empty;

            if (result.HasErrorMarker)
            {
                step.IsFailed = true;
                step.Result = output;
                StepFailed = true;
                StepResult = output;
                StatusMessage = $"Step {CurrentStep} failed. Review the output and retry or skip.";
                _loggingService.Log($"Wizard step {CurrentStep} ({step.Title}) failed: {output}", LogLevel.Error);
            }
            else
            {
                step.IsCompleted = true;
                step.Result = output;
                StepCompleted = true;
                StepResult = string.IsNullOrWhiteSpace(output) ? "Step completed successfully." : output;
                StatusMessage = $"Step {CurrentStep} completed: {step.Title}";
                _loggingService.Log($"Wizard step {CurrentStep} ({step.Title}) completed successfully", LogLevel.Info);
            }

            OnPropertyChanged(nameof(CanExecuteStep));
        }

        [RelayCommand(CanExecute = nameof(CanGoNext))]
        private void GoToNextStep()
        {
            if (CurrentStep < TotalSteps - 1 && !StepFailed)
            {
                CurrentStep++;
                UpdateCurrentStep();
            }
        }

        [RelayCommand]
        private void GoToPreviousStep()
        {
            if (CurrentStep > 0)
            {
                CurrentStep--;
                UpdateCurrentStep();
            }
        }

        [RelayCommand]
        private void SkipStep()
        {
            if (CurrentStep < TotalSteps - 1)
            {
                var step = Steps[CurrentStep];
                step.IsSkipped = true;
                step.Result = "Skipped by user.";
                _loggingService.Log($"Wizard step {CurrentStep} ({step.Title}) skipped", LogLevel.Warning);
                CurrentStep++;
                UpdateCurrentStep();
            }
        }

        [RelayCommand]
        private void RetryStep()
        {
            if (CurrentStep >= 0 && CurrentStep < Steps.Count)
            {
                var step = Steps[CurrentStep];
                step.IsFailed = false;
                step.IsCompleted = false;
                step.Result = string.Empty;
                StepCompleted = false;
                StepFailed = false;
                StepResult = string.Empty;
                OnPropertyChanged(nameof(CanExecuteStep));
            }
        }

        [RelayCommand]
        private void GoToVariablesPage()
        {
            NavigateToVariables();
        }

        [RelayCommand]
        private void GoToHolidaysPage()
        {
            NavigateToHolidays();
        }

        /// <summary>
        /// Builds a read-only dry-run plan for the current configuration: every object that would be created
        /// with its resolved names, UPNs, number and settings, plus validation and preflight results. This
        /// performs no tenant mutation and executes no PowerShell — it derives purely from the current
        /// variables, so it can never diverge from what execution would emit.
        /// </summary>
        [RelayCommand]
        private void GeneratePlan()
        {
            if (_planBuilder == null)
            {
                StatusMessage = "Plan preview is unavailable.";
                return;
            }

            Plan = _planBuilder.BuildWizardPlan(Variables);
            var entry = Plan.Entries.Count > 0 ? Plan.Entries[0] : null;
            if (entry != null && !entry.IsValid)
            {
                StatusMessage = $"Plan generated with {entry.ValidationErrors.Count} validation issue(s). Review before executing.";
            }
            else
            {
                StatusMessage = $"Plan generated: {Plan.TotalObjectCount} objects would be created/changed.";
            }
            _loggingService.Log("Wizard dry-run plan generated", LogLevel.Info);
        }

        [RelayCommand]
        private Task ExportPlanJsonAsync() => ExportPlanAsync(asJson: true);

        [RelayCommand]
        private Task ExportPlanCsvAsync() => ExportPlanAsync(asJson: false);

        private async Task ExportPlanAsync(bool asJson)
        {
            if (_planBuilder == null || _planExporter == null)
            {
                StatusMessage = "Plan export is unavailable.";
                return;
            }

            Plan ??= _planBuilder.BuildWizardPlan(Variables);

            var content = asJson ? _planExporter.ToJson(Plan) : _planExporter.ToCsv(Plan);
            var extension = asJson ? "json" : "csv";
            var saved = await DryRunPlanExportHelper.SavePlanAsync(content, $"wizard-dry-run-plan.{extension}", extension);
            if (saved != null)
            {
                StatusMessage = $"Plan exported to {saved}";
                _loggingService.Log($"Wizard dry-run plan exported to: {saved}", LogLevel.Info);
            }
            else
            {
                StatusMessage = "Plan export cancelled.";
            }
        }

        partial void OnCurrentStepChanged(int value)
        {
            UpdateCurrentStep();
        }

        partial void OnStepFailedChanged(bool value)
        {
            OnPropertyChanged(nameof(CanGoNext));
            GoToNextStepCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// Represents a single wizard step with its state.
    /// </summary>
    public partial class WizardStepInfo : ObservableObject
    {
        public int StepNumber { get; }
        public string Title { get; }
        public string Description { get; }
        public string StepType { get; }

        [ObservableProperty]
        private bool _isCompleted;

        [ObservableProperty]
        private bool _isFailed;

        [ObservableProperty]
        private bool _isSkipped;

        [ObservableProperty]
        private string _result = string.Empty;

        public string StatusIcon => IsCompleted ? "✅" : IsFailed ? "❌" : IsSkipped ? "⏭️" : "⬜";

        public WizardStepInfo(int stepNumber, string title, string description, string stepType)
        {
            StepNumber = stepNumber;
            Title = title;
            Description = description;
            StepType = stepType;
        }

        partial void OnIsCompletedChanged(bool value) => OnPropertyChanged(nameof(StatusIcon));
        partial void OnIsFailedChanged(bool value) => OnPropertyChanged(nameof(StatusIcon));
        partial void OnIsSkippedChanged(bool value) => OnPropertyChanged(nameof(StatusIcon));
    }
}
