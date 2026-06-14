using System;
using System.Linq;

namespace teams_phonemanager.Tests
{
    /// <summary>
    /// Enforces Clean Architecture's Dependency Rule at build time: the inner layers must not
    /// take dependencies on any UI / host framework. If a future change leaks one of the
    /// forbidden assemblies into Domain (or, later, Application), these tests fail.
    /// </summary>
    public class DependencyRuleTests
    {
        private static readonly string[] ForbiddenFrameworks =
        {
            "Avalonia",
            "CommunityToolkit.Mvvm",
            "FluentAvalonia",
            "FluentIcons",
            "System.Management.Automation",
            "Microsoft.PowerShell",
            "Microsoft.Identity.Client",
        };

        [Fact]
        public void Domain_assembly_has_no_framework_dependencies()
        {
            // Anchor on a known Domain type to load the Domain assembly.
            var domain = typeof(teams_phonemanager.Services.ConstantsService).Assembly;
            Assert.Equal("TeamsPhoneManager.Domain", domain.GetName().Name);

            var referenced = domain.GetReferencedAssemblies()
                .Select(a => a.Name ?? string.Empty)
                .ToArray();

            var leaks = referenced
                .Where(name => ForbiddenFrameworks.Any(f =>
                    name.StartsWith(f, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            Assert.True(
                leaks.Length == 0,
                $"Domain must be framework-free but references: {string.Join(", ", leaks)}");
        }

        [Fact]
        public void Application_assembly_has_no_ui_or_io_framework_dependencies()
        {
            // Anchor on a known Application port to load the Application assembly.
            var application = typeof(teams_phonemanager.Services.Interfaces.IPowerShellContextService).Assembly;
            Assert.Equal("TeamsPhoneManager.Application", application.GetName().Name);

            var referenced = application.GetReferencedAssemblies()
                .Select(a => a.Name ?? string.Empty)
                .ToArray();

            var leaks = referenced
                .Where(name => ForbiddenFrameworks.Any(f =>
                    name.StartsWith(f, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            Assert.True(
                leaks.Length == 0,
                $"Application must be free of UI/IO frameworks but references: {string.Join(", ", leaks)}");
        }
    }
}
