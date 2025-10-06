using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using teams_phonemanager.Services;

namespace teams_phonemanager.ViewModels
{
    public partial class WelcomeViewModel : ViewModelBase
    {
        public WelcomeViewModel()
        {
            _loggingService.Log("Welcome page loaded", LogLevel.Info);
        }

        [ObservableProperty]
        private string _welcomeMessage = "Welcome to Teams Phone Manager! This application will help you manage your Microsoft Teams phone system configuration.";

        [RelayCommand]
        private new void NavigateToGetStarted()
        {
            NavigateTo("GetStarted");
        }

        [RelayCommand]
        private void OpenDocumentation()
        {
            try
            {
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
