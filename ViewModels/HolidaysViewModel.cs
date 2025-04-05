using CommunityToolkit.Mvvm.ComponentModel;
using teams_phonemanager.Services;

namespace teams_phonemanager.ViewModels
{
    public partial class HolidaysViewModel : ViewModelBase
    {
        private readonly LoggingService _loggingService;

        public HolidaysViewModel()
        {
            _loggingService = LoggingService.Instance;
            _loggingService.Log("Holidays page loaded", LogLevel.Info);
        }
    }
} 