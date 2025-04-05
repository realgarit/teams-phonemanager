using CommunityToolkit.Mvvm.ComponentModel;
using teams_phonemanager.Services;

namespace teams_phonemanager.ViewModels
{
    public partial class AutoAttendantsViewModel : ViewModelBase
    {
        private readonly LoggingService _loggingService;

        public AutoAttendantsViewModel()
        {
            _loggingService = LoggingService.Instance;
            _loggingService.Log("Auto Attendants page loaded", LogLevel.Info);
        }
    }
} 