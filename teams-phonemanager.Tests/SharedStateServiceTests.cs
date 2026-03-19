using Xunit;
using teams_phonemanager.Services;
using teams_phonemanager.Models;

namespace teams_phonemanager.Tests
{
    public class SharedStateServiceTests
    {
        [Fact]
        public void DefaultState_HasExpectedDefaults()
        {
            var service = new SharedStateService();

            Assert.NotNull(service.Variables);
            Assert.False(service.SkipScriptPreview);
            Assert.False(service.SkipDeleteConfirmation);
            Assert.True(service.AutoRefreshAfterOperations);
            Assert.Equal(LogLevel.Info, service.MinimumLogLevel);
        }

        [Fact]
        public void Variables_CanBeReplaced()
        {
            var service = new SharedStateService();
            var newVars = new PhoneManagerVariables { Customer = "NewCustomer" };

            service.Variables = newVars;

            Assert.Equal("NewCustomer", service.Variables.Customer);
        }

        [Fact]
        public void Settings_Persist()
        {
            var service = new SharedStateService();

            service.SkipScriptPreview = true;
            service.SkipDeleteConfirmation = true;
            service.AutoRefreshAfterOperations = false;
            service.MinimumLogLevel = LogLevel.Warning;

            Assert.True(service.SkipScriptPreview);
            Assert.True(service.SkipDeleteConfirmation);
            Assert.False(service.AutoRefreshAfterOperations);
            Assert.Equal(LogLevel.Warning, service.MinimumLogLevel);
        }
    }
}
