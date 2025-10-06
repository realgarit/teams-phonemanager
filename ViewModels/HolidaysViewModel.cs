using CommunityToolkit.Mvvm.ComponentModel;
using teams_phonemanager.Services;

namespace teams_phonemanager.ViewModels
{
    public partial class HolidaysViewModel : ViewModelBase
    {
        public HolidaysViewModel()
        {
            _loggingService.Log("Holidays page loaded", LogLevel.Info);
        }
    }
} 
