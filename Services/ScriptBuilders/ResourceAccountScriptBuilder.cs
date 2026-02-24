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
            // SECURITY: Sanitize all user inputs and wrap in quotes to prevent command injection
            var sanitizedUpn = _sanitizer.SanitizeString(variables.RacqUPN);
            var sanitizedDisplayName = _sanitizer.SanitizeString(variables.RacqDisplayName);
            
            return $@"
try {{
    New-CsOnlineApplicationInstance -UserPrincipalName ""{sanitizedUpn}"" -ApplicationId ""{variables.CsAppCqId}"" -DisplayName ""{sanitizedDisplayName}""
    Write-Host ""SUCCESS: Resource account created successfully""
}}
catch {{
    Write-Host ""ERROR: Failed to create resource account: $_""
}}";
        }

        public string GetUpdateResourceAccountUsageLocationCommand(string upn, string usageLocation)
        {
            // SECURITY: Sanitize inputs
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
            // SECURITY: Sanitize all user inputs and wrap in quotes to prevent command injection
            var sanitizedUpn = _sanitizer.SanitizeString(variables.RaaaUPN);
            var sanitizedDisplayName = _sanitizer.SanitizeString(variables.RaaaDisplayName);
            
            return $@"
try {{
    New-CsOnlineApplicationInstance -UserPrincipalName ""{sanitizedUpn}"" -ApplicationId ""{variables.CsAppAaId}"" -DisplayName ""{sanitizedDisplayName}""
    Write-Host ""SUCCESS: Resource account created successfully""
}}
catch {{
    Write-Host ""ERROR: Failed to create resource account: $_""
}}";
        }

        public string GetUpdateAutoAttendantResourceAccountUsageLocationCommand(string upn, string usageLocation)
        {
            // SECURITY: Sanitize inputs
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

        public string GetAssignAutoAttendantLicenseCommand(string userId, string skuId)
        {
            // SECURITY: Sanitize inputs
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
    }
}
