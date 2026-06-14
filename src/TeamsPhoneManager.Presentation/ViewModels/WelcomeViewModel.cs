using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using teams_phonemanager.Services;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.ViewModels
{
    public partial class WelcomeViewModel : ViewModelBase
    {
        public WelcomeViewModel(
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
                    FileName = "https://github.com/realgarit/teams-phonemanager",
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
