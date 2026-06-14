using System.Collections.ObjectModel;
using System.ComponentModel;

namespace teams_phonemanager.Services.Interfaces;

/// <summary>
/// Service for logging application messages.
/// </summary>
public interface ILoggingService : INotifyPropertyChanged
{
    ObservableCollection<string> LogEntries { get; }
    string LatestLogEntry { get; }
    LogLevel MinimumLogLevel { get; set; }
    void Log(string message, LogLevel level = LogLevel.Info);
    void Clear();
    /// <summary>Returns log entries filtered by the current MinimumLogLevel.</summary>
    IReadOnlyList<string> GetFilteredEntries();
}
