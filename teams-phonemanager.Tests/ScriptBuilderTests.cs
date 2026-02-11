using Moq;
using Xunit;
using teams_phonemanager.Services.ScriptBuilders;
using teams_phonemanager.Services.Interfaces;
using teams_phonemanager.Models;

namespace teams_phonemanager.Tests
{
    public class ScriptBuilderTests
    {
        private readonly Mock<IPowerShellSanitizationService> _mockSanitizer;

        public ScriptBuilderTests()
        {
            _mockSanitizer = new Mock<IPowerShellSanitizationService>();
            _mockSanitizer.Setup(s => s.SanitizeString(It.IsAny<string>())).Returns<string>(s => s);
            _mockSanitizer.Setup(s => s.SanitizeIdentifier(It.IsAny<string>())).Returns<string>(s => s);
        }

        [Fact]
        public void CallQueueScriptBuilder_GetRetrieveCallQueuesCommand_ReturnsCorrectScript()
        {
            var builder = new CallQueueScriptBuilder(_mockSanitizer.Object);
            var script = builder.GetRetrieveCallQueuesCommand();

            Assert.Contains("Get-CsCallQueue", script);
            Assert.Contains("cq-", script);
        }

        [Fact]
        public void AutoAttendantScriptBuilder_GetCreateSimpleAutoAttendantCommand_ReturnsCorrectScript()
        {
            var builder = new AutoAttendantScriptBuilder(_mockSanitizer.Object);
            var variables = new PhoneManagerVariables
            {
                Customer = "Cust",
                RaaAnrName = "Anr",
                CustomerGroupName = "Grp",
                LanguageId = "en-US",
                TimeZoneId = "UTC"
            };

            var script = builder.GetCreateSimpleAutoAttendantCommand(variables);

            Assert.Contains("New-CsAutoAttendant", script);
            Assert.Contains("-Name \"aa-Cust-Anr-Grp\"", script);
            Assert.Contains("-LanguageId \"en-US\"", script);
            Assert.Contains("-TimeZoneId \"UTC\"", script);
        }

        [Fact]
        public void HolidayScriptBuilder_GetCreateHolidayCommand_ReturnsCorrectScript()
        {
            var builder = new HolidayScriptBuilder(_mockSanitizer.Object);
            var date = new DateTime(2023, 12, 25, 0, 0, 0);
            var script = builder.GetCreateHolidayCommand("Christmas", date);

            Assert.Contains("New-CsOnlineSchedule", script);
            Assert.Contains("-Name \"Christmas\"", script);
            Assert.Contains("25/12/2023", script);
        }
    }
}
