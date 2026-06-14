namespace teams_phonemanager.Services.Interfaces
{
    /// <summary>
    /// Port for building the PowerShell scripts that export tenant documentation data.
    /// Implemented by the Infrastructure DocumentationScriptBuilder (frozen script text);
    /// consumed by the Presentation DocumentationViewModel via this abstraction.
    /// </summary>
    public interface IDocumentationScriptBuilder
    {
        string GetExportTenantInfoCommand();
        string GetExportResourceAccountsCommand();
        string GetExportAutoAttendantsCommand();
        string GetExportCallQueuesCommand();
        string GetExportSchedulesCommand();
        string GetExportPhoneNumbersCommand();
        string GetExportVoiceUsersCommand();
    }
}
