using CommunityToolkit.Mvvm.ComponentModel;

namespace teams_phonemanager.Models
{
    public partial class AutoAttendant : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _identity = string.Empty;

        [ObservableProperty]
        private string _languageId = string.Empty;

        [ObservableProperty]
        private string _timeZoneId = string.Empty;

        [ObservableProperty]
        private bool _isSelected;

        public AutoAttendant()
        {
        }

        public AutoAttendant(string name, string identity, string languageId, string timeZoneId)
        {
            Name = name;
            Identity = identity;
            LanguageId = languageId;
            TimeZoneId = timeZoneId;
        }
    }
}
