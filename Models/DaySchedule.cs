using CommunityToolkit.Mvvm.ComponentModel;

namespace teams_phonemanager.Models
{
    /// <summary>
    /// Represents business hours configuration for a single day of the week.
    /// Supports two time ranges per day (e.g., morning and afternoon shifts).
    /// </summary>
    public partial class DaySchedule : ObservableObject
    {
        [ObservableProperty]
        private string _dayName = string.Empty;

        [ObservableProperty]
        private bool _isEnabled = true;

        [ObservableProperty]
        private TimeSpan _hours1Start = new TimeSpan(8, 0, 0);

        [ObservableProperty]
        private TimeSpan _hours1End = new TimeSpan(12, 0, 0);

        [ObservableProperty]
        private TimeSpan _hours2Start = new TimeSpan(13, 0, 0);

        [ObservableProperty]
        private TimeSpan _hours2End = new TimeSpan(17, 0, 0);

        [ObservableProperty]
        private bool _hasSecondRange = false;

        public DaySchedule() { }

        public DaySchedule(string dayName, bool isEnabled = true)
        {
            DayName = dayName;
            IsEnabled = isEnabled;
        }

        /// <summary>
        /// Returns true if the second time range is different from 00:00-00:00 (indicating it's actually configured).
        /// </summary>
        public bool HasEffectiveSecondRange => HasSecondRange && Hours2Start != Hours2End;
    }
}
