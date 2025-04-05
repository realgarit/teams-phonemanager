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
        
        // Module availability state
        public bool ModulesChecked { get; set; }
        
        // Connection states
        public bool TeamsConnected { get; set; }
        public bool GraphConnected { get; set; }
        
        // Session information
        public string? TeamsAccount { get; set; }
        public string? GraphAccount { get; set; }
        
        // Tenant information
        public string? TeamsTenantId { get; set; }
        public string? TeamsTenantName { get; set; }
        
        // Session timestamp
        public DateTime LastTeamsConnection { get; set; }
        public DateTime LastGraphConnection { get; set; }
        
        // Session status
        public bool IsSessionValid => TeamsConnected && GraphConnected;
        
        // Session duration
        public TimeSpan TeamsSessionDuration => DateTime.Now - LastTeamsConnection;
        public TimeSpan GraphSessionDuration => DateTime.Now - LastGraphConnection;
        
        // Session timeout (24 hours)
        public TimeSpan SessionTimeout => TimeSpan.FromHours(24);
        
        // Check if session is expired
        public bool IsSessionExpired => 
            TeamsSessionDuration > SessionTimeout || 
            GraphSessionDuration > SessionTimeout;
        
        // Update Teams connection state
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
        
        // Update Graph connection state
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
        
        // Update module availability
        public void UpdateModulesChecked(bool modulesChecked)
        {
            ModulesChecked = modulesChecked;
            LoggingService.Instance.Log($"Modules checked state updated: {modulesChecked}", LogLevel.Info);
        }
        
        // Update tenant information
        public void UpdateTeamsTenantInfo(string tenantId, string tenantName)
        {
            TeamsTenantId = tenantId;
            TeamsTenantName = tenantName;
            LoggingService.Instance.Log($"Teams tenant info updated: {tenantName} ({tenantId})", LogLevel.Info);
        }
        
        // Reset all session data
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