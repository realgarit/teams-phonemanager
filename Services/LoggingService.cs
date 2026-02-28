using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Services
{
    public partial class LoggingService : ObservableObject, ILoggingService
    {
        [GeneratedRegex(@"[\w\.-]+@[\w\.-]+\.\w+", RegexOptions.Compiled)]
        private static partial Regex EmailPattern();

        [GeneratedRegex(@"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}", RegexOptions.Compiled)]
        private static partial Regex GuidPattern();

        [ObservableProperty]
        private ObservableCollection<string> _logEntries = new();

        [ObservableProperty]
        private string _latestLogEntry = string.Empty;

        [ObservableProperty]
        private LogLevel _minimumLogLevel = LogLevel.Info;

        // Store the log level for each entry so we can filter later
        private readonly List<(string Entry, LogLevel Level)> _allEntries = new();

        public LoggingService() { }

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            var sanitizedMessage = SanitizeLogMessage(message);
            var formattedMessage = $"[{DateTime.Now:HH:mm:ss}] [{level}] {sanitizedMessage}";
            _allEntries.Add((formattedMessage, level));
            LogEntries.Add(formattedMessage);
            LatestLogEntry = formattedMessage;
        }

        public void Clear()
        {
            _allEntries.Clear();
            LogEntries.Clear();
            LatestLogEntry = string.Empty;
        }

        public IReadOnlyList<string> GetFilteredEntries()
        {
            return _allEntries
                .Where(e => e.Level >= MinimumLogLevel)
                .Select(e => e.Entry)
                .ToList();
        }

        private static string SanitizeLogMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return message;

            // Mask email addresses (keep first character and domain)
            message = EmailPattern().Replace(message, m =>
            {
                var parts = m.Value.Split('@');
                if (parts.Length == 2 && parts[0].Length > 0)
                {
                    return $"{parts[0][0]}***@{parts[1]}";
                }
                return "***@***";
            });

            // Mask GUIDs (tenant IDs, etc.)
            message = GuidPattern().Replace(message, "***-****-****-****-************");

            return message;
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
