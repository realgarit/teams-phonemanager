using System.IO;
using teams_phonemanager.Services;
using Xunit;

namespace teams_phonemanager.Tests
{
    /// <summary>
    /// Covers <see cref="BundledModuleVersionService"/>'s parsing of the pinned-version manifest,
    /// including its fail-silent behavior when the file is missing or malformed. Uses the
    /// explicit-path constructor overload as a test seam instead of the app's output directory.
    /// </summary>
    public class BundledModuleVersionServiceTests : IDisposable
    {
        private readonly string _tempDir = Directory.CreateTempSubdirectory("bundled-module-version-test-").FullName;

        [Fact]
        public void ValidManifest_ExposesTeamsGraphAndSdkVersions()
        {
            const string json = """
                {
                  "modules": [
                    { "name": "MicrosoftTeams", "version": "6.9.0" },
                    { "name": "Microsoft.Graph.Authentication", "version": "2.25.0" },
                    { "name": "Microsoft.Graph.Users", "version": "2.25.0" }
                  ],
                  "powerShellSdkVersion": "7.6.0"
                }
                """;

            var service = new BundledModuleVersionService(WriteManifest(json));

            Assert.Equal("6.9.0", service.TeamsModuleVersion);
            Assert.Equal("2.25.0", service.GraphModuleVersion);
            Assert.Equal("7.6.0", service.PowerShellSdkVersion);
        }

        [Fact]
        public void MissingManifest_ReturnsUnknownForAllVersionsAndDoesNotThrow()
        {
            var missingPath = Path.Combine(_tempDir, "does-not-exist.json");

            var service = new BundledModuleVersionService(missingPath);

            Assert.Equal("Unknown", service.TeamsModuleVersion);
            Assert.Equal("Unknown", service.GraphModuleVersion);
            Assert.Equal("Unknown", service.PowerShellSdkVersion);
        }

        [Fact]
        public void MalformedManifest_ReturnsUnknownVersionsAndDoesNotThrow()
        {
            var service = new BundledModuleVersionService(WriteManifest("{ not valid json"));

            Assert.Equal("Unknown", service.TeamsModuleVersion);
            Assert.Equal("Unknown", service.GraphModuleVersion);
            Assert.Equal("Unknown", service.PowerShellSdkVersion);
        }

        private string WriteManifest(string json)
        {
            var path = Path.Combine(_tempDir, "module-versions.json");
            File.WriteAllText(path, json);
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }
    }
}
