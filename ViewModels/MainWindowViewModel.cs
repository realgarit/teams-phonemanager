using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using teams_phonemanager.Services;
using System.Collections.ObjectModel;
using System.Collections;
using System.Text;
using System.Linq;
using System;
using teams_phonemanager.Models;
using FluentAvalonia.Styling;

namespace teams_phonemanager.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<string> LogEntries => _loggingService.LogEntries;
        public string LatestLogEntry => _loggingService.LatestLogEntry;

        public string AllLogEntriesText
        {
            get
            {
                return string.Join(Environment.NewLine, LogEntries);
            }
        }

        [ObservableProperty]
        private bool _isDarkTheme = true;

        [ObservableProperty]
        private string _currentPage = ConstantsService.Pages.Welcome;

        [ObservableProperty]
        private bool _isSettingsOpen;

        [ObservableProperty]
        private bool _isLogDialogOpen;

        [ObservableProperty]
        private string _version = ConstantsService.Application.Version;
        
        [ObservableProperty]
        private PhoneManagerVariables _variables = new();

        public MainWindowViewModel()
        {
            _loggingService.Log("Application started", LogLevel.Info);

            _loggingService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(LoggingService.LatestLogEntry))
                {
                    OnPropertyChanged(nameof(LatestLogEntry));
                }
            };

            LogEntries.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(AllLogEntriesText));
            };

            _navigationService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(NavigationService.CurrentPage))
                {
                    CurrentPage = _navigationService.CurrentPage;
                }
            };
        }

        partial void OnIsDarkThemeChanged(bool value)
        {
            // Update FluentAvalonia theme
            var app = Avalonia.Application.Current;
            if (app != null)
            {
                var faTheme = app.RequestedThemeVariant;
                app.RequestedThemeVariant = value ? Avalonia.Styling.ThemeVariant.Dark : Avalonia.Styling.ThemeVariant.Light;
            }
            _loggingService.Log($"Theme changed to {(value ? "Dark" : "Light")}", LogLevel.Info);
        }

        [RelayCommand]
        public new void NavigateTo(string page)
        {
            _navigationService.NavigateTo(page);
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

        [RelayCommand]
        private void ToggleLogDialog()
        {
            IsLogDialogOpen = !IsLogDialogOpen;
            _loggingService.Log($"Log viewer {(IsLogDialogOpen ? "opened" : "closed")}", LogLevel.Info);
        }

        [RelayCommand]
        private void CloseLogDialog()
        {
            IsLogDialogOpen = false;
            _loggingService.Log("Log viewer closed", LogLevel.Info);
        }
    }
} 
