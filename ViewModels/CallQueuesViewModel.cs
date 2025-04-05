using CommunityToolkit.Mvvm.ComponentModel;
using teams_phonemanager.Services;

namespace teams_phonemanager.ViewModels
{
    public partial class CallQueuesViewModel : ViewModelBase
    {
        private readonly LoggingService _loggingService;

        public CallQueuesViewModel()
        {
            _loggingService = LoggingService.Instance;
            _loggingService.Log("Call Queues page loaded", LogLevel.Info);
        }
    }
} 