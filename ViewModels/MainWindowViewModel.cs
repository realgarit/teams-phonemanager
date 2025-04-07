using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using teams_phonemanager.Services;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Collections;
using System.Text;
using System.Linq;
using System.Windows.Input;
using System;

namespace teams_phonemanager.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly LoggingService _loggingService;
        private readonly PaletteHelper _paletteHelper;

        public ObservableCollection<string> LogEntries => _loggingService.LogEntries;
        public string LatestLogEntry => _loggingService.LatestLogEntry;

        [ObservableProperty]
        private bool _isDarkTheme;

        [ObservableProperty]
        private string _currentPage = "Welcome";

        [ObservableProperty]
        private bool _isSettingsOpen;

        [ObservableProperty]
        private bool _isLogExpanded;

        [ObservableProperty]
        private string _version = "Version 1.7.16";

        public MainWindowViewModel()
        {
            _loggingService = LoggingService.Instance;
            _paletteHelper = new PaletteHelper();
            
            // Initialize theme based on current system theme
            var theme = _paletteHelper.GetTheme();
            IsDarkTheme = theme.GetBaseTheme() == BaseTheme.Dark;
            
            _loggingService.Log("Application started", LogLevel.Info);

            // Subscribe to logging service property changes
            _loggingService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(LoggingService.LatestLogEntry))
                {
                    OnPropertyChanged(nameof(LatestLogEntry));
                }
            };
        }

        partial void OnIsDarkThemeChanged(bool value)
        {
            var theme = _paletteHelper.GetTheme();
            theme.SetBaseTheme(value ? Theme.Dark : Theme.Light);
            _paletteHelper.SetTheme(theme);
            _loggingService.Log($"Theme changed to {(value ? "Dark" : "Light")}", LogLevel.Info);
        }

        [RelayCommand]
        private void NavigateTo(string page)
        {
            CurrentPage = page;
            _loggingService.Log($"Navigated to {page} page", LogLevel.Info);
        }

        [RelayCommand]
        private void ToggleSettings()
        {
            IsSettingsOpen = !IsSettingsOpen;
            _loggingService.Log($"Settings panel {(IsSettingsOpen ? "opened" : "closed")}", LogLevel.Info);
        }

        [RelayCommand]
        private void CloseSettings()
        {
            IsSettingsOpen = false;
            _loggingService.Log("Settings panel closed", LogLevel.Info);
        }

        [RelayCommand]
        private void ClearLog()
        {
            _loggingService.Clear();
            _loggingService.Log("Log cleared", LogLevel.Info);
        }
    }
} 