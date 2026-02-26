using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using teams_phonemanager.Services;
using teams_phonemanager.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Collections;
using System.Text;
using System.Linq;
using System;
using teams_phonemanager.Models;
using FluentAvalonia.Styling;
using System.Collections.Specialized;
using System.ComponentModel;

namespace teams_phonemanager.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private const int MaxLogEntries = 1000;
        private bool _disposed = false;
        private string? _allLogEntriesTextCache;
        private bool _logCacheDirty = true;

        private PropertyChangedEventHandler? _loggingPropertyHandler;
        private NotifyCollectionChangedEventHandler? _logEntriesHandler;
        private PropertyChangedEventHandler? _navigationPropertyHandler;

        public ObservableCollection<string> LogEntries => _loggingService.LogEntries;
        public string LatestLogEntry => _loggingService.LatestLogEntry;

        public string AllLogEntriesText
        {
            get
            {
                if (_logCacheDirty || _allLogEntriesTextCache == null)
                {
                    var sb = new StringBuilder();
                    foreach (var entry in LogEntries)
                    {
                        sb.AppendLine(entry);
                    }
                    _allLogEntriesTextCache = sb.ToString();
                    _logCacheDirty = false;
                }
                return _allLogEntriesTextCache;
            }
        }

        [ObservableProperty]
        private bool _isDarkTheme = true;

        [ObservableProperty]
        private string _currentPage = Services.ConstantsService.Pages.Welcome;

        [ObservableProperty]
        private bool _isSettingsOpen;

        [ObservableProperty]
        private bool _isLogDialogOpen;

        [ObservableProperty]
        private string _version = Services.ConstantsService.Application.Version;

        public string Copyright => Services.ConstantsService.Application.Copyright;

        [ObservableProperty]
        private PhoneManagerVariables _variables = new();

        public MainWindowViewModel(
            IPowerShellContextService powerShellContextService,
            IPowerShellCommandService powerShellCommandService,
            ILoggingService loggingService,
            ISessionManager sessionManager,
            INavigationService navigationService,
            IErrorHandlingService errorHandlingService,
            IValidationService validationService)
            : base(powerShellContextService, powerShellCommandService, loggingService,
                  sessionManager, navigationService, errorHandlingService, validationService)
        {
            _loggingService.Log("Application started", LogLevel.Info);

            _loggingPropertyHandler = (s, e) =>
            {
                if (e.PropertyName == nameof(ILoggingService.LatestLogEntry))
                {
                    OnPropertyChanged(nameof(LatestLogEntry));
                }
            };
            _loggingService.PropertyChanged += _loggingPropertyHandler;

            _logEntriesHandler = (s, e) =>
            {
                _logCacheDirty = true;

                // Limit log size
                while (LogEntries.Count > MaxLogEntries)
                {
                    LogEntries.RemoveAt(0);
                }

                OnPropertyChanged(nameof(AllLogEntriesText));
            };
            LogEntries.CollectionChanged += _logEntriesHandler;

            _navigationPropertyHandler = (s, e) =>
            {
                if (e.PropertyName == nameof(INavigationService.CurrentPage))
                {
                    CurrentPage = _navigationService.CurrentPage;
                }
            };
            _navigationService.PropertyChanged += _navigationPropertyHandler;
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

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_loggingPropertyHandler != null)
                    _loggingService.PropertyChanged -= _loggingPropertyHandler;

                if (_logEntriesHandler != null)
                    LogEntries.CollectionChanged -= _logEntriesHandler;

                if (_navigationPropertyHandler != null)
                    _navigationService.PropertyChanged -= _navigationPropertyHandler;

                _disposed = true;
            }
        }
    }
} 
