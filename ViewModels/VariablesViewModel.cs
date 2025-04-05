using CommunityToolkit.Mvvm.ComponentModel;
using teams_phonemanager.Services;

namespace teams_phonemanager.ViewModels
{
    public partial class VariablesViewModel : ViewModelBase
    {
        private readonly LoggingService _loggingService;

        public VariablesViewModel()
        {
            _loggingService = LoggingService.Instance;
            _loggingService.Log("Variables page loaded", LogLevel.Info);
        }
    }
} 