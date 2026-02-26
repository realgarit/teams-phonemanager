using teams_phonemanager.Models;
using teams_phonemanager.Services.Interfaces;
using teams_phonemanager.Services;
using System.Text;

namespace teams_phonemanager.Services.ScriptBuilders
{
    public class AutoAttendantScriptBuilder
    {
        private readonly IPowerShellSanitizationService _sanitizer;

        public AutoAttendantScriptBuilder(IPowerShellSanitizationService sanitizer)
        {
            _sanitizer = sanitizer;
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

$aatr1 = New-CsOnlineTimeRange -Start ""{variables.OpeningHours1Start:hh\:mm}"" -End ""{variables.OpeningHours1End:hh\:mm}""
$aatr2 = New-CsOnlineTimeRange -Start ""{variables.OpeningHours2Start:hh\:mm}"" -End ""{variables.OpeningHours2End:hh\:mm}""
$aaTimeRanges = if (""{variables.OpeningHours2Start:hh\:mm}"" -ne ""{variables.OpeningHours2End:hh\:mm}"") {{ @($aatr1, $aatr2) }} else {{ @($aatr1) }}
$aaafterHoursSchedule = New-CsOnlineSchedule -Name ""After Hours Schedule"" -WeeklyRecurrentSchedule -MondayHours $aaTimeRanges -TuesdayHours $aaTimeRanges -WednesdayHours $aaTimeRanges -ThursdayHours $aaTimeRanges -FridayHours $aaTimeRanges -Complement
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
            var sb = new StringBuilder();
            var prefix = flowName.Replace(" ", "").ToLower();
            var sanitizedFlowName = _sanitizer.SanitizeString(flowName);

            // Greeting
            string greetingVar = "$null";
            if (greetingType == "AudioFile" && !string.IsNullOrWhiteSpace(audioFileId))
            {
                var sanitizedAudioFileId = _sanitizer.SanitizeString(audioFileId);
                // AudioFilePrompt requires an AudioFile object, not just an ID string.
                // Retrieve the audio file object first using Get-CsOnlineAudioFile.
                sb.AppendLine($"${prefix}AudioFile = Get-CsOnlineAudioFile -Identity \"{sanitizedAudioFileId}\" -ApplicationId OrgAutoAttendant");
                sb.AppendLine($"${prefix}Greeting = New-CsAutoAttendantPrompt -AudioFilePrompt ${prefix}AudioFile");
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

        public string GetVerifyAutoAttendantCommand(string aaDisplayName)
        {
            var sanitizedAaDisplayName = _sanitizer.SanitizeString(aaDisplayName);
            
            return $@"
try {{
    $AutoAttendant = Get-CsAutoAttendant -NameFilter ""{sanitizedAaDisplayName}""
    if ($AutoAttendant) {{
        Write-Host ""SUCCESS: Auto attendant found and is accessible""
        Write-Host ""Auto Attendant ID: $($AutoAttendant.Identity)""
        Write-Host ""Auto Attendant Name: $($AutoAttendant.Name)""
    }} else {{
        Write-Host ""ERROR: Auto attendant not found""
    }}
}}
catch {{
    Write-Host ""ERROR: Failed to verify auto attendant: $_""
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

        public string GetAssignPhoneNumberToAutoAttendantCommand(string upn, string phoneNumber, string phoneNumberType)
        {
            // SECURITY: Sanitize inputs
            var sanitizedUpn = _sanitizer.SanitizeString(upn);
            var sanitizedPhoneNumber = _sanitizer.SanitizeString(phoneNumber);
            var sanitizedPhoneNumberType = _sanitizer.SanitizeString(phoneNumberType);
            
            return $@"
try {{
    Set-CsPhoneNumberAssignment -Identity ""{sanitizedUpn}"" -PhoneNumber ""{sanitizedPhoneNumber}"" -PhoneNumberType ""{sanitizedPhoneNumberType}""
    Write-Host ""SUCCESS: Phone number assigned successfully""
}}
catch {{
    Write-Host ""ERROR: Failed to assign phone number: $_""
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
    $aatr1 = New-CsOnlineTimeRange -Start ""{variables.OpeningHours1Start:hh\:mm}"" -End ""{variables.OpeningHours1End:hh\:mm}""
    $aatr2 = New-CsOnlineTimeRange -Start ""{variables.OpeningHours2Start:hh\:mm}"" -End ""{variables.OpeningHours2End:hh\:mm}""
    $aaTimeRanges = if (""{variables.OpeningHours2Start:hh\:mm}"" -ne ""{variables.OpeningHours2End:hh\:mm}"") {{ @($aatr1, $aatr2) }} else {{ @($aatr1) }}
    $aaafterHoursSchedule = New-CsOnlineSchedule -Name ""After Hours Schedule"" -WeeklyRecurrentSchedule -MondayHours $aaTimeRanges -TuesdayHours $aaTimeRanges -WednesdayHours $aaTimeRanges -ThursdayHours $aaTimeRanges -FridayHours $aaTimeRanges -Complement

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
            // SECURITY: Sanitize inputs
            var sanitizedAaDisplayName = _sanitizer.SanitizeString(variables.AaDisplayName);
            var sanitizedLanguageId = _sanitizer.SanitizeString(variables.LanguageId);
            var sanitizedTimeZoneId = _sanitizer.SanitizeString(variables.TimeZoneId);
            
            return $@"
try {{
    New-CsAutoAttendant `
    -Name ""{sanitizedAaDisplayName}"" `
    -DefaultCallFlow $aadefaultCallFlow `
    -CallFlows @($aaafterHoursCallFlow) `
    -CallHandlingAssociations @($aaafterHoursCallHandlingAssociation) `
    -LanguageId ""{sanitizedLanguageId}"" `
    -TimeZoneId ""{sanitizedTimeZoneId}""
    Write-Host ""SUCCESS: Auto attendant created successfully""
}}
catch {{
    Write-Host ""ERROR: Failed to create auto attendant: $_""
}}";
        }

        public string GetAssociateResourceAccountWithAutoAttendantCommand(string resourceAccountUpn, string autoAttendantName)
        {
            // SECURITY: Sanitize inputs
            var sanitizedUpn = _sanitizer.SanitizeString(resourceAccountUpn);
            var sanitizedAaName = _sanitizer.SanitizeString(autoAttendantName);
            
            return $@"
try {{
    $aaapplicationInstanceId = (Get-CsOnlineUser ""{sanitizedUpn}"").Identity
    $aaautoAttendantId = (Get-CsAutoAttendant -NameFilter ""{sanitizedAaName}"").Identity
    New-CsOnlineApplicationInstanceAssociation -Identities @($aaapplicationInstanceId) -ConfigurationId $aaautoAttendantId -ConfigurationType AutoAttendant
    Write-Host ""SUCCESS: Associated resource account with auto attendant""
}}
catch {{
    Write-Host ""ERROR: Failed to associate resource account with auto attendant: $_""
}}";
        }
    }
}
