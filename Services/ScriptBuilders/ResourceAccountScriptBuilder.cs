using teams_phonemanager.Models;
using teams_phonemanager.Services.Interfaces;
using teams_phonemanager.Services;

namespace teams_phonemanager.Services.ScriptBuilders
{
    public class ResourceAccountScriptBuilder
    {
        private readonly IPowerShellSanitizationService _sanitizer;

        public ResourceAccountScriptBuilder(IPowerShellSanitizationService sanitizer)
        {
            _sanitizer = sanitizer;
        }

        public string GetRetrieveResourceAccountsCommand()
        {
            return BuildRetrieveCommand(ConstantsService.Naming.ResourceAccountCallQueuePrefix);
        }

        public string GetRetrieveAutoAttendantResourceAccountsCommand()
        {
            return BuildRetrieveCommand(ConstantsService.Naming.ResourceAccountAutoAttendantPrefix);
        }

        public string GetCreateResourceAccountCommand(PhoneManagerVariables variables)
            => GetCreateResourceAccountCommand(variables.RacqUPN, variables.RacqDisplayName, variables.CsAppCqId);

        public string GetCreateResourceAccountCommand(string upn, string displayName, string appId)
        {
            return BuildCreateCommand(upn, displayName, appId);
        }

        public string GetCreateAutoAttendantResourceAccountCommand(PhoneManagerVariables variables)
            => GetCreateAutoAttendantResourceAccountCommand(variables.RaaaUPN, variables.RaaaDisplayName, variables.CsAppAaId);

        public string GetCreateAutoAttendantResourceAccountCommand(string upn, string displayName, string appId)
        {
            return BuildCreateCommand(upn, displayName, appId);
        }

        public string GetUpdateResourceAccountUsageLocationCommand(string upn, string usageLocation)
        {
            return BuildUpdateUsageLocationCommand(upn, usageLocation);
        }

        public string GetUpdateAutoAttendantResourceAccountUsageLocationCommand(string upn, string usageLocation)
        {
            return BuildUpdateUsageLocationCommand(upn, usageLocation);
        }

        public string GetAssignAutoAttendantLicenseCommand(string userId, string skuId)
        {
            var sanitizedUserId = _sanitizer.SanitizeString(userId);
            var sanitizedSkuId = _sanitizer.SanitizeString(skuId);

            return $@"
try {{
    $SkuId = ""{sanitizedSkuId}""
    Set-MgUserLicense -UserId ""{sanitizedUserId}"" -AddLicenses @{{SkuId = $SkuId}} -RemoveLicenses @()
    Write-Host ""SUCCESS: License assigned to user successfully""
}}
catch {{
    Write-Host ""ERROR: Failed to assign license: $_""
}}";
        }

        private string BuildRetrieveCommand(string prefix)
        {
            return @"
try {
    $resourceAccounts = Get-MgUser -Filter ""startswith(userPrincipalName,'" + prefix + @"')"" -Property Id,DisplayName,UserPrincipalName,UsageLocation
    if ($resourceAccounts) {
        Write-Host ""SUCCESS: Found $($resourceAccounts.Count) resource accounts starting with '" + prefix + @"'""
        foreach ($account in $resourceAccounts) {
            Write-Host ""RESOURCEACCOUNT: $($account.DisplayName)|$($account.UserPrincipalName)|$($account.Id)|$($account.UsageLocation)""
        }
    } else {
        Write-Host ""INFO: No resource accounts found starting with '" + prefix + @"'""
    }
}
catch {
    Write-Host ""ERROR: Failed to retrieve resource accounts: $_""
}";
        }

        private string BuildCreateCommand(string upn, string displayName, string appId)
        {
            var sanitizedUpn = _sanitizer.SanitizeString(upn);
            var sanitizedDisplayName = _sanitizer.SanitizeString(displayName);
            var sanitizedAppId = _sanitizer.SanitizeString(appId);

            return $@"
try {{
    New-CsOnlineApplicationInstance -UserPrincipalName ""{sanitizedUpn}"" -ApplicationId ""{sanitizedAppId}"" -DisplayName ""{sanitizedDisplayName}""
    Write-Host ""SUCCESS: Resource account created successfully""
}}
catch {{
    Write-Host ""ERROR: Failed to create resource account: $_""
}}";
        }

        private string BuildUpdateUsageLocationCommand(string upn, string usageLocation)
        {
            var sanitizedUpn = _sanitizer.SanitizeString(upn);
            var sanitizedUsageLocation = _sanitizer.SanitizeString(usageLocation);

            return $@"
try {{
    Update-MgUser -UserId ""{sanitizedUpn}"" -UsageLocation ""{sanitizedUsageLocation}""
    Write-Host ""SUCCESS: Updated usage location for resource account""
}}
catch {{
    Write-Host ""ERROR: Failed to update usage location: $_""
}}";
        }
    }
}
