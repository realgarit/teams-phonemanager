using System;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Services
{
    public class SessionManager : ISessionManager
    {
        private readonly ILoggingService _loggingService;

        public SessionManager(ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _loggingService.Log("Session manager initialized", LogLevel.Info);
        }
        
        public bool ModulesChecked { get; set; }

        public bool TeamsConnected { get; set; }
        public bool GraphConnected { get; set; }

        public string? TeamsAccount { get; set; }
        public string? GraphAccount { get; set; }

        public string? TenantId { get; set; }
        public string? TenantName { get; set; }

        public DateTime LastTeamsConnection { get; set; }
        public DateTime LastGraphConnection { get; set; }

        public bool IsSessionValid => TeamsConnected && GraphConnected;
        
        public TimeSpan TeamsSessionDuration => DateTime.Now - LastTeamsConnection;
        public TimeSpan GraphSessionDuration => DateTime.Now - LastGraphConnection;
        
        public TimeSpan SessionTimeout => TimeSpan.FromHours(24);
        
        public bool IsSessionExpired => 
            TeamsSessionDuration > SessionTimeout || 
            GraphSessionDuration > SessionTimeout;
        
        public void UpdateTeamsConnection(bool connected, string? account = null)
        {
            TeamsConnected = connected;
            TeamsAccount = account;

            if (connected)
            {
                LastTeamsConnection = DateTime.Now;
                _loggingService.Log($"Teams connection updated: Connected as {account}", LogLevel.Info);
            }
            else
            {
                _loggingService.Log("Teams connection updated: Disconnected", LogLevel.Info);
            }
        }

        public void UpdateGraphConnection(bool connected, string? account = null)
        {
            GraphConnected = connected;
            GraphAccount = account;

            if (connected)
            {
                LastGraphConnection = DateTime.Now;
                _loggingService.Log($"Graph connection updated: Connected as {account}", LogLevel.Info);
            }
            else
            {
                _loggingService.Log("Graph connection updated: Disconnected", LogLevel.Info);
            }
        }

        public void UpdateModulesChecked(bool modulesChecked)
        {
            ModulesChecked = modulesChecked;
            _loggingService.Log($"Modules checked state updated: {modulesChecked}", LogLevel.Info);
        }

        public void UpdateTenantInfo(string tenantId, string tenantName)
        {
            TenantId = tenantId;
            TenantName = tenantName;
            _loggingService.Log($"Teams tenant info updated: {tenantName} ({tenantId})", LogLevel.Info);
        }

        public void ResetSession()
        {
            TeamsConnected = false;
            GraphConnected = false;
            TeamsAccount = null;
            GraphAccount = null;
            TenantId = null;
            TenantName = null;
            LastTeamsConnection = DateTime.MinValue;
            LastGraphConnection = DateTime.MinValue;

            _loggingService.Log("Session reset", LogLevel.Info);
        }
    }
} 
