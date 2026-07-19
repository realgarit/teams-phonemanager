using System.IO;
using System.Text.Json;
using Xunit;

namespace teams_phonemanager.Tests
{
    /// <summary>
    /// Validates the pinned-version manifest consumed by Scripts/download-modules.ps1/.sh and by
    /// <c>BundledModuleVersionService</c> at runtime.
    /// </summary>
    public class ModuleVersionsFileTests
    {
        private static readonly string[] ExpectedModuleNames =
        {
            "MicrosoftTeams",
            "Microsoft.Graph.Authentication",
            "Microsoft.Graph.Users",
            "Microsoft.Graph.Users.Actions",
            "Microsoft.Graph.Groups",
            "Microsoft.Graph.Identity.DirectoryManagement",
        };

        [Fact]
        public void ModuleVersionsJson_ParsesAndContainsAllExpectedModulesWithVersions()
        {
            var path = FindModuleVersionsFile();
            Assert.True(File.Exists(path), $"scripts/module-versions.json not found (searched up from {AppContext.BaseDirectory})");

            using var doc = JsonDocument.Parse(File.ReadAllText(path));

            Assert.True(doc.RootElement.TryGetProperty("modules", out var modules));
            Assert.Equal(JsonValueKind.Array, modules.ValueKind);

            var foundNames = new List<string>();
            foreach (var module in modules.EnumerateArray())
            {
                var name = module.GetProperty("name").GetString();
                var version = module.GetProperty("version").GetString();

                Assert.False(string.IsNullOrWhiteSpace(name));
                Assert.False(string.IsNullOrWhiteSpace(version));
                foundNames.Add(name!);
            }

            foreach (var expected in ExpectedModuleNames)
            {
                Assert.Contains(expected, foundNames);
            }

            Assert.True(doc.RootElement.TryGetProperty("powerShellSdkVersion", out var sdkVersion));
            Assert.False(string.IsNullOrWhiteSpace(sdkVersion.GetString()));
        }

        /// <summary>
        /// Walks up from the test's execution directory to find the repo root, identified by
        /// the presence of the solution file, then returns the path to scripts/module-versions.json.
        /// </summary>
        private static string FindModuleVersionsFile()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null)
            {
                var candidate = Path.Combine(dir.FullName, "Scripts", "module-versions.json");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                if (dir.GetFiles("*.slnx").Length > 0 || dir.GetFiles("*.sln").Length > 0)
                {
                    return Path.Combine(dir.FullName, "Scripts", "module-versions.json");
                }

                dir = dir.Parent;
            }

            throw new FileNotFoundException("Could not locate repo root (no *.slnx/*.sln found while walking up).");
        }
    }
}
