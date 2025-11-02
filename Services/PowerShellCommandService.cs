using teams_phonemanager.Models;

namespace teams_phonemanager.Services
{
    public class PowerShellCommandService
    {
        private static PowerShellCommandService? _instance;

        private PowerShellCommandService() { }

        public static PowerShellCommandService Instance
        {
            get
            {
                _instance ??= new PowerShellCommandService();
                return _instance;
            }
        }

        public string GetCheckModulesCommand()
        {
            return @"
$ErrorActionPreference = 'Stop'
$output = @()

# Enable TLS 1.2 for secure communication
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$output += 'TLS 1.2 security protocol enabled'

# Add bundled modules to PSModulePath
$appDir = Get-Location
$possiblePaths = @(
    (Join-Path $appDir 'Modules'),
    (Join-Path $appDir 'win-x64\Modules'),
    (Join-Path $appDir '..\Modules'),
    (Join-Path $appDir '..\win-x64\Modules')
)

$bundledModulesPath = $null
foreach ($path in $possiblePaths) {
    if (Test-Path $path) {
        $bundledModulesPath = $path
        break
    }
}

if ($bundledModulesPath) {
    $env:PSModulePath = $bundledModulesPath + ';' + $env:PSModulePath
    $output += 'Bundled modules path added to PSModulePath: ' + $bundledModulesPath
} else {
    $output += 'WARNING: Bundled modules path not found in any expected location'
    $output += 'App directory: ' + $appDir
    $output += 'Checked paths: ' + ($possiblePaths -join ', ')
    $output += 'Current location: ' + (Get-Location)
}

# Check MicrosoftTeams module
if (Get-Module -ListAvailable -Name " + ConstantsService.PowerShellModules.MicrosoftTeams + @") {
    $teamsModule = Get-Module -ListAvailable -Name " + ConstantsService.PowerShellModules.MicrosoftTeams + @"
    $output += 'MicrosoftTeams module is available: ' + $teamsModule.Version
} else {
    $output += 'ERROR: MicrosoftTeams module not found in bundled modules'
}

# Check Microsoft.Graph modules
$requiredGraphModules = @(
    """ + ConstantsService.PowerShellModules.MicrosoftGraphAuthentication + @""",
    """ + ConstantsService.PowerShellModules.MicrosoftGraphUsers + @""",
    """ + ConstantsService.PowerShellModules.MicrosoftGraphUsersActions + @""",
    """ + ConstantsService.PowerShellModules.MicrosoftGraphGroups + @""",
    """ + ConstantsService.PowerShellModules.MicrosoftGraphIdentityDirectoryManagement + @"""
)

foreach ($moduleName in $requiredGraphModules) {
    if (Get-Module -ListAvailable -Name $moduleName) {
        $module = Get-Module -ListAvailable -Name $moduleName
        $output += ""$moduleName module is available: $($module.Version)""
    } else {
        $output += ""ERROR: $moduleName module not found in bundled modules""
    }
}

$output | ForEach-Object { Write-Host $_ }
";
        }

        public string GetConnectTeamsCommand()
        {
            return @"
try {
    Connect-MicrosoftTeams -ErrorAction Stop
    $connection = Get-CsTenant -ErrorAction Stop
    if ($connection) {
        Write-Host 'SUCCESS: Connected to Microsoft Teams'
        Write-Host ""Connected to tenant: $($connection.DisplayName) ($($connection.TenantId))""
    }
}
catch {
    Write-Error ""Failed to connect to Microsoft Teams: $_""
    exit " + ConstantsService.PowerShell.ExitCodeError + @"
}";
        }

