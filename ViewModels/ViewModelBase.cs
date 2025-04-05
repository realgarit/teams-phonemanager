using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace teams_phonemanager.ViewModels
{
    public partial class ViewModelBase : ObservableObject
    {
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private string _logMessage = string.Empty;

        protected void UpdateStatus(string message)
        {
            StatusMessage = message;
            LogMessage = $"{DateTime.Now:HH:mm:ss} - {message}";
        }
    }
} 