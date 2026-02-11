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
            return @"
try {
    $resourceAccounts = Get-MgUser -Filter ""startswith(userPrincipalName,'" + ConstantsService.Naming.ResourceAccountCallQueuePrefix + @"')"" -Property Id,DisplayName,UserPrincipalName,UsageLocation
    if ($resourceAccounts) {
        Write-Host ""SUCCESS: Found $($resourceAccounts.Count) resource accounts starting with '" + ConstantsService.Naming.ResourceAccountCallQueuePrefix + @"'""
        foreach ($account in $resourceAccounts) {
            Write-Host ""RESOURCEACCOUNT: $($account.DisplayName)|$($account.UserPrincipalName)|$($account.Id)|$($account.UsageLocation)""
        }
    } else {
        Write-Host ""INFO: No resource accounts found starting with '" + ConstantsService.Naming.ResourceAccountCallQueuePrefix + @"'""
    }
}
catch {
    Write-Host ""ERROR: Failed to retrieve resource accounts: $_""
}";
        }

        public string GetCreateResourceAccountCommand(PhoneManagerVariables variables)
        {
            return $@"
New-CsOnlineApplicationInstance -UserPrincipalName {variables.RacqUPN} -ApplicationId {variables.CsAppCqId} -DisplayName {variables.RacqDisplayName}
Write-Host ""SUCCESS: Resource account {variables.RacqUPN} created successfully""";
        }

        public string GetUpdateResourceAccountUsageLocationCommand(string upn, string usageLocation)
        {
            return $@"
try {{
    Update-MgUser -UserId ""{upn}"" -UsageLocation ""{usageLocation}""
    Write-Host ""SUCCESS: Updated usage location for {upn} to {usageLocation}""
}}
catch {{
    Write-Host ""ERROR: Failed to update usage location for {upn}: $_""
}}";
        }

        public string GetRetrieveAutoAttendantResourceAccountsCommand()
        {
            return @"
try {
    $resourceAccounts = Get-MgUser -Filter ""startswith(userPrincipalName,'" + ConstantsService.Naming.ResourceAccountAutoAttendantPrefix + @"')"" -Property Id,DisplayName,UserPrincipalName,UsageLocation
    if ($resourceAccounts) {
        Write-Host ""SUCCESS: Found $($resourceAccounts.Count) resource accounts starting with '" + ConstantsService.Naming.ResourceAccountAutoAttendantPrefix + @"'""
        foreach ($account in $resourceAccounts) {
            Write-Host ""RESOURCEACCOUNT: $($account.DisplayName)|$($account.UserPrincipalName)|$($account.Id)|$($account.UsageLocation)""
        }
    } else {
        Write-Host ""INFO: No resource accounts found starting with '" + ConstantsService.Naming.ResourceAccountAutoAttendantPrefix + @"'""
    }
}
catch {
    Write-Host ""ERROR: Failed to retrieve resource accounts: $_""
}";
        }

        public string GetCreateAutoAttendantResourceAccountCommand(PhoneManagerVariables variables)
        {
            return $@"
New-CsOnlineApplicationInstance -UserPrincipalName {variables.RaaaUPN} -ApplicationId {variables.CsAppAaId} -DisplayName {variables.RaaaDisplayName}
Write-Host ""SUCCESS: Resource account {variables.RaaaUPN} created successfully""";
        }

        public string GetUpdateAutoAttendantResourceAccountUsageLocationCommand(string upn, string usageLocation)
        {
            return $@"
try {{
    Update-MgUser -UserId ""{upn}"" -UsageLocation ""{usageLocation}""
    Write-Host ""SUCCESS: Updated usage location for {upn} to {usageLocation}""
}}
catch {{
    Write-Host ""ERROR: Failed to update usage location for {upn}: $_""
}}";
        }

        public string GetAssignAutoAttendantLicenseCommand(string userId, string skuId)
        {
            return $@"
try {{
    $SkuId = ""{skuId}""
    Set-MgUserLicense -UserId ""{userId}"" -AddLicenses @{{SkuId = $SkuId}} -RemoveLicenses @()
    Write-Host ""SUCCESS: License assigned to user {userId} successfully""
}}
catch {{
    Write-Host ""ERROR: Failed to assign license to user {userId}: $_""
}}";
        }
    }
}
