using System;
using System.Threading.Tasks;

namespace teams_phonemanager.Services
{
    public class SessionManager
    {
        private static SessionManager? _instance;
        
        private SessionManager()
        {
            LoggingService.Instance.Log("Session manager initialized", LogLevel.Info);
        }
        
        public static SessionManager Instance
        {
            get
            {
                _instance ??= new SessionManager();
                return _instance;
            }
        }
        
        public bool ModulesChecked { get; set; }
        
        public bool TeamsConnected { get; set; }
        public bool GraphConnected { get; set; }
        
        public string? TeamsAccount { get; set; }
        public string? GraphAccount { get; set; }
        
        public string? TeamsTenantId { get; set; }
        public string? TeamsTenantName { get; set; }
        
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
                LoggingService.Instance.Log($"Teams connection updated: Connected as {account}", LogLevel.Info);
            }
            else
            {
                LoggingService.Instance.Log("Teams connection updated: Disconnected", LogLevel.Info);
            }
        }
        
        public void UpdateGraphConnection(bool connected, string? account = null)
        {
            GraphConnected = connected;
            GraphAccount = account;
            
            if (connected)
            {
                LastGraphConnection = DateTime.Now;
                LoggingService.Instance.Log($"Graph connection updated: Connected as {account}", LogLevel.Info);
            }
            else
            {
                LoggingService.Instance.Log("Graph connection updated: Disconnected", LogLevel.Info);
            }
        }
        
        public void UpdateModulesChecked(bool modulesChecked)
        {
            ModulesChecked = modulesChecked;
            LoggingService.Instance.Log($"Modules checked state updated: {modulesChecked}", LogLevel.Info);
        }
        
        public void UpdateTeamsTenantInfo(string tenantId, string tenantName)
        {
            TeamsTenantId = tenantId;
            TeamsTenantName = tenantName;
            LoggingService.Instance.Log($"Teams tenant info updated: {tenantName} ({tenantId})", LogLevel.Info);
        }
        
        public void ResetSession()
        {
            TeamsConnected = false;
            GraphConnected = false;
            TeamsAccount = null;
            GraphAccount = null;
            TeamsTenantId = null;
            TeamsTenantName = null;
            LastTeamsConnection = DateTime.MinValue;
            LastGraphConnection = DateTime.MinValue;
            
            LoggingService.Instance.Log("Session reset", LogLevel.Info);
        }
    }
} 
