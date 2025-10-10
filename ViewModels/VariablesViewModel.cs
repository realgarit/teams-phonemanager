using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using teams_phonemanager.Services;
using System;
using System.Threading.Tasks;
using teams_phonemanager.Models;
using System.IO;
using System.Text.Json;
using Microsoft.Win32;
using System.Collections.ObjectModel;

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

        [ObservableProperty]
        private object? _selectedHolidayTime;

        [ObservableProperty]
        private bool _showHolidaySeriesManager = false;

        [ObservableProperty]
        private HolidayEntry? _editingHoliday;

        [ObservableProperty]
        private bool _showEditHolidayDialog = false;

        [ObservableProperty]
        private object? _selectedEditHolidayTime;

        // Simple properties for Add/Edit dialogs
        [ObservableProperty]
        private DateTime _newHolidayDate = DateTime.Now;

        [ObservableProperty]
        private TimeSpan _newHolidayTime = new TimeSpan(0, 0, 0);

        [ObservableProperty]
        private TimeSpan _editHolidayTime = new TimeSpan(0, 0, 0);

        // Computed properties for the dialog
        public DateTime DialogHolidayDate
        {
            get => EditingHoliday?.Date ?? NewHolidayDate;
            set
            {
                if (EditingHoliday != null)
                    EditingHoliday.Date = value;
                else
                    NewHolidayDate = value;
            }
        }

        // Removed DialogHolidayTime - now using SelectedEditHolidayTime with ComboBoxItem approach

        public VariablesViewModel()
        {
            _mainWindowViewModel = Application.Current.MainWindow.DataContext as MainWindowViewModel;

            _loggingService.Log("Variables page loaded", LogLevel.Info);
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
                
                MessageBox.Show(
                    $"Variables saved successfully to:\n{filePath}",
                    "Save Successful",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error saving variables: {ex.Message}", LogLevel.Error);
                MessageBox.Show(
                    $"Error saving variables:\n{ex.Message}",
                    "Save Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task LoadVariablesFromFileAsync()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Load Variables from File",
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var json = await File.ReadAllTextAsync(openFileDialog.FileName);
                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    var loadedVariables = JsonSerializer.Deserialize<PhoneManagerVariables>(json, jsonOptions);
                    
                    if (loadedVariables != null)
                    {
                        Variables = loadedVariables;
                        _loggingService.Log($"Variables loaded from: {openFileDialog.FileName}", LogLevel.Info);
                        
                        MessageBox.Show(
                            $"Variables loaded successfully from:\n{openFileDialog.FileName}",
                            "Load Successful",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            "Failed to load variables from the selected file.",
                            "Load Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error loading variables: {ex.Message}", LogLevel.Error);
                MessageBox.Show(
                    $"Error loading variables:\n{ex.Message}",
                    "Load Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void OpenHolidayTimePicker()
        {
            // Set the current time as selected if it exists
            var currentTime = Variables.HolidayTime;
            var timeString = currentTime.ToString(@"hh\:mm");
            
            // Find the corresponding ComboBoxItem
            foreach (var item in GetTimeOptions())
            {
                if (item.Tag?.ToString() == timeString)
                {
                    SelectedHolidayTime = item;
                    break;
                }
            }
            
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
            if (SelectedHolidayTime is System.Windows.Controls.ComboBoxItem selectedItem && 
                selectedItem.Tag is string timeString)
            {
                if (TimeSpan.TryParse(timeString, out var timeSpan))
                {
                    Variables.HolidayTime = timeSpan;
                    _loggingService.Log($"Holiday time updated to: {timeString}", LogLevel.Info);
                }
            }
            
            ShowHolidayTimePicker = false;
        }

        private System.Collections.Generic.List<System.Windows.Controls.ComboBoxItem> GetTimeOptions()
        {
            var times = new System.Collections.Generic.List<System.Windows.Controls.ComboBoxItem>();
            
            for (int hour = 0; hour < 24; hour++)
            {
                for (int minute = 0; minute < 60; minute += 15)
                {
                    var timeString = $"{hour:D2}:{minute:D2}";
                    var item = new System.Windows.Controls.ComboBoxItem
                    {
                        Content = timeString,
                        Tag = timeString
                    };
                    times.Add(item);
                }
            }
            
            return times;
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
                
                // Set the selected time for the dialog
                SetSelectedTimeForEdit(NewHolidayTime);
                
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
                    
                    // Set the selected ComboBoxItem for the time
                    SetSelectedTimeForEdit(holiday.Time);
                    
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
                TimeSpan selectedTime = new TimeSpan(0, 0, 0); // Default
                if (SelectedEditHolidayTime is System.Windows.Controls.ComboBoxItem selectedItem && 
                    selectedItem.Tag is string timeString)
                {
                    if (TimeSpan.TryParse(timeString, out var timeSpan))
                    {
                        selectedTime = timeSpan;
                    }
                }

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

        private void SetSelectedTimeForEdit(TimeSpan time)
        {
            try
            {
                var timeString = time.ToString(@"hh\:mm");
                
                // Find the corresponding ComboBoxItem
                foreach (var item in GetTimeOptions())
                {
                    if (item.Tag?.ToString() == timeString)
                    {
                        SelectedEditHolidayTime = item;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error in SetSelectedTimeForEdit: {ex.Message}", LogLevel.Error);
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
    }
} 
