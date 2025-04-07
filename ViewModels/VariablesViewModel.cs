using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using teams_phonemanager.Services;
using System;
using System.Threading.Tasks;
using teams_phonemanager.Models;

namespace teams_phonemanager.ViewModels
{
    public partial class VariablesViewModel : ViewModelBase
    {
        private readonly PowerShellService _powerShellService;
        private readonly LoggingService _loggingService;
        private readonly SessionManager _sessionManager;
        private readonly MainWindowViewModel? _mainWindowViewModel;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome to the Variables page. Here you can set the variables that will be used throughout the application.";

        [ObservableProperty]
        private bool _teamsConnected;

        [ObservableProperty]
        private bool _graphConnected;

        public VariablesViewModel()
        {
            _powerShellService = PowerShellService.Instance;
            _loggingService = LoggingService.Instance;
            _sessionManager = SessionManager.Instance;
            _mainWindowViewModel = Application.Current.MainWindow.DataContext as MainWindowViewModel;

            TeamsConnected = _sessionManager.TeamsConnected;
            GraphConnected = _sessionManager.GraphConnected;

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

        public bool CanProceed => TeamsConnected && GraphConnected;
    }
} 