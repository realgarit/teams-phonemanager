using CommunityToolkit.Mvvm.ComponentModel;
using teams_phonemanager.ViewModels;

namespace teams_phonemanager.Services
{
    public class NavigationService : ObservableObject
    {
        private static NavigationService? _instance;
        private string _currentPage = ConstantsService.Pages.Welcome;

        private NavigationService()
        {
            LoggingService.Instance.Log("Navigation service initialized", LogLevel.Info);
        }

        public static NavigationService Instance
        {
            get
            {
                _instance ??= new NavigationService();
                return _instance;
            }
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
                    LoggingService.Instance.Log($"Navigated to {value} page", LogLevel.Info);
                }
            }
        }

        public void NavigateTo(string page)
        {
            CurrentPage = page;
        }
    }
}