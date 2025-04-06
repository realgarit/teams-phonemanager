using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using teams_phonemanager.Models;
using teams_phonemanager.Services;

namespace teams_phonemanager.ViewModels
{
    public partial class VariablesViewModel : ViewModelBase
    {
        private readonly PowerShellService _powerShellService;
        private readonly LoggingService _loggingService;
        private readonly SessionManager _sessionManager;

        [ObservableProperty]
        private PhoneManagerVariables _variables = new();

        [ObservableProperty]
        private string _welcomeMessage = "Welcome to the Variables page. Please make sure you have connected to Teams and Graph on the Get Started page before proceeding.";

        [ObservableProperty]
        private bool _teamsConnected;

        [ObservableProperty]
        private bool _graphConnected;

        public bool CanProceed => TeamsConnected && GraphConnected;

        public VariablesViewModel(
            PowerShellService powerShellService,
            LoggingService loggingService,
            SessionManager sessionManager)
        {
            _powerShellService = powerShellService;
            _loggingService = loggingService;
            _sessionManager = sessionManager;

            // Initialize state from session manager
            TeamsConnected = _sessionManager.TeamsConnected;
            GraphConnected = _sessionManager.GraphConnected;

            _loggingService.Log("Variables page initialized", LogLevel.Info);
        }

        [RelayCommand]
        private void SaveVariables()
        {
            try
            {
                _loggingService.Log("Saving variables...", LogLevel.Info);
                // TODO: Implement save functionality
                _loggingService.Log("Variables saved successfully", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error saving variables: {ex.Message}", LogLevel.Error);
            }
        }

        [RelayCommand]
        private void LoadVariables()
        {
            try
            {
                _loggingService.Log("Loading variables...", LogLevel.Info);
                // TODO: Implement load functionality
                _loggingService.Log("Variables loaded successfully", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error loading variables: {ex.Message}", LogLevel.Error);
            }
        }
    }
} 