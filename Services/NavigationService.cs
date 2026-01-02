using CommunityToolkit.Mvvm.ComponentModel;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Services
{
    public class NavigationService : ObservableObject, INavigationService
    {
        private readonly ILoggingService _loggingService;
        private string _currentPage = ConstantsService.Pages.Welcome;

        public NavigationService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _loggingService.Log("Navigation service initialized", LogLevel.Info);
        }

        public string CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged();
                    _loggingService.Log($"Navigated to {value} page", LogLevel.Info);
                }
            }
        }

        public void NavigateTo(string page)
        {
            CurrentPage = page;
        }
    }
}