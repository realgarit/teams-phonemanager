namespace teams_phonemanager.Services.Interfaces;

/// <summary>
/// Manages session state for PowerShell connections.
/// </summary>
public interface ISessionManager
{
    bool ModulesChecked { get; set; }
    bool TeamsConnected { get; set; }
    bool GraphConnected { get; set; }
    bool IsSessionValid { get; }
    string? TeamsAccount { get; }
    string? GraphAccount { get; }
    string? TenantId { get; }
    string? TenantName { get; }

    void UpdateTeamsConnection(bool connected, string? account = null);
    void UpdateGraphConnection(bool connected, string? account = null);
    void UpdateModulesChecked(bool modulesChecked);
    void UpdateTenantInfo(string tenantId, string tenantName);
    void ResetSession();
}
