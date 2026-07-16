using System.IO;
using System.Text.Json;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Services;

/// <summary>
/// Reads the pinned PowerShell module versions bundled with the app from
/// Scripts/module-versions.json (copied to the output directory alongside the app; see
/// teams-phonemanager.csproj). Fails silently: any missing/malformed file yields "Unknown"
/// values rather than throwing, so the Settings/About panel never crashes.
/// </summary>
public sealed class BundledModuleVersionService : IBundledModuleVersionService
{
    private const string UnknownVersion = "Unknown";
    private const string VersionsFileName = "Scripts/module-versions.json";
    private const string GraphModulePrefix = "Microsoft.Graph";

    public string TeamsModuleVersion { get; }
    public string GraphModuleVersion { get; }
    public string PowerShellSdkVersion { get; }

    /// <summary>Used by DI: reads Scripts/module-versions.json next to the running app.</summary>
    public BundledModuleVersionService(ILoggingService? loggingService = null)
        : this(Path.Combine(AppContext.BaseDirectory, VersionsFileName), loggingService)
    {
    }

    /// <summary>Test seam: reads the manifest from an explicit path instead of the app's output directory.</summary>
    public BundledModuleVersionService(string versionsFilePath, ILoggingService? loggingService = null)
    {
        var (teamsVersion, graphVersion, sdkVersion) = Load(versionsFilePath, loggingService);
        TeamsModuleVersion = teamsVersion;
        GraphModuleVersion = graphVersion;
        PowerShellSdkVersion = sdkVersion;
    }

    private static (string teams, string graph, string sdk) Load(string path, ILoggingService? loggingService)
    {
        try
        {
            if (!File.Exists(path))
            {
                loggingService?.Log($"Bundled module versions file not found at {path}", LogLevel.Warning);
                return (UnknownVersion, UnknownVersion, UnknownVersion);
            }

            using var stream = File.OpenRead(path);
            using var doc = JsonDocument.Parse(stream);

            var teams = UnknownVersion;
            var graph = UnknownVersion;

            if (doc.RootElement.TryGetProperty("modules", out var modules) &&
                modules.ValueKind == JsonValueKind.Array)
            {
                foreach (var module in modules.EnumerateArray())
                {
                    var name = module.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : null;
                    var version = module.TryGetProperty("version", out var versionElement) ? versionElement.GetString() : null;

                    if (name is null || version is null)
                    {
                        continue;
                    }

                    if (string.Equals(name, "MicrosoftTeams", StringComparison.OrdinalIgnoreCase))
                    {
                        teams = version;
                    }
                    else if (name.StartsWith(GraphModulePrefix, StringComparison.OrdinalIgnoreCase) && graph == UnknownVersion)
                    {
                        graph = version;
                    }
                }
            }

            var sdk = doc.RootElement.TryGetProperty("powerShellSdkVersion", out var sdkElement)
                ? sdkElement.GetString() ?? UnknownVersion
                : UnknownVersion;

            return (teams, graph, sdk);
        }
        catch (Exception ex)
        {
            loggingService?.Log($"Failed to read bundled module versions: {ex.Message}", LogLevel.Warning);
            return (UnknownVersion, UnknownVersion, UnknownVersion);
        }
    }
}
