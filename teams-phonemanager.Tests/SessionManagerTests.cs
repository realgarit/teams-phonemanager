using Moq;
using Xunit;
using teams_phonemanager.Services;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Tests
{
    public class SessionManagerTests
    {
        private readonly SessionManager _sessionManager;

        public SessionManagerTests()
        {
            var mockLogging = new Mock<ILoggingService>();
            _sessionManager = new SessionManager(mockLogging.Object);
        }

        [Fact]
        public void InitialState_NothingConnected()
        {
            Assert.False(_sessionManager.ModulesChecked);
            Assert.False(_sessionManager.TeamsConnected);
            Assert.False(_sessionManager.GraphConnected);
            Assert.False(_sessionManager.IsSessionValid);
        }

        [Fact]
        public void UpdateTeamsConnection_SetsState()
        {
            _sessionManager.UpdateTeamsConnection(true, "admin@test.com");

            Assert.True(_sessionManager.TeamsConnected);
            Assert.Equal("admin@test.com", _sessionManager.TeamsAccount);
        }

        [Fact]
        public void UpdateGraphConnection_SetsState()
        {
            _sessionManager.UpdateGraphConnection(true, "admin@test.com");

            Assert.True(_sessionManager.GraphConnected);
            Assert.Equal("admin@test.com", _sessionManager.GraphAccount);
        }

        [Fact]
        public void IsSessionValid_RequiresBothConnections()
        {
            _sessionManager.UpdateTeamsConnection(true);
            Assert.False(_sessionManager.IsSessionValid);

            _sessionManager.UpdateGraphConnection(true);
            Assert.True(_sessionManager.IsSessionValid);
        }

        [Fact]
        public void UpdateTenantInfo_StoresValues()
        {
            _sessionManager.UpdateTenantInfo("tenant-id-123", "Test Tenant");

            Assert.Equal("tenant-id-123", _sessionManager.TenantId);
            Assert.Equal("Test Tenant", _sessionManager.TenantName);
        }

        [Fact]
        public void ResetSession_ClearsEverything()
        {
            _sessionManager.UpdateTeamsConnection(true, "admin@test.com");
            _sessionManager.UpdateGraphConnection(true, "admin@test.com");
            _sessionManager.UpdateTenantInfo("id", "name");
            _sessionManager.UpdateModulesChecked(true);

            _sessionManager.ResetSession();

            Assert.False(_sessionManager.TeamsConnected);
            Assert.False(_sessionManager.GraphConnected);
            Assert.Null(_sessionManager.TeamsAccount);
            Assert.Null(_sessionManager.GraphAccount);
            Assert.Null(_sessionManager.TenantId);
            Assert.Null(_sessionManager.TenantName);
        }

        [Fact]
        public void SessionTimeout_Is24Hours()
        {
            Assert.Equal(TimeSpan.FromHours(24), _sessionManager.SessionTimeout);
        }

        [Fact]
        public void Disconnect_TeamsAccount_ClearedOnDisconnect()
        {
            _sessionManager.UpdateTeamsConnection(true, "admin@test.com");
            _sessionManager.UpdateTeamsConnection(false);

            Assert.False(_sessionManager.TeamsConnected);
            Assert.Null(_sessionManager.TeamsAccount);
        }

        [Fact]
        public void IsSessionExpired_FalseWhenNotConnected()
        {
            // When not connected, session shouldn't be considered "expired"
            Assert.False(_sessionManager.IsSessionExpired);
        }

        [Fact]
        public void IsSessionExpired_FalseWhenJustConnected()
        {
            _sessionManager.UpdateTeamsConnection(true);
            _sessionManager.UpdateGraphConnection(true);

            Assert.False(_sessionManager.IsSessionExpired);
        }

        [Fact]
        public void IsSessionExpired_FalseAfterReset()
        {
            _sessionManager.UpdateTeamsConnection(true);
            _sessionManager.UpdateGraphConnection(true);
            _sessionManager.ResetSession();

            Assert.False(_sessionManager.IsSessionExpired);
        }
    }
}
