using System;
using System.Collections.ObjectModel;

namespace teams_phonemanager.Helpers
{
    /// <summary>
    /// Provides reusable time options for 15-minute increments throughout the day.
    /// This is a static provider to avoid duplicating time generation logic.
    /// </summary>
    public static class TimeOptionsProvider
    {
        private static readonly Lazy<ObservableCollection<TimeSpan>> _timeOptions = 
            new Lazy<ObservableCollection<TimeSpan>>(() => GenerateTimeOptions());

        /// <summary>
        /// Gets all time options in 15-minute increments from 00:00 to 23:45.
        /// Returns a cached, shared instance for performance.
        /// </summary>
        public static ObservableCollection<TimeSpan> TimeOptions => _timeOptions.Value;

        /// <summary>
        /// Generates all time options for the day in 15-minute increments.
        /// </summary>
        private static ObservableCollection<TimeSpan> GenerateTimeOptions()
        {
            var times = new ObservableCollection<TimeSpan>();
            
            for (int hour = 0; hour < 24; hour++)
            {
                for (int minute = 0; minute < 60; minute += 15)
                {
                    times.Add(new TimeSpan(hour, minute, 0));
                }
            }
            
            return times;
        }
    }
}
