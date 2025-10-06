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
    }
}
