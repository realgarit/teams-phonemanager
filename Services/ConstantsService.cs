namespace teams_phonemanager.Services
{
    public static class ConstantsService
    {
        public static class TeamsPhone
        {
            public const string SkuId = "440eaaa8-b3e0-484b-a8be-62870b9ba70a";
            public const string CallQueueAppId = "11cd3e2e-fccb-42ad-ad00-878b93575e07";
            public const string AutoAttendantAppId = "ce933385-9390-45d1-9512-c8d228074e07";
        }

        public static class PowerShell
        {
            public const int DefaultWaitTimeSeconds = 30;
            public const int LongWaitTimeSeconds = 120;
            public const string ExecutionPolicy = "Bypass";
            public const int ExitCodeError = 1;
        }

        public static class Application
        {
            public const string Version = "Version 3.10.0";
            public const string Copyright = "Realgar © 2026. MIT License.";
        }

        public static class Messages
        {
            public const string WaitingMessage = "Bitte warte bis die vorherige Operation im Backend von Microsoft ankommmt";
            public const string LicenseWaitingMessage = "Bitte warte bis die Teams Phone Resource Lizenz richtig gesetzt ist";
        }

        public static class CallQueue
        {
            public const int AgentAlertTime = 30;
            public const int OverflowThreshold = 15;
            public const int TimeoutThreshold = 30;
            public const int MinTimeoutThreshold = 30;
        }

        public static class Naming
        {
            public const string ResourceAccountCallQueuePrefix = "racq-";
            public const string ResourceAccountAutoAttendantPrefix = "raaa-";
            public const string M365GroupPrefix = "ttgrp";
            public const string CallQueuePrefix = "cq-";
            public const string AutoAttendantPrefix = "aa-";
            public const string HolidayPrefix = "hd-";
        }

        public static class PowerShellModules
        {
            public const string MicrosoftTeams = "MicrosoftTeams";
            public const string MicrosoftGraph = "Microsoft.Graph";
            public const string MicrosoftGraphAuthentication = "Microsoft.Graph.Authentication";
            public const string MicrosoftGraphUsers = "Microsoft.Graph.Users";
            public const string MicrosoftGraphUsersActions = "Microsoft.Graph.Users.Actions";
            public const string MicrosoftGraphGroups = "Microsoft.Graph.Groups";
            public const string MicrosoftGraphIdentityDirectoryManagement = "Microsoft.Graph.Identity.DirectoryManagement";
        }

        public static class Pages
        {
            public const string Welcome = "Welcome";
            public const string GetStarted = "Get Started";
            public const string Variables = "Variables";
            public const string M365Groups = "M365 Groups";
            public const string CallQueues = "Call Queues";
            public const string AutoAttendants = "Auto Attendants";
            public const string Holidays = "Holidays";
        }

        public static class ErrorDialogTitles
        {
            public const string PowerShellError = "PowerShell Error";
            public const string ValidationError = "Validation Error";
            public const string ConnectionError = "Connection Error";
            public const string Error = "Error";
            public const string Confirmation = "Confirmation";
            public const string Success = "Success";
            public const string Information = "Information";
        }
    }
}