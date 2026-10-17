using System.Text.Json;
using System.Xml.Linq;
using PhoneDesk.Localization;
using Xunit.Sdk;

namespace PhoneDesk.Tests;

public sealed class ReadinessGuidanceTests
{
    [Fact]
    public void SameOrganizationRequirementIsAlwaysVisibleBeforeSignInActions()
        => AssertVisibleRequirement(LoadReadinessView());

    [Theory]
    [InlineData(AppLanguage.English, "en")]
    [InlineData(AppLanguage.German, "de")]
    public void SameOrganizationRequirementHasExplicitTranslation(AppLanguage language, string languageCode)
    {
        using var catalog = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            FindRepositoryRoot(), "src", "PhoneDesk.Presentation", "Resources", "Localization",
            $"Strings.{languageCode}.json")));

        Assert.Equal(ExpectedSameOrganizationRequirement(language),
            catalog.RootElement.GetProperty(nameof(UiTextKey.GetStartedSameOrganizationRequirement)).GetString());
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("collapsed-help")]
    [InlineData("conditional")]
    [InlineData("after-sign-in")]
    public void VisibilityGuardRejectsHiddenOrLateRequirements(string regression)
    {
        var document = LoadReadinessView();
        var requirement = FindRequirement(document);
        switch (regression)
        {
            case "missing":
                requirement.Remove();
                break;
            case "collapsed-help":
                requirement.Remove();
                document.Descendants().Single(e => e.Name.LocalName == "Expander").Add(requirement);
                break;
            case "conditional":
                requirement.SetAttributeValue("IsVisible", "{Binding !CanProceed}");
                break;
            case "after-sign-in":
                var banner = requirement.Ancestors().Single(IsGuidanceBanner);
                var parent = banner.Parent!;
                banner.Remove();
                parent.Add(banner);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(regression));
        }

        Assert.ThrowsAny<XunitException>(() => AssertVisibleRequirement(document));
    }

    internal static string ExpectedSameOrganizationRequirement(AppLanguage language)
        => language == AppLanguage.German
            ? "Melden Sie sich bei Teams und Graph in derselben Organisation an."
            : "Sign in to the same organization in Teams and Graph.";

    private static void AssertVisibleRequirement(XDocument document)
    {
        var requirement = FindRequirement(document);
        Assert.Equal("TextBlock", requirement.Name.LocalName);
        Assert.Equal("{loc:Translate Key=GetStartedSameOrganizationRequirement}",
            requirement.Attribute("Text")?.Value);
        Assert.Contains(requirement.Ancestors(), IsGuidanceBanner);
        Assert.DoesNotContain(requirement.Ancestors(), e =>
            e.Name.LocalName is "InfoButton" or "Expander" or "Flyout" or "ToolTip"
            || e.Name.LocalName.Contains('.'));
        Assert.All(requirement.AncestorsAndSelf(), e =>
        {
            var visibility = e.Attribute("IsVisible");
            Assert.True(visibility is null || visibility.Value.Equals("True", StringComparison.OrdinalIgnoreCase),
                "The same-organization instruction must not depend on readiness or hidden parent content.");
        });

        var elements = document.Descendants().ToList();
        foreach (var command in new[] { "CheckModulesCommand", "ConnectTeamsCommand", "ConnectGraphCommand" })
        {
            var action = Assert.Single(elements, e =>
                e.Name.LocalName == "Button" && e.Attribute("Command")?.Value == $"{{Binding {command}}}");
            Assert.True(elements.IndexOf(requirement) < elements.IndexOf(action),
                "The same-organization instruction must appear before setup and sign-in actions.");
        }
    }

    private static XElement FindRequirement(XDocument document)
        => Assert.Single(document.Descendants(), e =>
            e.Attributes().Any(a => a.Name.LocalName == "Name" && a.Value == "SameOrganizationRequirement"));

    private static bool IsGuidanceBanner(XElement element)
        => element.Name.LocalName == "Border"
            && (element.Attribute("Classes")?.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Contains("setup-guidance") ?? false);

    private static XDocument LoadReadinessView()
        => XDocument.Load(Path.Combine(FindRepositoryRoot(), "src", "PhoneDesk.Presentation", "Views", "GetStartedView.axaml"));

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "PhoneDesk.slnx")))
            directory = directory.Parent;
        Assert.NotNull(directory);
        return directory.FullName;
    }
}
