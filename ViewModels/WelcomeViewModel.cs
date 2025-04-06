using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using teams_phonemanager.Services;

namespace teams_phonemanager.ViewModels
{
    public partial class WelcomeViewModel : ViewModelBase
    {
        private readonly LoggingService _loggingService;
        private readonly MainWindowViewModel? _mainWindowViewModel;

        public WelcomeViewModel()
        {
            _loggingService = LoggingService.Instance;
            
            if (App.Current?.MainWindow?.DataContext is MainWindowViewModel mainViewModel)
            {
                _mainWindowViewModel = mainViewModel;
            }
            else
            {
                _loggingService.Log("Failed to get MainWindowViewModel reference", LogLevel.Warning);
            }
            
            _loggingService.Log("Welcome page loaded", LogLevel.Info);
        }

        [ObservableProperty]
        private string _welcomeMessage = "Welcome to Teams Phone Manager! This application will help you manage your Microsoft Teams phone system configuration.";

        [RelayCommand]
        private void NavigateToGetStarted()
        {
            if (_mainWindowViewModel?.NavigateToCommand != null)
            {
                _mainWindowViewModel.NavigateToCommand.Execute("GetStarted");
                _loggingService.Log("Navigating to Get Started page", LogLevel.Info);
            }
            else
            {
                _loggingService.Log("Navigation failed: MainWindowViewModel not available", LogLevel.Error);
            }
        }

        [RelayCommand]
        private void OpenDocumentation()
        {
            try
            {
                // Replace this URL with your actual documentation URL
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/yourusername/teams-phonemanager/wiki",
                    UseShellExecute = true
                });
                _loggingService.Log("Opening documentation in browser", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Failed to open documentation: {ex.Message}", LogLevel.Error);
            }
        }
    }
} 