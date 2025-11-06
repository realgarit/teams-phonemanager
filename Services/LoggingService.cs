using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace teams_phonemanager.Services
{
    public partial class LoggingService : ObservableObject
    {
        private static LoggingService? _instance;
        
        [ObservableProperty]
        private ObservableCollection<string> _logEntries = new();

        [ObservableProperty]
        private string _latestLogEntry = string.Empty;

        private LoggingService() { }

        public static LoggingService Instance
        {
            get
            {
                _instance ??= new LoggingService();
                return _instance;
            }
        }

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            var formattedMessage = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
            LogEntries.Add(formattedMessage);
            LatestLogEntry = formattedMessage;
        }

        public void Clear()
        {
            LogEntries.Clear();
            LatestLogEntry = string.Empty;
        }
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Success
    }
} 