        public string GetConnectGraphCommand()
        {
            return @"
try {
    # Import Microsoft.Graph modules
    Import-Module " + ConstantsService.PowerShellModules.MicrosoftGraphAuthentication + @" -Force
    Import-Module " + ConstantsService.PowerShellModules.MicrosoftGraphUsers + @" -Force
    Import-Module " + ConstantsService.PowerShellModules.MicrosoftGraphUsersActions + @" -Force
    Import-Module " + ConstantsService.PowerShellModules.MicrosoftGraphGroups + @" -Force
    Import-Module " + ConstantsService.PowerShellModules.MicrosoftGraphIdentityDirectoryManagement + @" -Force
    
    Connect-MgGraph -Scopes User.ReadWrite.All, Organization.Read.All, Group.ReadWrite.All, Directory.ReadWrite.All -ErrorAction Stop -NoWelcome
    $context = Get-MgContext -ErrorAction Stop
    if ($context) {
        Write-Host 'SUCCESS: Connected to Microsoft Graph'
        Write-Host ""Connected as: $($context.Account)""
    }
}
catch {
    Write-Error ""Failed to connect to Microsoft Graph: $_""
    exit " + ConstantsService.PowerShell.ExitCodeError + @"
}";
        }

        public string GetDisconnectTeamsCommand()
        {
            return @"
try {
    Disconnect-MicrosoftTeams -ErrorAction Stop
    Write-Host 'SUCCESS: Disconnected from Microsoft Teams'
}
catch {
    Write-Error ""Failed to disconnect from Microsoft Teams: $_""
    exit " + ConstantsService.PowerShell.ExitCodeError + @"
}";
        }

        public string GetDisconnectGraphCommand()
        {
            return @"
try {
    # Import Microsoft.Graph modules
    Import-Module " + ConstantsService.PowerShellModules.MicrosoftGraphAuthentication + @" -Force
    
    Disconnect-MgGraph -ErrorAction Stop
    Write-Host 'SUCCESS: Disconnected from Microsoft Graph'
}
catch {
    Write-Error ""Failed to disconnect from Microsoft Graph: $_""
    exit " + ConstantsService.PowerShell.ExitCodeError + @"
}";
        }

        public string GetCreateM365GroupCommand(string groupName)
        {
            return $@"
$existingGroup = Get-MgGroup -Filter ""displayName eq '{groupName}'"" -ErrorAction SilentlyContinue
if ($existingGroup) 
{{
    Write-Host ""{groupName} already exists. Please check, otherwise SFO will be salty!""
    $global:m365groupId = $existingGroup.Id
    Write-Host ""{groupName} was found successfully with ID: $global:m365groupId""
    return
}} 

try {{
    $newGroup = New-MgGroup -DisplayName ""{groupName}"" `
        -MailEnabled:$False `
        -MailNickName ""{groupName}"" `
        -SecurityEnabled `
        -GroupTypes @(""Unified"")

    $global:m365groupId = $newGroup.Id
    Write-Host ""{groupName} created successfully with ID: $global:m365groupId""
}}
catch {{
    Write-Host ""{groupName} failed to create: $_""
    exit
}}";
        }

