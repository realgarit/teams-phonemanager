namespace PhoneDesk.Services
{
    /// <summary>
    /// Severity levels for application log messages. Pure enum — lives in Domain so every layer
    /// (logging port in Application, log viewer in Presentation) can reference it without coupling.
    /// </summary>
    public enum LogLevel
    {
        Info,
        Success,
        Warning,
        Error
    }
}
