using CommunityToolkit.Mvvm.ComponentModel;
using teams_phonemanager.Services;

namespace teams_phonemanager.ViewModels
{
    public partial class AutoAttendantsViewModel : ViewModelBase
    {
        public AutoAttendantsViewModel()
        {
            _loggingService.Log("Auto Attendants page loaded", LogLevel.Info);
        }
    }
} 
