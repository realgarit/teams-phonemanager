using Moq;
using Xunit;
using teams_phonemanager.Services.ScriptBuilders;
using teams_phonemanager.Services.Interfaces;
using teams_phonemanager.Models;

namespace teams_phonemanager.Tests
{
    public class ResourceAccountScriptBuilderTests
    {
        private readonly Mock<IPowerShellSanitizationService> _mockSanitizer;
        private readonly ResourceAccountScriptBuilder _builder;

        public ResourceAccountScriptBuilderTests()
        {
            _mockSanitizer = new Mock<IPowerShellSanitizationService>();
            _mockSanitizer.Setup(s => s.SanitizeString(It.IsAny<string>())).Returns<string>(s => s);
            _mockSanitizer.Setup(s => s.SanitizeIdentifier(It.IsAny<string>())).Returns<string>(s => s);
            _builder = new ResourceAccountScriptBuilder(_mockSanitizer.Object);
        }

        [Fact]
        public void GetRetrieveResourceAccountsCommand_ContainsCqPrefix()
        {
            var script = _builder.GetRetrieveResourceAccountsCommand();
            Assert.Contains("racq-", script);
            Assert.Contains("Get-MgUser", script);
        }

        [Fact]
        public void GetRetrieveAutoAttendantResourceAccountsCommand_ContainsAaPrefix()
        {
            var script = _builder.GetRetrieveAutoAttendantResourceAccountsCommand();
            Assert.Contains("raaa-", script);
            Assert.Contains("Get-MgUser", script);
        }

        [Fact]
        public void GetCreateResourceAccountCommand_SanitizesInputs()
        {
            var script = _builder.GetCreateResourceAccountCommand("upn@test.com", "Display Name", "app-id");

            Assert.Contains("New-CsOnlineApplicationInstance", script);
            Assert.Contains("upn@test.com", script);
            Assert.Contains("Display Name", script);
            _mockSanitizer.Verify(s => s.SanitizeString("upn@test.com"), Times.Once());
            _mockSanitizer.Verify(s => s.SanitizeString("Display Name"), Times.Once());
        }

        [Fact]
        public void GetCreateAutoAttendantResourceAccountCommand_ProducesSameScriptAsCallQueue()
        {
            var cqScript = _builder.GetCreateResourceAccountCommand("upn@test.com", "Name", "cq-app-id");
            var aaScript = _builder.GetCreateAutoAttendantResourceAccountCommand("upn@test.com", "Name", "aa-app-id");

            // Both should use same template, just different app IDs
            Assert.Contains("New-CsOnlineApplicationInstance", cqScript);
            Assert.Contains("New-CsOnlineApplicationInstance", aaScript);
            Assert.Contains("cq-app-id", cqScript);
            Assert.Contains("aa-app-id", aaScript);
        }

        [Fact]
        public void GetCreateResourceAccountCommand_FromVariables_DelegatesToParameterized()
        {
            var variables = new PhoneManagerVariables
            {
                Customer = "acme",
                CustomerGroupName = "sales",
                MsFallbackDomain = "@acme.onmicrosoft.com"
            };

            var script = _builder.GetCreateResourceAccountCommand(variables);

            Assert.Contains("New-CsOnlineApplicationInstance", script);
            Assert.Contains(variables.CsAppCqId, script);
        }

        [Fact]
        public void GetUpdateResourceAccountUsageLocationCommand_ContainsUpdateMgUser()
        {
            var script = _builder.GetUpdateResourceAccountUsageLocationCommand("upn@test.com", "CH");

            Assert.Contains("Update-MgUser", script);
            Assert.Contains("upn@test.com", script);
            Assert.Contains("CH", script);
        }

        [Fact]
        public void GetAssignAutoAttendantLicenseCommand_ContainsSetMgUserLicense()
        {
            var script = _builder.GetAssignAutoAttendantLicenseCommand("user@test.com", "sku-123");

            Assert.Contains("Set-MgUserLicense", script);
            Assert.Contains("user@test.com", script);
            Assert.Contains("sku-123", script);
        }
    }
}
