using CommunityToolkit.Mvvm.ComponentModel;

namespace teams_phonemanager.Models
{
    public partial class ResourceAccount : ObservableObject
    {
        [ObservableProperty]
        private string _displayName = string.Empty;

        [ObservableProperty]
        private string _userPrincipalName = string.Empty;

        [ObservableProperty]
        private string _identity = string.Empty;

        [ObservableProperty]
        private string _usageLocation = string.Empty;

        [ObservableProperty]
        private bool _isSelected;

        public ResourceAccount()
        {
        }

        public ResourceAccount(string displayName, string userPrincipalName, string identity, string usageLocation)
        {
            DisplayName = displayName;
            UserPrincipalName = userPrincipalName;
            Identity = identity;
            UsageLocation = usageLocation;
        }
    }
}
