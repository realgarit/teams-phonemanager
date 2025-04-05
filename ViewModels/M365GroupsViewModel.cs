using CommunityToolkit.Mvvm.ComponentModel;
using teams_phonemanager.Services;

namespace teams_phonemanager.ViewModels
{
    public partial class M365GroupsViewModel : ViewModelBase
    {
        private readonly LoggingService _loggingService;

        public M365GroupsViewModel()
        {
            _loggingService = LoggingService.Instance;
            _loggingService.Log("M365 Groups page loaded", LogLevel.Info);
        }
    }
} 