        public string GetCreateCallQueueCommand(PhoneManagerVariables variables)
        {
            return $@"
New-CsOnlineApplicationInstance -UserPrincipalName {variables.RacqUPN} -ApplicationId {variables.CsAppCqId} -DisplayName {variables.RacqDisplayName}

Write-Host """ + ConstantsService.Messages.WaitingMessage + @"""
Start-Sleep -Seconds " + ConstantsService.PowerShell.DefaultWaitTimeSeconds + @"

$global:racqid = (Get-CsOnlineUser {variables.RacqUPN}).Identity

Update-MgUser -UserId {variables.RacqUPN} -UsageLocation {variables.UsageLocation}

New-CsCallQueue `
-Name {variables.CqDisplayName} `
-RoutingMethod Attendant `
-AllowOptOut $true `
-ConferenceMode $true `
-AgentAlertTime " + ConstantsService.CallQueue.AgentAlertTime + @" `
-LanguageId {variables.LanguageId} `
-DistributionLists $global:m365groupId `
-OverflowThreshold " + ConstantsService.CallQueue.OverflowThreshold + @" `
-OverflowAction Disconnect `
-TimeoutThreshold " + ConstantsService.CallQueue.TimeoutThreshold + @" `
-TimeoutAction Disconnect `
-UseDefaultMusicOnHold $true `
-PresenceBasedRouting $false

Write-Host """ + ConstantsService.Messages.WaitingMessage + @"""
Start-Sleep -Seconds " + ConstantsService.PowerShell.DefaultWaitTimeSeconds + @"

$cqapplicationInstanceId = (Get-CsOnlineUser {variables.RacqUPN}).Identity
$cqautoAttendantId = (Get-CsCallQueue -NameFilter {variables.CqDisplayName}).Identity
New-CsOnlineApplicationInstanceAssociation -Identities @($cqapplicationInstanceId) -ConfigurationId $cqautoAttendantId -ConfigurationType CallQueue";
        }

        public string GetCreateAutoAttendantCommand(PhoneManagerVariables variables)
        {
            return $@"
New-CsOnlineApplicationInstance -UserPrincipalName ""{variables.RaaaUPN}"" -ApplicationId ""{variables.CsAppAaId}"" -DisplayName ""{variables.RaaaDisplayName}""

Update-MgUser -UserId ""{variables.RaaaUPN}"" -UsageLocation ""{variables.UsageLocation}""

$SkuId = ""{variables.SkuId}""
Set-MgUserLicense -UserId ""{variables.RaaaUPN}"" -AddLicenses @{{SkuId = $SkuId}} -RemoveLicenses @()

Set-CsPhoneNumberAssignment -Identity ""{variables.RaaaUPN}"" -PhoneNumber ""{variables.RaaAnr}"" -PhoneNumberType ""{variables.PhoneNumberType}""

$racqUser = Get-CsOnlineUser ""{variables.RacqUPN}""
if (-not $racqUser) {{
    throw ""Resource account {variables.RacqUPN} not found. Please ensure it was created successfully.""
}}
$racqid = $racqUser.Identity
$aacalltarget = New-CsAutoAttendantCallableEntity -Identity $racqid -Type ApplicationEndpoint
$aamenuOptionDisconnect = New-CsAutoAttendantMenuOption -Action TransferCallToTarget -CallTarget $aacalltarget -DtmfResponse Automatic
$aadefaultMenu = New-CsAutoAttendantMenu -Name ""Default menu"" -MenuOptions $aamenuOptionDisconnect
$aadefaultCallFlow = New-CsAutoAttendantCallFlow -Name ""Default call flow"" -Greetings @(""{variables.DefaultCallFlowGreetingPromptDE}"") -Menu $aadefaultMenu

$aaafterHoursMenuOption = New-CsAutoAttendantMenuOption -Action DisconnectCall -DtmfResponse Automatic
$aaafterHoursMenu = New-CsAutoAttendantMenu -Name ""After Hours menu"" -MenuOptions @($aaafterHoursMenuOption)
$aaafterHoursCallFlow = New-CsAutoAttendantCallFlow -Name ""After Hours call flow"" -Greetings @(""{variables.AfterHoursCallFlowGreetingPromptDE}"") -Menu $aaafterHoursMenu
$aaafterHoursSchedule = New-CsOnlineSchedule -Name ""After Hours Schedule"" -WeeklyRecurrentSchedule -MondayHours @(@{{Start=""{variables.OpeningHours1Start:hh\:mm}""; End=""{variables.OpeningHours1End:hh\:mm}""}}, @{{Start=""{variables.OpeningHours2Start:hh\:mm}""; End=""{variables.OpeningHours2End:hh\:mm}""}}) -TuesdayHours @(@{{Start=""{variables.OpeningHours1Start:hh\:mm}""; End=""{variables.OpeningHours1End:hh\:mm}""}}, @{{Start=""{variables.OpeningHours2Start:hh\:mm}""; End=""{variables.OpeningHours2End:hh\:mm}""}}) -WednesdayHours @(@{{Start=""{variables.OpeningHours1Start:hh\:mm}""; End=""{variables.OpeningHours1End:hh\:mm}""}}, @{{Start=""{variables.OpeningHours2Start:hh\:mm}""; End=""{variables.OpeningHours2End:hh\:mm}""}}) -ThursdayHours @(@{{Start=""{variables.OpeningHours1Start:hh\:mm}""; End=""{variables.OpeningHours1End:hh\:mm}""}}, @{{Start=""{variables.OpeningHours2Start:hh\:mm}""; End=""{variables.OpeningHours2End:hh\:mm}""}}) -FridayHours @(@{{Start=""{variables.OpeningHours1Start:hh\:mm}""; End=""{variables.OpeningHours1End:hh\:mm}""}}, @{{Start=""{variables.OpeningHours2Start:hh\:mm}""; End=""{variables.OpeningHours2End:hh\:mm}""}}) -Complement
$aaafterHoursCallHandlingAssociation = New-CsAutoAttendantCallHandlingAssociation -Type AfterHours -ScheduleId $aaafterHoursSchedule.Id -CallFlowId $aaafterHoursCallFlow.Id
New-CsAutoAttendant `
-Name ""{variables.AaDisplayName}"" `
-DefaultCallFlow $aadefaultCallFlow `
-CallFlows @($aaafterHoursCallFlow) `
-CallHandlingAssociations @($aaafterHoursCallHandlingAssociation) `
-LanguageId ""{variables.LanguageId}"" `
-TimeZoneId ""{variables.TimeZoneId}""

$aaapplicationInstanceId = (Get-CsOnlineUser ""{variables.RaaaUPN}"").Identity
$aaautoAttendantId = (Get-CsAutoAttendant -NameFilter ""{variables.AaDisplayName}"").Identity
New-CsOnlineApplicationInstanceAssociation -Identities @($aaapplicationInstanceId) -ConfigurationId $aaautoAttendantId -ConfigurationType AutoAttendant";
        }

        public string GetCreateHolidayCommand(string holidayName, DateTime holidayDate)
        {
            // Format date explicitly to ensure slashes are used instead of dots
            var formattedDate = holidayDate.ToString("d/M/yyyy H:mm", System.Globalization.CultureInfo.InvariantCulture);
            var formattedDateShort = holidayDate.ToString("d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            
            return $@"
try {{
    $HolidayDateRange = New-CsOnlineDateTimeRange -Start ""{formattedDate}""
    New-CsOnlineSchedule -Name ""{holidayName}"" -FixedSchedule -DateTimeRanges @($HolidayDateRange)
    Write-Host ""SUCCESS: Holiday {holidayName} created successfully for {formattedDateShort}""
}}
catch {{
    Write-Host ""ERROR: Failed to create holiday {holidayName}: $_""
}}";
        }

        public string GetCreateHolidaySeriesCommand(string holidayName, List<DateTime> holidayDates)
        {
            var formattedDateTimes = new List<string>();
            var formattedDates = new List<string>();
            
            foreach (var date in holidayDates)
            {
                var formattedDateTime = date.ToString("d/M/yyyy H:mm", System.Globalization.CultureInfo.InvariantCulture);
                var formattedDateShort = date.ToString("d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                // Use single-quoted string literals for PowerShell date strings
                formattedDateTimes.Add($"'{formattedDateTime}'");
                formattedDates.Add(formattedDateShort);
            }
            
            var datesArray = string.Join(", ", formattedDateTimes);
            var datesList = string.Join(", ", formattedDates);
            
            return $@"
try {{
    $dates = @({datesArray})
    $HolidayDateRange = foreach ($d in $dates) {{
        New-CsOnlineDateTimeRange -Start $d
    }}
    New-CsOnlineSchedule -Name ""{holidayName}"" -FixedSchedule -DateTimeRanges @($HolidayDateRange)
    Write-Host ""SUCCESS: Holiday series {holidayName} created successfully for dates: {datesList}""
}}
catch {{
    Write-Host ""ERROR: Failed to create holiday series {holidayName}: $_""
}}";
        }

        public string GetVerifyAutoAttendantCommand(string aaDisplayName)
        {
            return $@"
try {{
    $AutoAttendant = Get-CsAutoAttendant -NameFilter ""{aaDisplayName}""
    if ($AutoAttendant) {{
        Write-Host ""SUCCESS: Auto attendant '{aaDisplayName}' found and is accessible""
        Write-Host ""Auto Attendant ID: $($AutoAttendant.Identity)""
        Write-Host ""Auto Attendant Name: $($AutoAttendant.Name)""
    }} else {{
        Write-Host ""ERROR: Auto attendant '{aaDisplayName}' not found""
    }}
}}
catch {{
    Write-Host ""ERROR: Failed to verify auto attendant '{aaDisplayName}': $_""
}}";
        }

        public string GetAttachHolidayToAutoAttendantCommand(string holidayName, string aaDisplayName, string holidayGreetingPrompt)
        {
            return $@"
try {{
    $HolidaySchedule = Get-CsOnlineSchedule | Where-Object {{$_.Name -eq ""{holidayName}""}}
    if (-not $HolidaySchedule) {{
        throw ""Holiday schedule '{holidayName}' not found. Please create the holiday first.""
    }}
    
    $HolidayScheduleName = $HolidaySchedule.Name
    $HolidayMenuOption = New-CsAutoAttendantMenuOption -Action DisconnectCall -DtmfResponse Automatic
    $HolidayMenu = New-CsAutoAttendantMenu -Name $HolidayScheduleName -MenuOptions @($HolidayMenuOption)
    $HolidayGreetingPromptDE = New-CsAutoAttendantPrompt -TextToSpeechPrompt ""{holidayGreetingPrompt}""
    $HolidayCallFlow = New-CsAutoAttendantCallFlow -Name $HolidayScheduleName -Menu $HolidayMenu -Greetings @($HolidayGreetingPromptDE)
    $HolidayCallHandlingAssociation = New-CsAutoAttendantCallHandlingAssociation -Type Holiday -ScheduleId $HolidaySchedule.Id -CallFlowId $HolidayCallFlow.Id
    
    $HolidayAutoAttendant = Get-CsAutoAttendant -NameFilter ""{aaDisplayName}"" | Where-Object {{$_.Name -eq ""{aaDisplayName}""}} | Select-Object -First 1
    if (-not $HolidayAutoAttendant) {{
        throw ""Auto Attendant '{aaDisplayName}' not found. Please ensure it exists.""
    }}
    
    $HolidayAutoAttendant.CallFlows += @($HolidayCallFlow)
    $HolidayAutoAttendant.CallHandlingAssociations += @($HolidayCallHandlingAssociation)
    Set-CsAutoAttendant -Instance $HolidayAutoAttendant
    Write-Host ""SUCCESS: Holiday {holidayName} attached to auto attendant {aaDisplayName} successfully""
}}
catch {{
    Write-Host ""ERROR: Failed to attach holiday {holidayName} to auto attendant {aaDisplayName}: $_""
}}";
        }

        public string GetRetrieveM365GroupsCommand()
        {
            return @"
try {
    $groups = Get-MgGroup -Filter ""startswith(displayName,'" + ConstantsService.Naming.M365GroupPrefix + @"')"" -All
    if ($groups) {
        Write-Host ""SUCCESS: Found $($groups.Count) groups starting with '" + ConstantsService.Naming.M365GroupPrefix + @"'""
        foreach ($group in $groups) {
            Write-Host ""GROUP: $($group.DisplayName)|$($group.Id)|$($group.MailNickname)|$($group.Description)""
        }
    } else {
        Write-Host ""INFO: No groups found starting with '" + ConstantsService.Naming.M365GroupPrefix + @"'""
    }
}
catch {
    Write-Host ""ERROR: Failed to retrieve groups: $_""
}";
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

        public string GetRetrieveCallQueuesCommand()
        {
            return @"
try {
    $callQueues = Get-CsCallQueue | Where-Object {$_.Name -like '*" + ConstantsService.Naming.CallQueuePrefix + @"*'}
    if ($callQueues) {
        Write-Host ""SUCCESS: Found $($callQueues.Count) call queues containing '" + ConstantsService.Naming.CallQueuePrefix + @"'""
        foreach ($queue in $callQueues) {
            Write-Host ""CALLQUEUE: $($queue.Name)|$($queue.Identity)|$($queue.RoutingMethod)|$($queue.AgentAlertTime)""
        }
    } else {
        Write-Host ""INFO: No call queues found containing '" + ConstantsService.Naming.CallQueuePrefix + @"'""
    }
}
catch {
    Write-Host ""ERROR: Failed to retrieve call queues: $_""
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
    Update-MgUser -UserId {upn} -UsageLocation {usageLocation}
    Write-Host ""SUCCESS: Updated usage location for {upn} to {usageLocation}""
}}
catch {{
    Write-Host ""ERROR: Failed to update usage location for {upn}: $_""
}}";
        }

        public string GetCreateCallQueueCommand(string name, string languageId, string m365GroupId)
        {
            return $@"
try {{
    New-CsCallQueue `
    -Name {name} `
    -RoutingMethod Attendant `
    -AllowOptOut $true `
    -ConferenceMode $true `
    -AgentAlertTime 30 `
    -LanguageId {languageId} `
    -DistributionLists {m365GroupId} `
    -OverflowThreshold 15 `
    -OverflowAction Disconnect `
    -TimeoutThreshold 30 `
    -TimeoutAction Disconnect `
    -UseDefaultMusicOnHold $true `
    -PresenceBasedRouting $false
    
    Write-Host ""SUCCESS: Call queue {name} created successfully""
}}
catch {{
    Write-Host ""ERROR: Failed to create call queue {name}: $_""
}}";
        }

        public string GetAssociateResourceAccountWithCallQueueCommand(string resourceAccountUpn, string callQueueName)
        {
            return $@"
try {{
    $cqapplicationInstanceId = (Get-CsOnlineUser {resourceAccountUpn}).Identity
    $cqautoAttendantId = (Get-CsCallQueue -NameFilter {callQueueName}).Identity
    New-CsOnlineApplicationInstanceAssociation -Identities @($cqapplicationInstanceId) -ConfigurationId $cqautoAttendantId -ConfigurationType CallQueue
    Write-Host ""SUCCESS: Associated resource account {resourceAccountUpn} with call queue {callQueueName}""
}}
catch {{
    Write-Host ""ERROR: Failed to associate resource account {resourceAccountUpn} with call queue {callQueueName}: $_""
}}";
        }

        public string GetM365GroupIdCommand(string groupName)
        {
            return $@"
try {{
    $group = Get-MgGroup -Filter ""DisplayName eq '{groupName}'""
    if ($group) {{
        Write-Host ""SUCCESS: M365 Group ID retrieved successfully""
        Write-Host ""M365GROUPID: $($group.Id)""
    }} else {{
        Write-Host ""ERROR: M365 Group '{groupName}' not found""
    }}
}}
catch {{
    Write-Host ""ERROR: Failed to retrieve M365 Group ID for '{groupName}': $_""
}}";
        }

        public string GetAssignLicenseCommand(string userId, string skuId)
        {
            return $@"
try {{
    $SkuId = ""{skuId}""
    Set-MgUserLicense -UserId {userId} -AddLicenses @{{SkuId = $SkuId}} -RemoveLicenses @()
    Write-Host ""SUCCESS: License assigned to user {userId} successfully""
}}
catch {{
    Write-Host ""ERROR: Failed to assign license to user {userId}: $_""
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

        public string GetRetrieveAutoAttendantsCommand()
        {
            return @"
try {
    $autoAttendants = Get-CsAutoAttendant | Where-Object {$_.Name -like '*" + ConstantsService.Naming.AutoAttendantPrefix + @"*'}
    if ($autoAttendants) {
        Write-Host ""SUCCESS: Found $($autoAttendants.Count) auto attendants containing '" + ConstantsService.Naming.AutoAttendantPrefix + @"'""
        foreach ($aa in $autoAttendants) {
            Write-Host ""AUTOATTENDANT: $($aa.Name)|$($aa.Identity)|$($aa.LanguageId)|$($aa.TimeZoneId)""
        }
    } else {
        Write-Host ""INFO: No auto attendants found containing '" + ConstantsService.Naming.AutoAttendantPrefix + @"'""
    }
}
catch {
    Write-Host ""ERROR: Failed to retrieve auto attendants: $_""
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
    Update-MgUser -UserId {upn} -UsageLocation {usageLocation}
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
    Set-MgUserLicense -UserId {userId} -AddLicenses @{{SkuId = $SkuId}} -RemoveLicenses @()
    Write-Host ""SUCCESS: License assigned to user {userId} successfully""
}}
catch {{
    Write-Host ""ERROR: Failed to assign license to user {userId}: $_""
}}";
        }

        public string GetAssignPhoneNumberToAutoAttendantCommand(string upn, string phoneNumber, string phoneNumberType)
        {
            return $@"
try {{
    Set-CsPhoneNumberAssignment -Identity {upn} -PhoneNumber {phoneNumber} -PhoneNumberType {phoneNumberType}
    Write-Host ""SUCCESS: Phone number {phoneNumber} assigned to {upn} successfully""
}}
catch {{
    Write-Host ""ERROR: Failed to assign phone number {phoneNumber} to {upn}: $_""
}}";
        }



        public string GetValidateCallQueueResourceAccountCommand(string racqUpn)
        {
            return $@"
try {{
    $racqUser = Get-CsOnlineUser {racqUpn}
    if ($racqUser) {{
        Write-Host ""SUCCESS: Call Queue resource account {racqUpn} found and validated""
        Write-Host ""Account Details: $($racqUser.DisplayName) - $($racqUser.UserPrincipalName)""
    }} else {{
        Write-Host ""ERROR: Call Queue resource account {racqUpn} not found. Please create it first.""
    }}
}}
catch {{
    Write-Host ""ERROR: Failed to validate Call Queue resource account {racqUpn}: $_""
}}";
        }

        public string GetCreateCallTargetCommand(string racqUpn)
        {
            return $@"
try {{
    $racqUser = Get-CsOnlineUser ""{racqUpn}""
    if (-not $racqUser) {{
        throw ""Resource account {racqUpn} not found. Please ensure it was created successfully.""
    }}
    $racqid = $racqUser.Identity
    $aacalltarget = New-CsAutoAttendantCallableEntity -Identity $racqid -Type ApplicationEndpoint
    Write-Host ""SUCCESS: Created call target for {racqUpn}""
}}
catch {{
    Write-Host ""ERROR: Failed to create call target for {racqUpn}: $_""
}}";
        }

        public string GetCreateDefaultCallFlowCommand(string greetingText)
        {
            return $@"
try {{
    $aamenuOptionDisconnect = New-CsAutoAttendantMenuOption -Action TransferCallToTarget -CallTarget $aacalltarget -DtmfResponse Automatic
    $aadefaultMenu = New-CsAutoAttendantMenu -Name ""Default menu"" -MenuOptions $aamenuOptionDisconnect
    $aadefaultCallFlow = New-CsAutoAttendantCallFlow -Name ""Default call flow"" -Greetings @(""{greetingText}"") -Menu $aadefaultMenu
    Write-Host ""SUCCESS: Created default call flow""
}}
catch {{
    Write-Host ""ERROR: Failed to create default call flow: $_""
}}";
        }

        public string GetCreateAfterHoursCallFlowCommand(string greetingText)
        {
            return $@"
try {{
    $aaafterHoursMenuOption = New-CsAutoAttendantMenuOption -Action DisconnectCall -DtmfResponse Automatic
    $aaafterHoursMenu = New-CsAutoAttendantMenu -Name ""After Hours menu"" -MenuOptions @($aaafterHoursMenuOption)
    $aaafterHoursCallFlow = New-CsAutoAttendantCallFlow -Name ""After Hours call flow"" -Greetings @(""{greetingText}"") -Menu $aaafterHoursMenu
    Write-Host ""SUCCESS: Created after hours call flow""
}}
catch {{
    Write-Host ""ERROR: Failed to create after hours call flow: $_""
}}";
        }

        public string GetCreateAfterHoursScheduleCommand(PhoneManagerVariables variables)
        {
            return $@"
try {{
    $aaafterHoursSchedule = New-CsOnlineSchedule -Name ""After Hours Schedule"" -WeeklyRecurrentSchedule -MondayHours @(@{{Start=""{variables.OpeningHours1Start:hh\:mm}""; End=""{variables.OpeningHours1End:hh\:mm}""}}, @{{Start=""{variables.OpeningHours2Start:hh\:mm}""; End=""{variables.OpeningHours2End:hh\:mm}""}}) -TuesdayHours @(@{{Start=""{variables.OpeningHours1Start:hh\:mm}""; End=""{variables.OpeningHours1End:hh\:mm}""}}, @{{Start=""{variables.OpeningHours2Start:hh\:mm}""; End=""{variables.OpeningHours2End:hh\:mm}""}}) -WednesdayHours @(@{{Start=""{variables.OpeningHours1Start:hh\:mm}""; End=""{variables.OpeningHours1End:hh\:mm}""}}, @{{Start=""{variables.OpeningHours2Start:hh\:mm}""; End=""{variables.OpeningHours2End:hh\:mm}""}}) -ThursdayHours @(@{{Start=""{variables.OpeningHours1Start:hh\:mm}""; End=""{variables.OpeningHours1End:hh\:mm}""}}, @{{Start=""{variables.OpeningHours2Start:hh\:mm}""; End=""{variables.OpeningHours2End:hh\:mm}""}}) -FridayHours @(@{{Start=""{variables.OpeningHours1Start:hh\:mm}""; End=""{variables.OpeningHours1End:hh\:mm}""}}, @{{Start=""{variables.OpeningHours2Start:hh\:mm}""; End=""{variables.OpeningHours2End:hh\:mm}""}}) -Complement
    Write-Host ""SUCCESS: Created after hours schedule""
}}
catch {{
    Write-Host ""ERROR: Failed to create after hours schedule: $_""
}}";
        }

        public string GetCreateCallHandlingAssociationCommand()
        {
            return $@"
try {{
    $aaafterHoursCallHandlingAssociation = New-CsAutoAttendantCallHandlingAssociation -Type AfterHours -ScheduleId $aaafterHoursSchedule.Id -CallFlowId $aaafterHoursCallFlow.Id
    Write-Host ""SUCCESS: Created call handling association""
}}
catch {{
    Write-Host ""ERROR: Failed to create call handling association: $_""
}}";
        }

        public string GetCreateSimpleAutoAttendantCommand(PhoneManagerVariables variables)
        {
            return $@"
try {{
    New-CsAutoAttendant `
    -Name ""{variables.AaDisplayName}"" `
    -DefaultCallFlow $aadefaultCallFlow `
    -CallFlows @($aaafterHoursCallFlow) `
    -CallHandlingAssociations @($aaafterHoursCallHandlingAssociation) `
    -LanguageId ""{variables.LanguageId}"" `
    -TimeZoneId ""{variables.TimeZoneId}""
    Write-Host ""SUCCESS: Auto attendant {variables.AaDisplayName} created successfully""
}}
catch {{
    Write-Host ""ERROR: Failed to create auto attendant {variables.AaDisplayName}: $_""
}}";
        }

        public string GetAssociateResourceAccountWithAutoAttendantCommand(string resourceAccountUpn, string autoAttendantName)
        {
            return $@"
try {{
    $aaapplicationInstanceId = (Get-CsOnlineUser {resourceAccountUpn}).Identity
    $aaautoAttendantId = (Get-CsAutoAttendant -NameFilter {autoAttendantName}).Identity
    New-CsOnlineApplicationInstanceAssociation -Identities @($aaapplicationInstanceId) -ConfigurationId $aaautoAttendantId -ConfigurationType AutoAttendant
    Write-Host ""SUCCESS: Associated resource account {resourceAccountUpn} with auto attendant {autoAttendantName}""
}}
catch {{
    Write-Host ""ERROR: Failed to associate resource account {resourceAccountUpn} with auto attendant {autoAttendantName}: $_""
}}";
        }
    }
}
