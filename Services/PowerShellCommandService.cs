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

if (Get-Module -ListAvailable -Name MicrosoftTeams) {
    $teamsModule = Get-Module -ListAvailable -Name MicrosoftTeams
    $output += 'MicrosoftTeams module is available: ' + $teamsModule.Version
} else {
    $output += 'MicrosoftTeams module is NOT available, attempting to install...'
    try {
        Install-Module -Name MicrosoftTeams -Force -AllowClobber
        $output += 'MicrosoftTeams module installed successfully'
    } catch {
        $output += 'ERROR: Failed to install MicrosoftTeams module: ' + $_.Exception.Message
    }
}

if (Get-Module -ListAvailable -Name Microsoft.Graph) {
    $graphModule = Get-Module -ListAvailable -Name Microsoft.Graph
    $output += 'Microsoft.Graph module is available: ' + $graphModule.Version
} else {
    $output += 'Microsoft.Graph module is NOT available, attempting to install...'
    try {
        Install-Module -Name Microsoft.Graph -Force
        $output += 'Microsoft.Graph module installed successfully'
    } catch {
        $output += 'ERROR: Failed to install Microsoft.Graph module: ' + $_.Exception.Message
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
    exit 1
}";
        }

        public string GetConnectGraphCommand()
        {
            return @"
try {
    Connect-MgGraph -Scopes User.ReadWrite.All, Organization.Read.All, Group.ReadWrite.All, Directory.ReadWrite.All -ErrorAction Stop -NoWelcome
    $context = Get-MgContext -ErrorAction Stop
    if ($context) {
        Write-Host 'SUCCESS: Connected to Microsoft Graph'
        Write-Host ""Connected as: $($context.Account)""
    }
}
catch {
    Write-Error ""Failed to connect to Microsoft Graph: $_""
    exit 1
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
    exit 1
}";
        }

        public string GetDisconnectGraphCommand()
        {
            return @"
try {
    Disconnect-MgGraph -ErrorAction Stop
    Write-Host 'SUCCESS: Disconnected from Microsoft Graph'
}
catch {
    Write-Error ""Failed to disconnect from Microsoft Graph: $_""
    exit 1
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

Write-Host ""{ConstantsService.Messages.WaitingMessage}""
Start-Sleep -Seconds {ConstantsService.PowerShell.DefaultWaitTimeSeconds}

$global:racqid = (Get-CsOnlineUser {variables.RacqUPN}).Identity

Update-MgUser -UserId {variables.RacqUPN} -UsageLocation {variables.UsageLocation}

New-CsCallQueue `
-Name {variables.CqDisplayName} `
-RoutingMethod Attendant `
-AllowOptOut $true `
-ConferenceMode $true `
-AgentAlertTime 30 `
-LanguageId {variables.LanguageId} `
-DistributionLists $global:m365groupId `
-OverflowThreshold 15 `
-OverflowAction Disconnect `
-TimeoutThreshold 30 `
-TimeoutAction Disconnect `
-UseDefaultMusicOnHold $true `
-PresenceBasedRouting $false

Write-Host ""{ConstantsService.Messages.WaitingMessage}""
Start-Sleep -Seconds {ConstantsService.PowerShell.DefaultWaitTimeSeconds}

$cqapplicationInstanceId = (Get-CsOnlineUser {variables.RacqUPN}).Identity
$cqautoAttendantId = (Get-CsCallQueue -NameFilter {variables.CqDisplayName}).Identity
New-CsOnlineApplicationInstanceAssociation -Identities @($cqapplicationInstanceId) -ConfigurationId $cqautoAttendantId -ConfigurationType CallQueue";
        }

        public string GetCreateAutoAttendantCommand(PhoneManagerVariables variables)
        {
            return $@"
New-CsOnlineApplicationInstance -UserPrincipalName {variables.RaaaUPN} -ApplicationId {variables.CsAppAaId} -DisplayName {variables.RaaaDisplayName}

Write-Host ""{ConstantsService.Messages.WaitingMessage}""
Start-Sleep -Seconds {ConstantsService.PowerShell.DefaultWaitTimeSeconds}

Update-MgUser -UserId {variables.RaaaUPN} -UsageLocation {variables.UsageLocation}

$SkuId = ""{variables.SkuId}""
Set-MgUserLicense -UserId {variables.RaaaUPN} -AddLicenses @{{SkuId = $SkuId}} -RemoveLicenses @()

Write-Host ""{ConstantsService.Messages.LicenseWaitingMessage}""
Start-Sleep -Seconds {ConstantsService.PowerShell.DefaultWaitTimeSeconds}

Set-CsPhoneNumberAssignment -Identity {variables.RaaaUPN} -PhoneNumber {variables.RaaAnr} -PhoneNumberType {variables.PhoneNumberType}

$racqUser = Get-CsOnlineUser {variables.RacqUPN}
if (-not $racqUser) {{
    throw ""Resource account {variables.RacqUPN} not found. Please ensure it was created successfully.""
}}
$racqid = $racqUser.Identity
$aacalltarget = New-CsAutoAttendantCallableEntity -Identity $racqid -Type ApplicationEndpoint
$aamenuOptionDisconnect = New-CsAutoAttendantMenuOption -Action TransferCallToTarget -CallTarget $aacalltarget -DtmfResponse Automatic
$aadefaultMenu = New-CsAutoAttendantMenu -Name ""Default menu"" -MenuOptions $aamenuOptionDisconnect
$aadefaultCallFlow = New-CsAutoAttendantCallFlow -Name ""Default call flow"" -Greetings @(""{variables.DefaultCallFlowGreetingPromptDE}"") -Menu $aadefaultMenu

$aaAfterHoursMenuOption = New-CsAutoAttendantMenuOption -Action DisconnectCall -DtmfResponse Automatic
$aaafterHoursMenu = New-CsAutoAttendantMenu -Name ""After Hours menu"" -MenuOptions @($aaAfterHoursMenuOption)
$aaafterHoursCallFlow = New-CsAutoAttendantCallFlow -Name ""After Hours call flow"" -Greetings @(""{variables.AfterHoursCallFlowGreetingPromptDE}"") -Menu $aaafterHoursMenu
$aaafterHoursSchedule = New-CsOnlineSchedule -Name ""After Hours Schedule"" -WeeklyRecurrentSchedule -MondayHours @({{Start = ""{variables.OpeningHours1Start:hh\:mm}"", End = ""{variables.OpeningHours1End:hh\:mm}""}}, {{Start = ""{variables.OpeningHours2Start:hh\:mm}"", End = ""{variables.OpeningHours2End:hh\:mm}""}}) -TuesdayHours @({{Start = ""{variables.OpeningHours1Start:hh\:mm}"", End = ""{variables.OpeningHours1End:hh\:mm}""}}, {{Start = ""{variables.OpeningHours2Start:hh\:mm}"", End = ""{variables.OpeningHours2End:hh\:mm}""}}) -WednesdayHours @({{Start = ""{variables.OpeningHours1Start:hh\:mm}"", End = ""{variables.OpeningHours1End:hh\:mm}""}}, {{Start = ""{variables.OpeningHours2Start:hh\:mm}"", End = ""{variables.OpeningHours2End:hh\:mm}""}}) -ThursdayHours @({{Start = ""{variables.OpeningHours1Start:hh\:mm}"", End = ""{variables.OpeningHours1End:hh\:mm}""}}, {{Start = ""{variables.OpeningHours2Start:hh\:mm}"", End = ""{variables.OpeningHours2End:hh\:mm}""}}) -FridayHours @({{Start = ""{variables.OpeningHours1Start:hh\:mm}"", End = ""{variables.OpeningHours1End:hh\:mm}""}}, {{Start = ""{variables.OpeningHours2Start:hh\:mm}"", End = ""{variables.OpeningHours2End:hh\:mm}""}}) -Complement
$aaafterHoursCallHandlingAssociation = New-CsAutoAttendantCallHandlingAssociation -Type AfterHours -ScheduleId $aaafterHoursSchedule.Id -CallFlowId $aaafterHoursCallFlow.Id
New-CsAutoAttendant -Name {variables.AaDisplayName} -DefaultCallFlow $aadefaultCallFlow -CallFlows @($aaafterHoursCallFlow) -CallHandlingAssociations @($aaafterHoursCallHandlingAssociation) -LanguageId {variables.LanguageId} -TimeZoneId {variables.TimeZoneId}

Write-Host ""{ConstantsService.Messages.WaitingMessage}""
Start-Sleep -Seconds {ConstantsService.PowerShell.DefaultWaitTimeSeconds}

$aaapplicationInstanceId = (Get-CsOnlineUser {variables.RaaaUPN}).Identity
$aaautoAttendantId = (Get-CsAutoAttendant -NameFilter {variables.AaDisplayName}).Identity
New-CsOnlineApplicationInstanceAssociation -Identities @($aaapplicationInstanceId) -ConfigurationId $aaautoAttendantId -ConfigurationType AutoAttendant";
        }

        public string GetCreateHolidayCommand(string holidayName, DateTime holidayDate)
        {
            return $@"
$HolidayDateRange = New-CsOnlineDateTimeRange -Start ""{holidayDate:dd.MM.yyyy HH:mm}""
New-CsOnlineSchedule -Name ""{holidayName}"" -FixedSchedule -DateTimeRanges @($HolidayDateRange)";
        }

        public string GetAttachHolidayToAutoAttendantCommand(string holidayName, string aaDisplayName, string holidayGreetingPrompt)
        {
            return $@"
$HolidaySchedule = Get-CsOnlineSchedule | Where-Object {{$_.Name -eq ""{holidayName}""}}
$HolidayScheduleName = $HolidaySchedule.Name
$HolidayMenuOption = New-CsAutoAttendantMenuOption -Action DisconnectCall -DtmfResponse Automatic
$HolidayMenu = New-CsAutoAttendantMenu -Name $HolidayScheduleName -MenuOptions @($HolidayMenuOption)
$HolidayCallFlow = New-CsAutoAttendantCallFlow -Name $HolidayScheduleName -Menu $HolidayMenu -Greetings ""{holidayGreetingPrompt}""
$HolidayCallHandlingAssociation = New-CsAutoAttendantCallHandlingAssociation -Type Holiday -ScheduleId $HolidaySchedule.Id -CallFlowId $HolidayCallFlow.Id
$HolidayAutoAttendant = Get-CsAutoAttendant -NameFilter ""{aaDisplayName}""
$HolidayAutoAttendant.CallFlows += @($HolidayCallFlow)
$HolidayAutoAttendant.CallHandlingAssociations += @($HolidayCallHandlingAssociation)
Set-CsAutoAttendant -Instance $HolidayAutoAttendant";
        }

        public string GetRetrieveM365GroupsCommand()
        {
            return @"
try {
    $groups = Get-MgGroup -Filter ""startswith(displayName,'ttgrp')"" -All
    if ($groups) {
        Write-Host ""SUCCESS: Found $($groups.Count) groups starting with 'ttgrp'""
        foreach ($group in $groups) {
            Write-Host ""GROUP: $($group.DisplayName)|$($group.Id)|$($group.MailNickname)|$($group.Description)""
        }
    } else {
        Write-Host ""INFO: No groups found starting with 'ttgrp'""
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
    $resourceAccounts = Get-MgUser -Filter ""startswith(userPrincipalName,'racq-')"" -Property Id,DisplayName,UserPrincipalName,UsageLocation
    if ($resourceAccounts) {
        Write-Host ""SUCCESS: Found $($resourceAccounts.Count) resource accounts starting with 'racq-'""
        foreach ($account in $resourceAccounts) {
            Write-Host ""RESOURCEACCOUNT: $($account.DisplayName)|$($account.UserPrincipalName)|$($account.Id)|$($account.UsageLocation)""
        }
    } else {
        Write-Host ""INFO: No resource accounts found starting with 'racq-'""
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
    $callQueues = Get-CsCallQueue | Where-Object {$_.Name -like '*cq-*'}
    if ($callQueues) {
        Write-Host ""SUCCESS: Found $($callQueues.Count) call queues containing 'cq-'""
        foreach ($queue in $callQueues) {
            Write-Host ""CALLQUEUE: $($queue.Name)|$($queue.Identity)|$($queue.RoutingMethod)|$($queue.AgentAlertTime)""
        }
    } else {
        Write-Host ""INFO: No call queues found containing 'cq-'""
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
    Set-MgUserLicense -UserId {userId} -AddLicenses @{{SkuId = ""{skuId}""}} -RemoveLicenses @()
    Write-Host ""SUCCESS: License assigned to user {userId} successfully""
}}
catch {{
    Write-Host ""ERROR: Failed to assign license to user {userId}: $_""
}}";
        }
    }
}
