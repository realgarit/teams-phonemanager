using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using teams_phonemanager.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using teams_phonemanager.Models;
using System.IO;
using System.Text.Json;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using teams_phonemanager.Helpers;

namespace teams_phonemanager.ViewModels
{
    public partial class VariablesViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel? _mainWindowViewModel;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome to the Variables page. Here you can set the variables that will be used throughout the application.";

        // Removed TeamsConnected and GraphConnected - no longer needed since lock was removed

        [ObservableProperty]
        private bool _showHolidayTimePicker = false;

        /// <summary>
        /// Observable collection of time options in 15-minute increments (00:00 to 23:45).
        /// Uses the shared TimeOptionsProvider for consistency across the application.
        /// </summary>
        public ObservableCollection<TimeSpan> TimeOptions => TimeOptionsProvider.TimeOptions;

        [ObservableProperty]
        private TimeSpan? _selectedHolidayTime;

        [ObservableProperty]
        private bool _showHolidaySeriesManager = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DialogHolidayDate))]
        private HolidayEntry? _editingHoliday;

        [ObservableProperty]
        private bool _showEditHolidayDialog = false;

        [ObservableProperty]
        private TimeSpan? _selectedEditHolidayTime;

        // Simple properties for Add/Edit dialogs
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DialogHolidayDate))]
        private DateTime _newHolidayDate = DateTime.Now;

        [ObservableProperty]
        private TimeSpan _newHolidayTime = new TimeSpan(0, 0, 0);

        [ObservableProperty]
        private TimeSpan _editHolidayTime = new TimeSpan(0, 0, 0);

        // Computed properties for the dialog
        public DateTime? DialogHolidayDate
        {
            get => EditingHoliday?.Date ?? NewHolidayDate;
            set
            {
                if (value.HasValue)
                {
                    if (EditingHoliday != null)
                        EditingHoliday.Date = value.Value;
                    else
                        NewHolidayDate = value.Value;
                }
            }
        }

        // Removed DialogHolidayTime - now using SelectedEditHolidayTime with ComboBoxItem approach

        public VariablesViewModel()
        {
            _mainWindowViewModel = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow?.DataContext as MainWindowViewModel
                : null;

            _loggingService.Log("Variables page loaded", LogLevel.Info);
            
            // Subscribe to variable changes for Call Queue configuration visibility
            if (_mainWindowViewModel?.Variables != null)
            {
                _mainWindowViewModel.Variables.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName?.StartsWith("Cq") == true)
                    {
                        UpdateCallQueueVisibility();
                    }
                    else if (e.PropertyName == nameof(PhoneManagerVariables.M365GroupId))
                    {
                        PrefillCallQueueTargets();
                    }
                    else if (e.PropertyName?.StartsWith("Aa") == true)
                    {
                        UpdateAutoAttendantVisibility();
                    }
                };
                
                // Prefill target fields if M365GroupId is already set
                PrefillCallQueueTargets();
            }
        }

        private void PrefillCallQueueTargets()
        {
            var variables = Variables;
            if (variables == null) return;

            var groupId = variables.M365GroupId;
            if (string.IsNullOrWhiteSpace(groupId)) return;

            // Prefill target fields if they are empty
            if (string.IsNullOrWhiteSpace(variables.CqOverflowActionTarget))
            {
                variables.CqOverflowActionTarget = groupId;
            }
            if (string.IsNullOrWhiteSpace(variables.CqTimeoutActionTarget))
            {
                variables.CqTimeoutActionTarget = groupId;
            }
            if (string.IsNullOrWhiteSpace(variables.CqNoAgentActionTarget))
            {
                variables.CqNoAgentActionTarget = groupId;
            }
        }

        public PhoneManagerVariables Variables
        {
            get => _mainWindowViewModel?.Variables ?? new PhoneManagerVariables();
            set
            {
                if (_mainWindowViewModel != null)
                {
                    _mainWindowViewModel.Variables = value;
                    OnPropertyChanged();
                    // Subscribe to changes on the new Variables instance
                    if (value != null)
                    {
                        value.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName?.StartsWith("Cq") == true)
                            {
                                UpdateCallQueueVisibility();
                            }
                            else if (e.PropertyName == nameof(PhoneManagerVariables.M365GroupId))
                            {
                                PrefillCallQueueTargets();
                            }
                            else if (e.PropertyName?.StartsWith("Aa") == true)
                            {
                                UpdateAutoAttendantVisibility();
                            }
                        };
                        
                        // Prefill target fields if M365GroupId is already set
                        PrefillCallQueueTargets();
                    }
                }
            }
        }

        public bool CanProceed => true; // Removed lock - Variables page is always accessible

        [RelayCommand]
        private async Task SaveVariablesToFileAsync()
        {
            try
            {
                var downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                downloadsPath = Path.Combine(downloadsPath, "Downloads");
                
                var fileName = $"PhoneManagerVariables_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var filePath = Path.Combine(downloadsPath, fileName);

                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(Variables, jsonOptions);
                await File.WriteAllTextAsync(filePath, json);

                _loggingService.Log($"Variables saved to: {filePath}", LogLevel.Info);
                await _errorHandlingService.ShowSuccess($"Variables saved successfully to:\n{filePath}", "Save Successful");
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error saving variables: {ex.Message}", LogLevel.Error);
                await _errorHandlingService.HandleGenericError($"Error saving variables:\n{ex.Message}", "SaveVariables");
            }
        }

        [RelayCommand]
        private async Task LoadVariablesFromFileAsync()
        {
            try
            {
                var window = Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
                if (window?.MainWindow != null)
                {
                    var storageProvider = window.MainWindow.StorageProvider;
                    var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                    var suggestedLocation = await storageProvider.TryGetFolderFromPathAsync(new Uri(downloadsPath));
                    
                    var file = await storageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
                    {
                        Title = "Load Variables from File",
                        FileTypeFilter = new[]
                        {
                            new Avalonia.Platform.Storage.FilePickerFileType("JSON files") { Patterns = new[] { "*.json" } },
                            new Avalonia.Platform.Storage.FilePickerFileType("All files") { Patterns = new[] { "*" } }
                        },
                        SuggestedStartLocation = suggestedLocation
                    });

                    if (file != null && file.Count > 0)
                    {
                        var fileName = file[0].Path.LocalPath;
                        var json = await File.ReadAllTextAsync(fileName);
                        var jsonOptions = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };

                        var loadedVariables = JsonSerializer.Deserialize<PhoneManagerVariables>(json, jsonOptions);
                        
                        if (loadedVariables != null)
                        {
                            Variables = loadedVariables;
                            _loggingService.Log($"Variables loaded from: {fileName}", LogLevel.Info);
                            await _errorHandlingService.ShowSuccess($"Variables loaded successfully from:\n{fileName}", "Load Successful");
                        }
                        else
                        {
                            await _errorHandlingService.HandleGenericError("Failed to load variables from the selected file.", "LoadVariables");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error loading variables: {ex.Message}", LogLevel.Error);
                await _errorHandlingService.HandleGenericError($"Error loading variables:\n{ex.Message}", "LoadVariables");
            }
        }

        [RelayCommand]
        private void OpenHolidayTimePicker()
        {
            // Set the current time as selected if it exists
            var currentTime = Variables.HolidayTime;
            // Round to nearest 15-minute increment if needed
            var roundedTime = new TimeSpan(currentTime.Hours, (currentTime.Minutes / 15) * 15, 0);
            SelectedHolidayTime = roundedTime;
            
            ShowHolidayTimePicker = true;
        }

        [RelayCommand]
        private void CloseHolidayTimePicker()
        {
            ShowHolidayTimePicker = false;
        }

        [RelayCommand]
        private void SaveHolidayTime()
        {
            if (SelectedHolidayTime.HasValue)
            {
                Variables.HolidayTime = SelectedHolidayTime.Value;
                _loggingService.Log($"Holiday time updated to: {SelectedHolidayTime.Value:hh\\:mm}", LogLevel.Info);
            }
            
            ShowHolidayTimePicker = false;
        }

        [RelayCommand]
        private void OpenHolidaySeriesManager()
        {
            // Save original state for cancel functionality
            var variables = _mainWindowViewModel?.Variables;
            if (variables != null)
            {
                OriginalHolidaySeries.Clear();
                foreach (var holiday in variables.HolidaySeries)
                {
                    OriginalHolidaySeries.Add(new HolidayEntry(holiday.Date, holiday.Time));
                }
            }
            
            ShowHolidaySeriesManager = true;
        }

        [ObservableProperty]
        private ObservableCollection<HolidayEntry> _originalHolidaySeries = new ObservableCollection<HolidayEntry>();

        [RelayCommand]
        private void CloseHolidaySeriesManager()
        {
            ShowHolidaySeriesManager = false;
            EditingHoliday = null;
        }

        [RelayCommand]
        private void CancelHolidaySeriesManager()
        {
            // Restore original state
            var variables = _mainWindowViewModel?.Variables;
            if (variables != null)
            {
                variables.HolidaySeries.Clear();
                foreach (var holiday in OriginalHolidaySeries)
                {
                    variables.HolidaySeries.Add(holiday);
                }
            }
            
            ShowHolidaySeriesManager = false;
            EditingHoliday = null;
        }

        [RelayCommand]
        private void AddHoliday()
        {
            try
            {
                // Reset to default values
                NewHolidayTime = new TimeSpan(0, 0, 0);
                EditingHoliday = null; // This is a new holiday, not editing existing
                
                // Set the selected time for the dialog (round to nearest 15-minute increment)
                var roundedTime = new TimeSpan(NewHolidayTime.Hours, (NewHolidayTime.Minutes / 15) * 15, 0);
                SelectedEditHolidayTime = roundedTime;
                
                // Open the edit dialog
                ShowEditHolidayDialog = true;
                
                _loggingService.Log("Add Holiday dialog opened", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error in AddHoliday: {ex.Message}", LogLevel.Error);
            }
        }

        // Predefined sets wizard state
        [ObservableProperty]
        private bool _showPredefinedHolidaysWizard = false;

        [ObservableProperty]
        private bool _showAargauInfoDialog = false;

        [ObservableProperty]
        private ObservableCollection<string> _countries = new ObservableCollection<string>(new[] { "Switzerland" });

        [ObservableProperty]
        private string? _selectedCountry = "Switzerland";

        [ObservableProperty]
        private ObservableCollection<string> _cantons = new ObservableCollection<string>(new[] { 
            "Aargau", "Basel-Land", "Bern", "Fribourg", "Genf", "Glarus", "Luzern", 
            "Schwyz", "Solothurn", "Tessin", "Thurgau", "Zug", "Zürich" 
        });

        [ObservableProperty]
        private string? _selectedCanton;

        [ObservableProperty]
        private ObservableCollection<string> _bezirke = new ObservableCollection<string>(new[] { 
            "Aarau (Brugg/Kulm/Lenzburg/Zofingen/Baden - nur Bergdietikon)", 
            "Baden (ohne Bergdietikon)", 
            "Bremgarten", 
            "Muri (Laufenburg, Rheinfelden - nur Hellikon, Mumpf, Obermumpf, Schupfart, Stein, Wegstetten)", 
            "Rheinfelden (nur Kaiseraugst, Magden, Möhlin, Olsberg, Rheinfelden, Wallbach, Zeiningen, Zuzgen)", 
            "Zurzach" 
        });

        [ObservableProperty]
        private string? _selectedBezirk;

        [ObservableProperty]
        private ObservableCollection<int> _years = new ObservableCollection<int>(Enumerable.Range(Math.Max(2025, DateTime.Now.Year), Math.Max(0, 2030 - Math.Max(2025, DateTime.Now.Year) + 1)));

        [ObservableProperty]
        private int _selectedYear = Math.Max(2025, Math.Min(2030, DateTime.Now.Year));

        public bool IsSwitzerlandSelected => string.Equals(SelectedCountry, "Switzerland", StringComparison.OrdinalIgnoreCase);
        public bool IsAargauSelected => string.Equals(SelectedCanton, "Aargau", StringComparison.OrdinalIgnoreCase);

        partial void OnSelectedCountryChanged(string? value)
        {
            OnPropertyChanged(nameof(IsSwitzerlandSelected));
            OnPropertyChanged(nameof(CanApplyPredefinedSelection));
        }

        partial void OnSelectedCantonChanged(string? value)
        {
            OnPropertyChanged(nameof(IsAargauSelected));
            OnPropertyChanged(nameof(CanApplyPredefinedSelection));
        }

        public bool CanApplyPredefinedSelection
        {
            get
            {
                if (!IsSwitzerlandSelected)
                    return SelectedYear >= 2025 && SelectedYear <= 2030;

                if (IsAargauSelected)
                    return !string.IsNullOrEmpty(SelectedBezirk) && SelectedYear >= 2025 && SelectedYear <= 2030;

                return !string.IsNullOrEmpty(SelectedCanton) && SelectedYear >= 2025 && SelectedYear <= 2030;
            }
        }

        partial void OnSelectedBezirkChanged(string? value)
        {
            OnPropertyChanged(nameof(CanApplyPredefinedSelection));
        }

        partial void OnSelectedYearChanged(int value)
        {
            OnPropertyChanged(nameof(CanApplyPredefinedSelection));
        }

        [RelayCommand]
        private void OpenPredefinedHolidaysWizard()
        {
            try
            {
                SelectedCountry = "Switzerland";
                SelectedCanton = null;
                SelectedBezirk = null;
                SelectedYear = Math.Max(2025, Math.Min(2030, DateTime.Now.Year));
                ShowPredefinedHolidaysWizard = true;
                _loggingService.Log("Opened Predefined Holidays wizard", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error opening wizard: {ex.Message}", LogLevel.Error);
            }
        }

        [RelayCommand]
        private void CancelPredefinedHolidaysWizard()
        {
            ShowPredefinedHolidaysWizard = false;
        }

        [RelayCommand]
        private void ShowAargauInfo()
        {
            try
            {
                ShowAargauInfoDialog = true;
                _loggingService.Log("Opened Aargau holiday reference info", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error opening Aargau info: {ex.Message}", LogLevel.Error);
            }
        }

        [RelayCommand]
        private void CloseAargauInfo()
        {
            ShowAargauInfoDialog = false;
        }

        [RelayCommand]
        private void ApplyPredefinedHolidays()
        {
            try
            {
                var variables = _mainWindowViewModel?.Variables;
                if (variables == null) return;

                // Use Swiss provider for Switzerland; expand later for other countries
                var provider = new teams_phonemanager.Services.Holidays.SwissHolidaysProvider();
                var holidays = provider.GetHolidays(SelectedCountry ?? "", SelectedCanton, SelectedBezirk, SelectedYear);

                foreach (var holiday in holidays)
                {
                    variables.HolidaySeries.Add(holiday);
                    _loggingService.Log($"Added predefined holiday: {holiday.DisplayText}", LogLevel.Info);
                }

                ShowPredefinedHolidaysWizard = false;
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error applying predefined holidays: {ex.Message}", LogLevel.Error);
            }
        }
        [RelayCommand]
        private void EditHoliday(HolidayEntry? holiday)
        {
            try
            {
                if (holiday != null)
                {
                    _loggingService.Log($"EditHoliday: Starting edit for holiday", LogLevel.Info);
                    EditingHoliday = holiday;
                    _loggingService.Log($"EditHoliday: Set EditingHoliday", LogLevel.Info);
                    
                    // Set the selected time, rounding to nearest 15-minute increment if needed
                    var roundedTime = new TimeSpan(holiday.Time.Hours, (holiday.Time.Minutes / 15) * 15, 0);
                    SelectedEditHolidayTime = roundedTime;
                    
                    ShowEditHolidayDialog = true;
                    _loggingService.Log($"EditHoliday: Set ShowEditHolidayDialog to true", LogLevel.Info);
                    _loggingService.Log($"Edit holiday requested: {holiday.DisplayText}", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error in EditHoliday: {ex.Message}", LogLevel.Error);
            }
        }

        [RelayCommand]
        private void CancelEditHoliday()
        {
            ShowEditHolidayDialog = false;
            EditingHoliday = null;
            SelectedEditHolidayTime = null;
        }

        [RelayCommand]
        private void SaveEditHoliday()
        {
            try
            {
                var variables = _mainWindowViewModel?.Variables;
                if (variables == null) return;

                // Get the selected time from the ComboBox
                TimeSpan selectedTime = SelectedEditHolidayTime ?? new TimeSpan(0, 0, 0);

                if (EditingHoliday != null)
                {
                    // Editing existing holiday
                    EditingHoliday.Time = selectedTime;
                    _loggingService.Log($"Updated holiday: {EditingHoliday.DisplayText}", LogLevel.Info);
                }
                else
                {
                    // Adding new holiday
                    var newHoliday = new HolidayEntry
                    {
                        Date = NewHolidayDate,
                        Time = selectedTime
                    };
                    variables.HolidaySeries.Add(newHoliday);
                    _loggingService.Log($"Added new holiday: {newHoliday.DisplayText}", LogLevel.Info);
                }
                
                ShowEditHolidayDialog = false;
                EditingHoliday = null;
                SelectedEditHolidayTime = null;
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error in SaveEditHoliday: {ex.Message}", LogLevel.Error);
            }
        }

        [RelayCommand]
        private void RemoveHoliday(HolidayEntry? holiday)
        {
            try
            {
                if (holiday != null)
                {
                    var variables = _mainWindowViewModel?.Variables;
                    if (variables != null)
                    {
                        variables.HolidaySeries.Remove(holiday);
                        _loggingService.Log($"Removed holiday: {holiday.DisplayText}", LogLevel.Info);
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error in RemoveHoliday: {ex.Message}", LogLevel.Error);
            }
        }

        [RelayCommand]
        private void DeleteAllHolidays()
        {
            try
            {
                var variables = _mainWindowViewModel?.Variables;
                if (variables != null && variables.HolidaySeries.Count > 0)
                {
                    var count = variables.HolidaySeries.Count;
                    variables.HolidaySeries.Clear();
                    _loggingService.Log($"Deleted all {count} holidays from series", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error in DeleteAllHolidays: {ex.Message}", LogLevel.Error);
            }
        }

        [RelayCommand]
        private void SaveHolidaySeries()
        {
            var variables = _mainWindowViewModel?.Variables;
            if (variables != null)
            {
                _loggingService.Log($"Saved holiday series with {variables.HolidaySeries.Count} holidays", LogLevel.Info);
                ShowHolidaySeriesManager = false;
            }
        }

        // Call Queue Configuration Properties
        public ObservableCollection<string> GreetingTypeOptions { get; } = new ObservableCollection<string>
        {
            "None",
            "AudioFile",
            "TextToSpeech"
        };

        public ObservableCollection<string> MusicOnHoldTypeOptions { get; } = new ObservableCollection<string>
        {
            "Default",
            "AudioFile"
        };

        public ObservableCollection<string> OverflowActionOptions { get; } = new ObservableCollection<string>
        {
            "Disconnect",
            "TransferToTarget",
            "TransferToVoicemail"
        };

        public ObservableCollection<string> TimeoutActionOptions { get; } = new ObservableCollection<string>
        {
            "Disconnect",
            "TransferToTarget",
            "TransferToVoicemail"
        };

        public ObservableCollection<string> NoAgentActionOptions { get; } = new ObservableCollection<string>
        {
            "QueueCall",
            "Disconnect",
            "TransferToTarget",
            "TransferToVoicemail"
        };

        public ObservableCollection<string> AaGreetingTypeOptions { get; } = new ObservableCollection<string>
        {
            "None",
            "AudioFile",
            "TextToSpeech"
        };

        public ObservableCollection<string> AaActionOptions { get; } = new ObservableCollection<string>
        {
            "Disconnect",
            "TransferToTarget",
            "TransferToVoicemail"
        };

        // Conditional visibility properties
        public bool ShowGreetingAudioFile => Variables.CqGreetingType == "AudioFile";
        public bool ShowGreetingTextToSpeech => Variables.CqGreetingType == "TextToSpeech";
        public bool ShowMusicOnHoldAudioFile => Variables.CqMusicOnHoldType == "AudioFile";
        public bool ShowOverflowTarget => Variables.CqOverflowAction == "TransferToTarget" || Variables.CqOverflowAction == "TransferToVoicemail";
        public bool ShowTimeoutTarget => Variables.CqTimeoutAction == "TransferToTarget" || Variables.CqTimeoutAction == "TransferToVoicemail";
        public bool ShowNoAgentTarget => Variables.CqNoAgentAction == "TransferToTarget" || Variables.CqNoAgentAction == "TransferToVoicemail";

        // AA Conditional visibility properties
        public bool ShowAaDefaultGreetingAudioFile => Variables.AaDefaultGreetingType == "AudioFile";
        public bool ShowAaDefaultGreetingTextToSpeech => Variables.AaDefaultGreetingType == "TextToSpeech";
        public bool ShowAaDefaultTarget => Variables.AaDefaultAction == "TransferToTarget" || Variables.AaDefaultAction == "TransferToVoicemail";

        public bool ShowAaAfterHoursGreetingAudioFile => Variables.AaAfterHoursGreetingType == "AudioFile";
        public bool ShowAaAfterHoursGreetingTextToSpeech => Variables.AaAfterHoursGreetingType == "TextToSpeech";
        public bool ShowAaAfterHoursTarget => Variables.AaAfterHoursAction == "TransferToTarget" || Variables.AaAfterHoursAction == "TransferToVoicemail";


        private void UpdateCallQueueVisibility()
        {
            OnPropertyChanged(nameof(ShowGreetingAudioFile));
            OnPropertyChanged(nameof(ShowGreetingTextToSpeech));
            OnPropertyChanged(nameof(ShowMusicOnHoldAudioFile));
            OnPropertyChanged(nameof(ShowOverflowTarget));
            OnPropertyChanged(nameof(ShowTimeoutTarget));
            OnPropertyChanged(nameof(ShowNoAgentTarget));
        }

        private void UpdateAutoAttendantVisibility()
        {
            OnPropertyChanged(nameof(ShowAaDefaultGreetingAudioFile));
            OnPropertyChanged(nameof(ShowAaDefaultGreetingTextToSpeech));
            OnPropertyChanged(nameof(ShowAaDefaultTarget));
            OnPropertyChanged(nameof(ShowAaAfterHoursGreetingAudioFile));
            OnPropertyChanged(nameof(ShowAaAfterHoursGreetingTextToSpeech));
            OnPropertyChanged(nameof(ShowAaAfterHoursTarget));
        }



        // Call Queue Configuration Dialog
        [ObservableProperty]
        private bool _showCallQueueConfigurationDialog = false;

        [RelayCommand]
        private void OpenCallQueueConfigurationDialog()
        {
            ShowCallQueueConfigurationDialog = true;
        }

        [RelayCommand]
        private void CancelCallQueueConfiguration()
        {
            ShowCallQueueConfigurationDialog = false;
        }

        [RelayCommand]
        private void SaveCallQueueConfiguration()
        {
            _loggingService.Log("Call Queue configuration saved", LogLevel.Info);
            ShowCallQueueConfigurationDialog = false;
        }

        [RelayCommand]
        private async Task SelectGreetingAudioFile()
        {
            await SelectAndImportAudioFile(
                audioFileId => Variables.CqGreetingAudioFileId = audioFileId,
                "Greeting");
        }

        [RelayCommand]
        private async Task SelectMusicOnHoldAudioFile()
        {
            await SelectAndImportAudioFile(
                audioFileId => Variables.CqMusicOnHoldAudioFileId = audioFileId,
                "Music on Hold");
        }

        // Auto Attendant Configuration Dialog
        [ObservableProperty]
        private bool _showAutoAttendantConfigurationDialog = false;

        [RelayCommand]
        private void OpenAutoAttendantConfigurationDialog()
        {
            ShowAutoAttendantConfigurationDialog = true;
        }

        [RelayCommand]
        private void CancelAutoAttendantConfiguration()
        {
            ShowAutoAttendantConfigurationDialog = false;
        }

        [RelayCommand]
        private void SaveAutoAttendantConfiguration()
        {
            _loggingService.Log("Auto Attendant configuration saved", LogLevel.Info);
            ShowAutoAttendantConfigurationDialog = false;
        }

        [RelayCommand]
        private async Task SelectAaDefaultGreetingAudioFile()
        {
            await SelectAndImportAudioFile(
                audioFileId => Variables.AaDefaultGreetingAudioFileId = audioFileId,
                "AA Default Greeting");
        }

        [RelayCommand]
        private async Task SelectAaAfterHoursGreetingAudioFile()
        {
            await SelectAndImportAudioFile(
                audioFileId => Variables.AaAfterHoursGreetingAudioFileId = audioFileId,
                "AA After Hours Greeting");
        }


        private async Task SelectAndImportAudioFile(Action<string> setAudioFileId, string context)
        {
            try
            {
                var window = Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
                if (window?.MainWindow != null)
                {
                    var storageProvider = window.MainWindow.StorageProvider;
                    
                    var file = await storageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
                    {
                        Title = $"Select Audio File for {context}",
                        FileTypeFilter = new[]
                        {
                            new Avalonia.Platform.Storage.FilePickerFileType("WAV files") { Patterns = new[] { "*.wav" } },
                            new Avalonia.Platform.Storage.FilePickerFileType("All files") { Patterns = new[] { "*" } }
                        }
                    });

                    if (file != null && file.Count > 0)
                    {
                        var filePath = file[0].Path.LocalPath;
                        var fileInfo = new FileInfo(filePath);
                        
                        // Validate file size (max 5MB)
                        if (fileInfo.Length > 5 * 1024 * 1024)
                        {
                            await _errorHandlingService.HandleGenericError("Audio file must be 5MB or smaller.", "File Size Error");
                            return;
                        }

                        // Validate file extension
                        if (!filePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                        {
                            await _errorHandlingService.HandleGenericError("Only WAV files are supported.", "File Type Error");
                            return;
                        }

                        _loggingService.Log($"Importing audio file: {filePath}", LogLevel.Info);
                        
                        // Import the audio file via PowerShell
                        var command = _powerShellCommandService.GetImportAudioFileCommand(filePath);
                        var result = await ExecutePowerShellCommandAsync(command, "ImportAudioFile");
                        
                        if (!string.IsNullOrEmpty(result) && result.Contains("SUCCESS"))
                        {
                            // Parse the audio file ID from the result
                            var audioFileId = ParseAudioFileIdFromResult(result);
                            if (!string.IsNullOrEmpty(audioFileId))
                            {
                                var fileName = fileInfo.Name;
                                setAudioFileId(audioFileId);
                                _loggingService.Log($"Audio file imported successfully. ID: {audioFileId}, File: {fileName}", LogLevel.Info);
                                await _errorHandlingService.ShowSuccess($"Audio file imported successfully.\nFile: {fileName}\nID: {audioFileId}", "Import Successful");
                            }
                            else
                            {
                                await _errorHandlingService.HandleGenericError("Failed to parse audio file ID from result.", "Import Error");
                            }
                        }
                        else
                        {
                            await _errorHandlingService.HandleGenericError($"Failed to import audio file:\n{result}", "Import Error");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error importing audio file: {ex.Message}", LogLevel.Error);
                await _errorHandlingService.HandleGenericError($"Error importing audio file:\n{ex.Message}", "Import Error");
            }
        }

        private string? ParseAudioFileIdFromResult(string result)
        {
            // Look for pattern like "AUDIOFILEID: {guid}" in the result
            var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.Contains("AUDIOFILEID:") && line.Length > 13)
                {
                    var id = line.Substring(13).Trim();
                    if (Guid.TryParse(id, out _))
                    {
                        return id;
                    }
                }
            }
            return null;
        }
    }
} 
