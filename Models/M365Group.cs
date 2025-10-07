using CommunityToolkit.Mvvm.ComponentModel;

namespace teams_phonemanager.Models
{
    public partial class M365Group : ObservableObject
    {
        [ObservableProperty]
        private string _displayName = string.Empty;

        [ObservableProperty]
        private string _id = string.Empty;

        [ObservableProperty]
        private string _mailNickname = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private DateTime _createdDateTime = DateTime.MinValue;

        [ObservableProperty]
        private bool _isSelected;

        public M365Group()
        {
        }

        public M365Group(string displayName, string id, string mailNickname, string description)
        {
            DisplayName = displayName;
            Id = id;
            MailNickname = mailNickname;
            Description = description;
        }
    }
}
