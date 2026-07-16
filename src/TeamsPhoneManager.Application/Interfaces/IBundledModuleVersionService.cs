namespace teams_phonemanager.Services.Interfaces;

/// <summary>
/// Port for reading the pinned versions of the PowerShell modules bundled with the app
/// (see Scripts/module-versions.json), for display in the Settings/About panel.
/// </summary>
public interface IBundledModuleVersionService
{
    /// <summary>Pinned MicrosoftTeams module version, or "Unknown" if it cannot be read.</summary>
    string TeamsModuleVersion { get; }

    /// <summary>
    /// Pinned Microsoft.Graph module version, representative of all bundled Microsoft.Graph.*
    /// modules (they are versioned together), or "Unknown" if it cannot be read.
    /// </summary>
    string GraphModuleVersion { get; }

    /// <summary>Bundled PowerShell SDK version, or "Unknown" if it cannot be read.</summary>
    string PowerShellSdkVersion { get; }
}
