using CommunityToolkit.Mvvm.ComponentModel;

namespace teams_phonemanager.Models
{
    public partial class HolidayEntry : ObservableObject
    {
        [ObservableProperty]
        private DateTime _date = DateTime.Now;

        [ObservableProperty]
        private TimeSpan _time = new TimeSpan(9, 0, 0); // Default to 9:00 AM

        public HolidayEntry()
        {
        }

        public HolidayEntry(DateTime date, TimeSpan time)
        {
            Date = date;
            Time = time;
        }

        public DateTime DateTime => Date.Date.Add(Time);

        public string DisplayText => $"{Date:dd.MM.yyyy} {Time:hh\\:mm}";
    }
}
