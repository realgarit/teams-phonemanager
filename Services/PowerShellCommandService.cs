using teams_phonemanager.Models;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Services
{
    public class PowerShellCommandService : IPowerShellCommandService
    {
        private readonly IPowerShellSanitizationService _sanitizer;

        public PowerShellCommandService(IPowerShellSanitizationService sanitizer)
        {
            _sanitizer = sanitizer;
        }

        public string GetCheckModulesCommand()
        {
            return @"
$ErrorActionPreference = 'Stop'
$output = @()

# Force MSAL and Azure.Identity to use system browser instead of WAM
$env:MSAL_DISABLE_WAM = 'true'
$env:AZURE_IDENTITY_DISABLE_WAM = 'true'

# Enable TLS 1.2 for secure communication
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$output += 'TLS 1.2 security protocol enabled'

# Add bundled modules to PSModulePath
# Get the application's base directory (where the executable is located)
$appDir = [System.IO.Path]::GetDirectoryName([System.Reflection.Assembly]::GetExecutingAssembly().Location)
if (-not $appDir) {
    $appDir = [System.AppDomain]::CurrentDomain.BaseDirectory
}
$possiblePaths = @(
    (Join-Path $appDir 'Modules'),
    (Join-Path $appDir 'win-x64\Modules'),
    (Join-Path $appDir 'osx-x64\Modules'),
    (Join-Path $appDir 'osx-arm64\Modules'),
    (Join-Path $appDir 'linux-x64\Modules'),
    (Join-Path $appDir '..\Modules'),
    (Join-Path $appDir '..\win-x64\Modules'),
    (Join-Path $appDir '..\osx-x64\Modules'),
    (Join-Path $appDir '..\osx-arm64\Modules'),
    (Join-Path $appDir '..\linux-x64\Modules')
)

$bundledModulesPath = $null
foreach ($path in $possiblePaths) {
    if (Test-Path $path) {
        $bundledModulesPath = $path
        break
    }
}

if ($bundledModulesPath) {
    $pathSeparator = [System.IO.Path]::PathSeparator
    $env:PSModulePath = $bundledModulesPath + $pathSeparator + $env:PSModulePath
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

        private string GetCommonSetupScript()
        {
            return @"
# Common Setup
$ErrorActionPreference = 'Stop'

# Force MSAL and Azure.Identity to use system browser instead of WAM (Legacy/Compat)
$env:MSAL_DISABLE_WAM = 'true'
$env:AZURE_IDENTITY_DISABLE_WAM = 'true'

# Enable TLS 1.2
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

# Add bundled modules to PSModulePath
$appDir = [System.IO.Path]::GetDirectoryName([System.Reflection.Assembly]::GetExecutingAssembly().Location)
if (-not $appDir) {
    $appDir = [System.AppDomain]::CurrentDomain.BaseDirectory
}
$possiblePaths = @(
    (Join-Path $appDir 'Modules'),
    (Join-Path $appDir 'win-x64\Modules'),
    (Join-Path $appDir 'osx-x64\Modules'),
    (Join-Path $appDir 'osx-arm64\Modules'),
    (Join-Path $appDir 'linux-x64\Modules'),
    (Join-Path $appDir '..\Modules'),
    (Join-Path $appDir '..\win-x64\Modules'),
    (Join-Path $appDir '..\osx-x64\Modules'),
    (Join-Path $appDir '..\osx-arm64\Modules'),
    (Join-Path $appDir '..\linux-x64\Modules')
)

$bundledModulesPath = $null
foreach ($path in $possiblePaths) {
    if (Test-Path $path) {
        $bundledModulesPath = $path
        break
    }
}

if ($bundledModulesPath) {
    $pathSeparator = [System.IO.Path]::PathSeparator
    $currentPaths = $env:PSModulePath -split $pathSeparator
    if ($bundledModulesPath -notin $currentPaths) {
        $env:PSModulePath = $bundledModulesPath + $pathSeparator + $env:PSModulePath
    }
}
";
        }

        public string GetConnectTeamsCommand()
        {
            return GetCommonSetupScript() + @"
try {
    # Explicitly import MicrosoftTeams to ensure cmdlets are available
    Import-Module " + ConstantsService.PowerShellModules.MicrosoftTeams + @" -Force -ErrorAction Stop

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
            // This method is kept for backwards compatibility but should not be called directly.
            // Use GetConnectGraphWithTokenCommand instead.
            return GetConnectGraphWithTokenCommand("");
        }

        /// <summary>
        /// Connects to Microsoft Graph using a pre-obtained access token from MSAL.
        /// This bypasses WAM issues on Windows by handling authentication in C#.
        /// </summary>
        public string GetConnectGraphWithTokenCommand(string accessToken)
        {
            // The access token is passed as a SecureString in PowerShell
            // We need to convert it properly
            return GetCommonSetupScript() + @"
try {
    # Import Microsoft.Graph modules
    Import-Module " + ConstantsService.PowerShellModules.MicrosoftGraphAuthentication + @" -Force
    Import-Module " + ConstantsService.PowerShellModules.MicrosoftGraphUsers + @" -Force
    Import-Module " + ConstantsService.PowerShellModules.MicrosoftGraphUsersActions + @" -Force
    Import-Module " + ConstantsService.PowerShellModules.MicrosoftGraphGroups + @" -Force
    Import-Module " + ConstantsService.PowerShellModules.MicrosoftGraphIdentityDirectoryManagement + @" -Force
    
    # Convert the access token to SecureString as required by Connect-MgGraph
    $tokenSecure = ConvertTo-SecureString -String '" + accessToken + @"' -AsPlainText -Force
    
    Connect-MgGraph -AccessToken $tokenSecure -ErrorAction Stop -NoWelcome
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
            var sanitizedGroupName = _sanitizer.SanitizeString(groupName);

            return $@"
$existingGroup = Get-MgGroup -Filter ""displayName eq '{sanitizedGroupName}'"" -ErrorAction SilentlyContinue
if ($existingGroup)
{{
    Write-Host ""{sanitizedGroupName} already exists. Please check, otherwise SFO will be salty!""
    $global:m365groupId = $existingGroup.Id
    Write-Host ""{sanitizedGroupName} was found successfully with ID: $global:m365groupId""
    return
}}

try {{
    $newGroup = New-MgGroup -DisplayName ""{sanitizedGroupName}"" `
        -MailEnabled:$True `
        -MailNickName ""{sanitizedGroupName}"" `
        -SecurityEnabled `
        -GroupTypes @(""Unified"")

    $global:m365groupId = $newGroup.Id
    Write-Host ""{sanitizedGroupName} created successfully with ID: $global:m365groupId""
}}
catch {{
    Write-Host ""{sanitizedGroupName} failed to create: $_""
    exit
}}";
        }

        public string GetCreateCallQueueCommand(PhoneManagerVariables variables)
        {
            var callQueueParams = BuildCallQueueParameters(variables);
            var sanitizedRacqUPN = _sanitizer.SanitizeIdentifier(variables.RacqUPN);
            var sanitizedRacqDisplayName = _sanitizer.SanitizeString(variables.RacqDisplayName);
            var sanitizedCqDisplayName = _sanitizer.SanitizeString(variables.CqDisplayName);
            var sanitizedUsageLocation = _sanitizer.SanitizeString(variables.UsageLocation);
            var sanitizedLanguageId = _sanitizer.SanitizeString(variables.LanguageId);

            return $@"
New-CsOnlineApplicationInstance -UserPrincipalName {sanitizedRacqUPN} -ApplicationId {variables.CsAppCqId} -DisplayName {sanitizedRacqDisplayName}

Write-Host """ + ConstantsService.Messages.WaitingMessage + @"""
Start-Sleep -Seconds " + ConstantsService.PowerShell.DefaultWaitTimeSeconds + @"

$global:racqid = (Get-CsOnlineUser {sanitizedRacqUPN}).Identity

Update-MgUser -UserId {sanitizedRacqUPN} -UsageLocation {sanitizedUsageLocation}

New-CsCallQueue `
-Name {sanitizedCqDisplayName} `
-RoutingMethod Attendant `
-AllowOptOut $true `
-ConferenceMode $true `
-AgentAlertTime " + ConstantsService.CallQueue.AgentAlertTime + @" `
-LanguageId {sanitizedLanguageId} `
-DistributionLists $global:m365groupId `
{callQueueParams}
-PresenceBasedRouting $false

Write-Host """ + ConstantsService.Messages.WaitingMessage + @"""
Start-Sleep -Seconds " + ConstantsService.PowerShell.DefaultWaitTimeSeconds + @"

$cqapplicationInstanceId = (Get-CsOnlineUser {sanitizedRacqUPN}).Identity
$cqautoAttendantId = (Get-CsCallQueue -NameFilter {sanitizedCqDisplayName}).Identity
New-CsOnlineApplicationInstanceAssociation -Identities @($cqapplicationInstanceId) -ConfigurationId $cqautoAttendantId -ConfigurationType CallQueue";
        }

        private string BuildCallQueueParameters(PhoneManagerVariables variables)
        {
            var parameters = new System.Text.StringBuilder();

            // Greeting
            if (variables.CqGreetingType == "AudioFile" && !string.IsNullOrWhiteSpace(variables.CqGreetingAudioFileId))
            {
                var sanitizedAudioFileId = _sanitizer.SanitizeString(variables.CqGreetingAudioFileId);
                parameters.AppendLine($"-WelcomeMusicAudioFileId \"{sanitizedAudioFileId}\" `");
            }
            else if (variables.CqGreetingType == "TextToSpeech" && !string.IsNullOrWhiteSpace(variables.CqGreetingTextToSpeechPrompt))
            {
                var sanitizedPrompt = _sanitizer.SanitizeString(variables.CqGreetingTextToSpeechPrompt);
                parameters.AppendLine($"-WelcomeTextToSpeechPrompt \"{sanitizedPrompt}\" `");
            }

            // Music on Hold
            if (variables.CqMusicOnHoldType == "AudioFile" && !string.IsNullOrWhiteSpace(variables.CqMusicOnHoldAudioFileId))
            {
                var sanitizedAudioFileId = _sanitizer.SanitizeString(variables.CqMusicOnHoldAudioFileId);
                parameters.AppendLine($"-MusicOnHoldAudioFileId \"{sanitizedAudioFileId}\" `");
                parameters.AppendLine($"-UseDefaultMusicOnHold $false `");
            }
            else
            {
                // Explicitly set UseDefaultMusicOnHold to true when "Default" is selected
                parameters.AppendLine($"-UseDefaultMusicOnHold $true `");
            }

            // Overflow
            var overflowThreshold = variables.CqOverflowThreshold ?? ConstantsService.CallQueue.OverflowThreshold;
            parameters.AppendLine($"-OverflowThreshold {overflowThreshold} `");

            if (!string.IsNullOrWhiteSpace(variables.CqOverflowAction))
            {
                if (variables.CqOverflowAction == "Disconnect")
                {
                    parameters.AppendLine($"-OverflowAction DisconnectWithBusy `");
                }
                else if (variables.CqOverflowAction == "TransferToTarget" && !string.IsNullOrWhiteSpace(variables.CqOverflowActionTarget))
                {
                    var sanitizedTarget = _sanitizer.SanitizeString(variables.CqOverflowActionTarget);
                    parameters.AppendLine($"-OverflowAction Forward `");
                    parameters.AppendLine($"-OverflowActionTarget \"{sanitizedTarget}\" `");
                }
                else if (variables.CqOverflowAction == "TransferToVoicemail" && !string.IsNullOrWhiteSpace(variables.CqOverflowActionTarget))
                {
                    var sanitizedTarget = _sanitizer.SanitizeString(variables.CqOverflowActionTarget);
                    if (System.Guid.TryParse(variables.CqOverflowActionTarget, out _))
                    {
                        parameters.AppendLine($"-OverflowAction SharedVoicemail `");
                        parameters.AppendLine($"-OverflowActionTarget \"{sanitizedTarget}\" `");
                        if (variables.CqOverflowVoicemailGreetingType == "TextToSpeech" && !string.IsNullOrWhiteSpace(variables.CqOverflowActionTextToSpeechPrompt))
                        {
                            var sanitizedPrompt = _sanitizer.SanitizeString(variables.CqOverflowActionTextToSpeechPrompt);
                            parameters.AppendLine($"-OverflowSharedVoicemailTextToSpeechPrompt \"{sanitizedPrompt}\" `");
                        }
                        else if (variables.CqOverflowVoicemailGreetingType == "AudioFile" && !string.IsNullOrWhiteSpace(variables.CqOverflowActionAudioFileId))
                        {
                            var sanitizedFileId = _sanitizer.SanitizeString(variables.CqOverflowActionAudioFileId);
                            parameters.AppendLine($"-OverflowSharedVoicemailAudioFilePrompt \"{sanitizedFileId}\" `");
                        }
                    }
                    else
                    {
                        parameters.AppendLine($"-OverflowAction Voicemail `");
                        parameters.AppendLine($"-OverflowActionTarget \"{sanitizedTarget}\" `");
                    }
                }
            }
            else
            {
                parameters.AppendLine($"-OverflowAction DisconnectWithBusy `");
            }

            // Timeout
            var timeoutThreshold = variables.CqTimeoutThreshold ?? ConstantsService.CallQueue.TimeoutThreshold;
            if (timeoutThreshold < ConstantsService.CallQueue.MinTimeoutThreshold)
            {
                timeoutThreshold = ConstantsService.CallQueue.MinTimeoutThreshold;
            }
            parameters.AppendLine($"-TimeoutThreshold {timeoutThreshold} `");

            if (!string.IsNullOrWhiteSpace(variables.CqTimeoutAction))
            {
                if (variables.CqTimeoutAction == "Disconnect")
                {
                    parameters.AppendLine($"-TimeoutAction Disconnect `");
                }
                else if (variables.CqTimeoutAction == "TransferToTarget" && !string.IsNullOrWhiteSpace(variables.CqTimeoutActionTarget))
                {
                    var sanitizedTarget = _sanitizer.SanitizeString(variables.CqTimeoutActionTarget);
                    parameters.AppendLine($"-TimeoutAction Forward `");
                    parameters.AppendLine($"-TimeoutActionTarget \"{sanitizedTarget}\" `");
                }
                else if (variables.CqTimeoutAction == "TransferToVoicemail" && !string.IsNullOrWhiteSpace(variables.CqTimeoutActionTarget))
                {
                    var sanitizedTarget = _sanitizer.SanitizeString(variables.CqTimeoutActionTarget);
                    if (System.Guid.TryParse(variables.CqTimeoutActionTarget, out _))
                    {
                        parameters.AppendLine($"-TimeoutAction SharedVoicemail `");
                        parameters.AppendLine($"-TimeoutActionTarget \"{sanitizedTarget}\" `");
                        if (variables.CqTimeoutVoicemailGreetingType == "TextToSpeech" && !string.IsNullOrWhiteSpace(variables.CqTimeoutActionTextToSpeechPrompt))
                        {
                            var sanitizedPrompt = _sanitizer.SanitizeString(variables.CqTimeoutActionTextToSpeechPrompt);
                            parameters.AppendLine($"-TimeoutSharedVoicemailTextToSpeechPrompt \"{sanitizedPrompt}\" `");
                        }
                        else if (variables.CqTimeoutVoicemailGreetingType == "AudioFile" && !string.IsNullOrWhiteSpace(variables.CqTimeoutActionAudioFileId))
                        {
                            var sanitizedFileId = _sanitizer.SanitizeString(variables.CqTimeoutActionAudioFileId);
                            parameters.AppendLine($"-TimeoutSharedVoicemailAudioFilePrompt \"{sanitizedFileId}\" `");
                        }
                    }
                    else
                    {
                        parameters.AppendLine($"-TimeoutAction Voicemail `");
                        parameters.AppendLine($"-TimeoutActionTarget \"{sanitizedTarget}\" `");
                    }
                }
            }
            else
            {
                parameters.AppendLine($"-TimeoutAction Disconnect `");
            }

            // No Agent
            if (!string.IsNullOrWhiteSpace(variables.CqNoAgentAction) && variables.CqNoAgentAction != "QueueCall")
            {
                if (variables.CqNoAgentAction == "Disconnect")
                {
                    parameters.AppendLine($"-NoAgentAction Disconnect `");
                }
                else if (variables.CqNoAgentAction == "TransferToTarget" && !string.IsNullOrWhiteSpace(variables.CqNoAgentActionTarget))
                {
                    var sanitizedTarget = _sanitizer.SanitizeString(variables.CqNoAgentActionTarget);
                    parameters.AppendLine($"-NoAgentAction Forward `");
                    parameters.AppendLine($"-NoAgentActionTarget \"{sanitizedTarget}\" `");
                }
                else if (variables.CqNoAgentAction == "TransferToVoicemail" && !string.IsNullOrWhiteSpace(variables.CqNoAgentActionTarget))
                {
                    var sanitizedTarget = _sanitizer.SanitizeString(variables.CqNoAgentActionTarget);
                    if (System.Guid.TryParse(variables.CqNoAgentActionTarget, out _))
                    {
                        parameters.AppendLine($"-NoAgentAction SharedVoicemail `");
                        parameters.AppendLine($"-NoAgentActionTarget \"{sanitizedTarget}\" `");
                        if (variables.CqNoAgentVoicemailGreetingType == "TextToSpeech" && !string.IsNullOrWhiteSpace(variables.CqNoAgentActionTextToSpeechPrompt))
                        {
                            var sanitizedPrompt = _sanitizer.SanitizeString(variables.CqNoAgentActionTextToSpeechPrompt);
                            parameters.AppendLine($"-NoAgentSharedVoicemailTextToSpeechPrompt \"{sanitizedPrompt}\" `");
                        }
                        else if (variables.CqNoAgentVoicemailGreetingType == "AudioFile" && !string.IsNullOrWhiteSpace(variables.CqNoAgentActionAudioFileId))
                        {
                            var sanitizedFileId = _sanitizer.SanitizeString(variables.CqNoAgentActionAudioFileId);
                            parameters.AppendLine($"-NoAgentSharedVoicemailAudioFilePrompt \"{sanitizedFileId}\" `");
                        }
                    }
                    else
                    {
                        parameters.AppendLine($"-NoAgentAction Voicemail `");
                        parameters.AppendLine($"-NoAgentActionTarget \"{sanitizedTarget}\" `");
                    }
                }

                if (variables.CqNoAgentApplyToNewCallsOnly)
                {
                    parameters.AppendLine($"-NoAgentApplyTo NewCalls `");
                }
                else
                {
                    parameters.AppendLine($"-NoAgentApplyTo AllCalls `");
                }
            }

            return parameters.ToString().TrimEnd();
        }


        public string GetCreateAutoAttendantCommand(PhoneManagerVariables variables)
        {
            var defaultCallFlow = BuildCallFlow(variables, "Default", variables.AaDefaultGreetingType, variables.AaDefaultGreetingTextToSpeechPrompt, variables.AaDefaultGreetingAudioFileId, variables.AaDefaultAction, variables.AaDefaultActionTarget);
            var afterHoursCallFlow = BuildCallFlow(variables, "After Hours", variables.AaAfterHoursGreetingType, variables.AaAfterHoursGreetingTextToSpeechPrompt, variables.AaAfterHoursGreetingAudioFileId, variables.AaAfterHoursAction, variables.AaAfterHoursActionTarget);

            var sanitizedRaaaUPN = _sanitizer.SanitizeIdentifier(variables.RaaaUPN);
            var sanitizedRaaaDisplayName = _sanitizer.SanitizeString(variables.RaaaDisplayName);
            var sanitizedUsageLocation = _sanitizer.SanitizeString(variables.UsageLocation);
            var sanitizedSkuId = _sanitizer.SanitizeString(variables.SkuId);
            var sanitizedPhoneNumber = _sanitizer.SanitizeString(variables.RaaAnr);
            var sanitizedPhoneNumberType = _sanitizer.SanitizeString(variables.PhoneNumberType);
            var sanitizedAaDisplayName = _sanitizer.SanitizeString(variables.AaDisplayName);
            var sanitizedLanguageId = _sanitizer.SanitizeString(variables.LanguageId);
            var sanitizedTimeZoneId = _sanitizer.SanitizeString(variables.TimeZoneId);

            return $@"
New-CsOnlineApplicationInstance -UserPrincipalName ""{sanitizedRaaaUPN}"" -ApplicationId ""{variables.CsAppAaId}"" -DisplayName ""{sanitizedRaaaDisplayName}""

Update-MgUser -UserId ""{sanitizedRaaaUPN}"" -UsageLocation ""{sanitizedUsageLocation}""

$SkuId = ""{sanitizedSkuId}""
Set-MgUserLicense -UserId ""{sanitizedRaaaUPN}"" -AddLicenses @{{SkuId = $SkuId}} -RemoveLicenses @()

Set-CsPhoneNumberAssignment -Identity ""{sanitizedRaaaUPN}"" -PhoneNumber ""{sanitizedPhoneNumber}"" -PhoneNumberType ""{sanitizedPhoneNumberType}""

{defaultCallFlow}

{afterHoursCallFlow}

$aaafterHoursSchedule = New-CsOnlineSchedule -Name ""After Hours Schedule"" -WeeklyRecurrentSchedule -MondayHours @(@{{Start=""{variables.OpeningHours1Start:hh\:mm}""; End=""{variables.OpeningHours1End:hh\:mm}""}}, @{{Start=""{variables.OpeningHours2Start:hh\:mm}""; End=""{variables.OpeningHours2End:hh\:mm}""}}) -TuesdayHours @(@{{Start=""{variables.OpeningHours1Start:hh\:mm}""; End=""{variables.OpeningHours1End:hh\:mm}""}}, @{{Start=""{variables.OpeningHours2Start:hh\:mm}""; End=""{variables.OpeningHours2End:hh\:mm}""}}) -WednesdayHours @(@{{Start=""{variables.OpeningHours1Start:hh\:mm}""; End=""{variables.OpeningHours1End:hh\:mm}""}}, @{{Start=""{variables.OpeningHours2Start:hh\:mm}""; End=""{variables.OpeningHours2End:hh\:mm}""}}) -ThursdayHours @(@{{Start=""{variables.OpeningHours1Start:hh\:mm}""; End=""{variables.OpeningHours1End:hh\:mm}""}}, @{{Start=""{variables.OpeningHours2Start:hh\:mm}""; End=""{variables.OpeningHours2End:hh\:mm}""}}) -FridayHours @(@{{Start=""{variables.OpeningHours1Start:hh\:mm}""; End=""{variables.OpeningHours1End:hh\:mm}""}}, @{{Start=""{variables.OpeningHours2Start:hh\:mm}""; End=""{variables.OpeningHours2End:hh\:mm}""}}) -Complement
$aaafterHoursCallHandlingAssociation = New-CsAutoAttendantCallHandlingAssociation -Type AfterHours -ScheduleId $aaafterHoursSchedule.Id -CallFlowId $aaafterHoursCallFlow.Id

New-CsAutoAttendant `
-Name ""{sanitizedAaDisplayName}"" `
-DefaultCallFlow $aadefaultCallFlow `
-CallFlows @($aaafterHoursCallFlow) `
-CallHandlingAssociations @($aaafterHoursCallHandlingAssociation) `
-LanguageId ""{sanitizedLanguageId}"" `
-TimeZoneId ""{sanitizedTimeZoneId}""

$aaapplicationInstanceId = (Get-CsOnlineUser ""{sanitizedRaaaUPN}"").Identity
$aaautoAttendantId = (Get-CsAutoAttendant -NameFilter ""{sanitizedAaDisplayName}"").Identity
New-CsOnlineApplicationInstanceAssociation -Identities @($aaapplicationInstanceId) -ConfigurationId $aaautoAttendantId -ConfigurationType AutoAttendant";
        }

        private string BuildCallFlow(PhoneManagerVariables variables, string flowName, string? greetingType, string? ttsPrompt, string? audioFileId, string? action, string? actionTarget)
        {
            var sb = new System.Text.StringBuilder();
            var prefix = flowName.Replace(" ", "").ToLower();
            var sanitizedFlowName = _sanitizer.SanitizeString(flowName);

            // Greeting
            string greetingVar = "$null";
            if (greetingType == "AudioFile" && !string.IsNullOrWhiteSpace(audioFileId))
            {
                var sanitizedAudioFileId = _sanitizer.SanitizeString(audioFileId);
                sb.AppendLine($"${prefix}Greeting = New-CsAutoAttendantPrompt -AudioFilePrompt \"{sanitizedAudioFileId}\"");
                greetingVar = $"@(${prefix}Greeting)";
            }
            else if (greetingType == "TextToSpeech" && !string.IsNullOrWhiteSpace(ttsPrompt))
            {
                var sanitizedTtsPrompt = _sanitizer.SanitizeString(ttsPrompt);
                sb.AppendLine($"${prefix}Greeting = New-CsAutoAttendantPrompt -TextToSpeechPrompt \"{sanitizedTtsPrompt}\"");
                greetingVar = $"@(${prefix}Greeting)";
            }
            // If None, greetingVar remains $null (or empty array for cmdlet)

            // Action
            sb.AppendLine($"${prefix}MenuOption = $null");
            if (action == "TransferToTarget" && !string.IsNullOrWhiteSpace(actionTarget))
            {
                var sanitizedActionTarget = _sanitizer.SanitizeString(actionTarget);
                // Check if target is a resource account to get Identity
                sb.AppendLine($"$targetUser = Get-CsOnlineUser \"{sanitizedActionTarget}\" -ErrorAction SilentlyContinue");
                sb.AppendLine($"if ($targetUser) {{ $targetId = $targetUser.Identity }} else {{ $targetId = \"{sanitizedActionTarget}\" }}"); // Fallback if not found or is object ID
                sb.AppendLine($"${prefix}CallTarget = New-CsAutoAttendantCallableEntity -Identity $targetId -Type ApplicationEndpoint");
                sb.AppendLine($"${prefix}MenuOption = New-CsAutoAttendantMenuOption -Action TransferCallToTarget -CallTarget ${prefix}CallTarget -DtmfResponse Automatic");
            }
            else if (action == "TransferToVoicemail" && !string.IsNullOrWhiteSpace(actionTarget))
            {
                var sanitizedActionTarget = _sanitizer.SanitizeString(actionTarget);
                // Shared Voicemail (M365 Group)
                sb.AppendLine($"${prefix}CallTarget = New-CsAutoAttendantCallableEntity -Identity \"{sanitizedActionTarget}\" -Type SharedVoicemail");
                sb.AppendLine($"${prefix}MenuOption = New-CsAutoAttendantMenuOption -Action TransferCallToTarget -CallTarget ${prefix}CallTarget -DtmfResponse Automatic");
            }
            else
            {
                // Default to Disconnect
                sb.AppendLine($"${prefix}MenuOption = New-CsAutoAttendantMenuOption -Action DisconnectCall -DtmfResponse Automatic");
            }

            sb.AppendLine($"${prefix}Menu = New-CsAutoAttendantMenu -Name \"{sanitizedFlowName} menu\" -MenuOptions @(${prefix}MenuOption)");

            if (greetingVar != "$null")
            {
                sb.AppendLine($"$aa{prefix}CallFlow = New-CsAutoAttendantCallFlow -Name \"{sanitizedFlowName} call flow\" -Greetings {greetingVar} -Menu ${prefix}Menu");
            }
            else
            {
                sb.AppendLine($"$aa{prefix}CallFlow = New-CsAutoAttendantCallFlow -Name \"{sanitizedFlowName} call flow\" -Menu ${prefix}Menu");
            }

            return sb.ToString();
        }

        public string GetCreateHolidayCommand(string holidayName, DateTime holidayDate)
        {
            var sanitizedHolidayName = _sanitizer.SanitizeString(holidayName);

            // Format date explicitly to ensure slashes are used instead of dots
            var formattedDate = holidayDate.ToString("d/M/yyyy H:mm", System.Globalization.CultureInfo.InvariantCulture);
            var formattedDateShort = holidayDate.ToString("d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);

            return $@"
try {{
    $HolidayDateRange = New-CsOnlineDateTimeRange -Start ""{formattedDate}""
    New-CsOnlineSchedule -Name ""{sanitizedHolidayName}"" -FixedSchedule -DateTimeRanges @($HolidayDateRange)
    Write-Host ""SUCCESS: Holiday {sanitizedHolidayName} created successfully for {formattedDateShort}""
}}
catch {{
    Write-Host ""ERROR: Failed to create holiday {sanitizedHolidayName}: $_""
}}";
        }

        public string GetCreateHolidaySeriesCommand(string holidayName, List<DateTime> holidayDates)
        {
            var sanitizedHolidayName = _sanitizer.SanitizeString(holidayName);

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
    New-CsOnlineSchedule -Name ""{sanitizedHolidayName}"" -FixedSchedule -DateTimeRanges @($HolidayDateRange)
    Write-Host ""SUCCESS: Holiday series {sanitizedHolidayName} created successfully for dates: {datesList}""
}}
catch {{
    Write-Host ""ERROR: Failed to create holiday series {sanitizedHolidayName}: $_""
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
            var sanitizedHolidayName = _sanitizer.SanitizeString(holidayName);
            var sanitizedAaDisplayName = _sanitizer.SanitizeString(aaDisplayName);
            var sanitizedGreetingPrompt = _sanitizer.SanitizeString(holidayGreetingPrompt);

            return $@"
try {{
    $HolidaySchedule = Get-CsOnlineSchedule | Where-Object {{$_.Name -eq ""{sanitizedHolidayName}""}}
    if (-not $HolidaySchedule) {{
        throw ""Holiday schedule '{sanitizedHolidayName}' not found. Please create the holiday first.""
    }}

    $HolidayScheduleName = $HolidaySchedule.Name
    $HolidayMenuOption = New-CsAutoAttendantMenuOption -Action DisconnectCall -DtmfResponse Automatic
    $HolidayMenu = New-CsAutoAttendantMenu -Name $HolidayScheduleName -MenuOptions @($HolidayMenuOption)
    $HolidayGreetingPromptDE = New-CsAutoAttendantPrompt -TextToSpeechPrompt ""{sanitizedGreetingPrompt}""
    $HolidayCallFlow = New-CsAutoAttendantCallFlow -Name $HolidayScheduleName -Menu $HolidayMenu -Greetings @($HolidayGreetingPromptDE)
    $HolidayCallHandlingAssociation = New-CsAutoAttendantCallHandlingAssociation -Type Holiday -ScheduleId $HolidaySchedule.Id -CallFlowId $HolidayCallFlow.Id

    $HolidayAutoAttendant = Get-CsAutoAttendant -NameFilter ""{sanitizedAaDisplayName}"" | Where-Object {{$_.Name -eq ""{sanitizedAaDisplayName}""}} | Select-Object -First 1
    if (-not $HolidayAutoAttendant) {{
        throw ""Auto Attendant '{sanitizedAaDisplayName}' not found. Please ensure it exists.""
    }}

    $HolidayAutoAttendant.CallFlows += @($HolidayCallFlow)
    $HolidayAutoAttendant.CallHandlingAssociations += @($HolidayCallHandlingAssociation)
    Set-CsAutoAttendant -Instance $HolidayAutoAttendant
    Write-Host ""SUCCESS: Holiday {sanitizedHolidayName} attached to auto attendant {sanitizedAaDisplayName} successfully""
}}
catch {{
    Write-Host ""ERROR: Failed to attach holiday {sanitizedHolidayName} to auto attendant {sanitizedAaDisplayName}: $_""
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
    Update-MgUser -UserId ""{upn}"" -UsageLocation ""{usageLocation}""
    Write-Host ""SUCCESS: Updated usage location for {upn} to {usageLocation}""
}}
catch {{
    Write-Host ""ERROR: Failed to update usage location for {upn}: $_""
}}";
        }

        public string GetCreateCallQueueCommand(string name, string languageId, string m365GroupId, PhoneManagerVariables? variables = null)
        {
            var callQueueParams = variables != null ? BuildCallQueueParameters(variables) : BuildDefaultCallQueueParameters();
            
            return $@"
try {{
    # Create Call Queue in one step
    New-CsCallQueue `
    -Name ""{name}"" `
    -RoutingMethod Attendant `
    -AllowOptOut $true `
    -ConferenceMode $true `
    -AgentAlertTime 30 `
    -LanguageId ""{languageId}"" `
    -DistributionLists @(""{m365GroupId}"") `
    {callQueueParams}
    -PresenceBasedRouting $false
    
    Write-Host ""SUCCESS: Call queue {name} created successfully""
}}
catch {{
    Write-Host ""ERROR: Failed to create call queue {name}: $_""
}}";
        }

        private string BuildDefaultCallQueueParameters()
        {
            return $@"-OverflowThreshold " + ConstantsService.CallQueue.OverflowThreshold + @" `
-OverflowAction DisconnectWithBusy `
-TimeoutThreshold " + ConstantsService.CallQueue.TimeoutThreshold + @" `
-TimeoutAction Disconnect `
-UseDefaultMusicOnHold $true `";
        }

        public string GetImportAudioFileCommand(string filePath)
        {
            // Escape single quotes for PowerShell (use single quotes to avoid issues with spaces and special chars)
            var escapedPath = filePath.Replace("'", "''");
            // Extract just the filename from the path
            var fileName = System.IO.Path.GetFileName(filePath).Replace("'", "''");
            
            return $@"
try {{
    # Read the audio file content as bytes (compatible with both PowerShell 5.x and 6+)
    $fileContent = $null
    if ($PSVersionTable.PSVersion.Major -ge 6) {{
        $fileContent = Get-Content -Path '{escapedPath}' -AsByteStream -ReadCount 0
    }} else {{
        $fileContent = Get-Content -Path '{escapedPath}' -Encoding Byte -ReadCount 0
    }}
    
    if (-not $fileContent) {{
        Write-Host ""ERROR: Failed to read audio file content""
        exit
    }}
    
    # Import the audio file (using HuntGroup ApplicationId for Call Queues)
    $audioFile = Import-CsOnlineAudioFile -ApplicationId HuntGroup -FileName '{fileName}' -Content $fileContent
    if ($audioFile) {{
        Write-Host ""SUCCESS: Audio file imported successfully""
        Write-Host ""AUDIOFILEID: $($audioFile.Id)""
    }} else {{
        Write-Host ""ERROR: Failed to import audio file - no result returned""
    }}
}}
catch {{
    Write-Host ""ERROR: Failed to import audio file: $_""
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
    Set-MgUserLicense -UserId ""{userId}"" -AddLicenses @{{SkuId = $SkuId}} -RemoveLicenses @()
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

        public string GetCreateDefaultCallFlowCommand(PhoneManagerVariables variables)
        {
            var callFlowScript = BuildCallFlow(variables, "Default", variables.AaDefaultGreetingType, variables.AaDefaultGreetingTextToSpeechPrompt, variables.AaDefaultGreetingAudioFileId, variables.AaDefaultAction, variables.AaDefaultActionTarget);
            return $@"
try {{
{callFlowScript}
    Write-Host ""SUCCESS: Created default call flow""
}}
catch {{
    Write-Host ""ERROR: Failed to create default call flow: $_""
}}";
        }

        public string GetCreateAfterHoursCallFlowCommand(PhoneManagerVariables variables)
        {
            var callFlowScript = BuildCallFlow(variables, "After Hours", variables.AaAfterHoursGreetingType, variables.AaAfterHoursGreetingTextToSpeechPrompt, variables.AaAfterHoursGreetingAudioFileId, variables.AaAfterHoursAction, variables.AaAfterHoursActionTarget);
            return $@"
try {{
{callFlowScript}
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
    
    if ($aaafterHoursSchedule) {{
        Write-Host ""SUCCESS: Created after hours schedule with ID: $($aaafterHoursSchedule.Id)""
    }} else {{
        Write-Host ""ERROR: Failed to retrieve ID after creating schedule""
    }}
}}
catch {{
    Write-Host ""ERROR: Failed to create after hours schedule: $_""
}}";
        }

        public string GetCreateCallHandlingAssociationCommand()
        {
            return $@"
try {{
    if ($aaafterHoursSchedule.Id) {{
        $aaafterHoursCallHandlingAssociation = New-CsAutoAttendantCallHandlingAssociation -Type AfterHours -ScheduleId $aaafterHoursSchedule.Id -CallFlowId $aaafterHoursCallFlow.Id
        Write-Host ""SUCCESS: Created call handling association""
    }} else {{
        Write-Host ""ERROR: Failed to create call handling association: ScheduleId is empty""
    }}
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
