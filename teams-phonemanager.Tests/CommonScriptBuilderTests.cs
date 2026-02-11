using Moq;
using Xunit;
using teams_phonemanager.Services.ScriptBuilders;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Tests
{
    public class CommonScriptBuilderTests
    {
        private readonly Mock<IPowerShellSanitizationService> _mockSanitizer;

        public CommonScriptBuilderTests()
        {
            _mockSanitizer = new Mock<IPowerShellSanitizationService>();
        }

        [Fact]
        public void GetCheckModulesCommand_ContainsLinuxPath_ReturnsTrue()
        {
            var builder = new CommonScriptBuilder(_mockSanitizer.Object);
            var script = builder.GetCheckModulesCommand();

            Assert.Contains("linux-x64/Modules", script);
            Assert.Contains("../linux-x64/Modules", script);
        }

        [Fact]
        public void GetCommonSetupScript_ContainsLinuxPath_ReturnsTrue()
        {
            var builder = new CommonScriptBuilder(_mockSanitizer.Object);
            var script = builder.GetCommonSetupScript();

            Assert.Contains("linux-x64/Modules", script);
            Assert.Contains("../linux-x64/Modules", script);
        }
    }
}
