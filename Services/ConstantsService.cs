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
        }

        public static class Application
        {
            public const string Version = "Version 2.1.0";
            public const string Copyright = "Realgar © 2025. MIT License.";
        }

        public static class Messages
        {
            public const string WaitingMessage = "Bitte warte bis die vorherige Operation im Backend von Microsoft ankommmt";
            public const string LicenseWaitingMessage = "Bitte warte bis die Teams Phone Resource Lizenz richtig gesetzt ist";
        }
    }
}
