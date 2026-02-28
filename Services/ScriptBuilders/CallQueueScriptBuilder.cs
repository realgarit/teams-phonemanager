using teams_phonemanager.Models;
using teams_phonemanager.Services.Interfaces;
using teams_phonemanager.Services;
using System.Text;

namespace teams_phonemanager.Services.ScriptBuilders
{
    public class CallQueueScriptBuilder
    {
        private readonly IPowerShellSanitizationService _sanitizer;

        public CallQueueScriptBuilder(IPowerShellSanitizationService sanitizer)
        {
            _sanitizer = sanitizer;
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

        public string GetCreateCallQueueCommand(PhoneManagerVariables variables)
        {
            var callQueueParams = BuildCallQueueParameters(variables);
            // SECURITY: Sanitize all user inputs and wrap in quotes
            var sanitizedRacqUPN = _sanitizer.SanitizeString(variables.RacqUPN);
            var sanitizedRacqDisplayName = _sanitizer.SanitizeString(variables.RacqDisplayName);
            var sanitizedCqDisplayName = _sanitizer.SanitizeString(variables.CqDisplayName);
            var sanitizedUsageLocation = _sanitizer.SanitizeString(variables.UsageLocation);
            var sanitizedLanguageId = _sanitizer.SanitizeString(variables.LanguageId);

            return $@"
try {{
    New-CsOnlineApplicationInstance -UserPrincipalName ""{sanitizedRacqUPN}"" -ApplicationId ""{variables.CsAppCqId}"" -DisplayName ""{sanitizedRacqDisplayName}""

    Write-Host """ + ConstantsService.Messages.WaitingMessage + @"""
    Start-Sleep -Seconds " + ConstantsService.PowerShell.DefaultWaitTimeSeconds + @"

    $global:racqid = (Get-CsOnlineUser ""{sanitizedRacqUPN}"").Identity

    Update-MgUser -UserId ""{sanitizedRacqUPN}"" -UsageLocation ""{sanitizedUsageLocation}""

    New-CsCallQueue `
    -Name ""{sanitizedCqDisplayName}"" `
    -RoutingMethod Attendant `
    -AllowOptOut $true `
    -ConferenceMode $true `
    -AgentAlertTime " + ConstantsService.CallQueue.AgentAlertTime + @" `
    -LanguageId ""{sanitizedLanguageId}"" `
    -DistributionLists $global:m365groupId `
    {callQueueParams}
    -PresenceBasedRouting $false

    Write-Host """ + ConstantsService.Messages.WaitingMessage + @"""
    Start-Sleep -Seconds " + ConstantsService.PowerShell.DefaultWaitTimeSeconds + @"

    $cqapplicationInstanceId = (Get-CsOnlineUser ""{sanitizedRacqUPN}"").Identity
    $cqautoAttendantId = (Get-CsCallQueue -NameFilter ""{sanitizedCqDisplayName}"").Identity
    New-CsOnlineApplicationInstanceAssociation -Identities @($cqapplicationInstanceId) -ConfigurationId $cqautoAttendantId -ConfigurationType CallQueue
    
    Write-Host ""SUCCESS: Call queue created successfully""
}}
catch {{
    Write-Host ""ERROR: Failed to create call queue: $_""
}}";
        }

        public string GetCreateCallQueueCommand(string name, string languageId, string m365GroupId, PhoneManagerVariables? variables = null)
        {
            var callQueueParams = variables != null ? BuildCallQueueParameters(variables) : BuildDefaultCallQueueParameters();

            // SECURITY: Sanitize all inputs
            var sanitizedName = _sanitizer.SanitizeString(name);
            var sanitizedLanguageId = _sanitizer.SanitizeString(languageId);
            var sanitizedM365GroupId = _sanitizer.SanitizeString(m365GroupId);

            return $@"
try {{
    # Create Call Queue in one step
    New-CsCallQueue `
    -Name ""{sanitizedName}"" `
    -RoutingMethod Attendant `
    -AllowOptOut $true `
    -ConferenceMode $true `
    -AgentAlertTime 30 `
    -LanguageId ""{sanitizedLanguageId}"" `
    -DistributionLists @(""{sanitizedM365GroupId}"") `
    {callQueueParams}
    -PresenceBasedRouting $false

    Write-Host ""SUCCESS: Call queue created successfully""
}}
catch {{
    Write-Host ""ERROR: Failed to create call queue: $_""
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

        public string GetAssociateResourceAccountWithCallQueueCommand(string resourceAccountUpn, string callQueueName)
        {
            var sanitizedUpn = _sanitizer.SanitizeString(resourceAccountUpn);
            var sanitizedQueueName = _sanitizer.SanitizeString(callQueueName);
            
            return $@"
try {{
    $cqapplicationInstanceId = (Get-CsOnlineUser ""{sanitizedUpn}"").Identity
    $cqautoAttendantId = (Get-CsCallQueue -NameFilter ""{sanitizedQueueName}"").Identity
    New-CsOnlineApplicationInstanceAssociation -Identities @($cqapplicationInstanceId) -ConfigurationId $cqautoAttendantId -ConfigurationType CallQueue
    Write-Host ""SUCCESS: Associated resource account with call queue""
}}
catch {{
    Write-Host ""ERROR: Failed to associate resource account with call queue: $_""
}}";
        }

        public string GetValidateCallQueueResourceAccountCommand(string racqUpn)
        {
            var sanitizedUpn = _sanitizer.SanitizeString(racqUpn);
            
            return $@"
try {{
    $racqUser = Get-CsOnlineUser ""{sanitizedUpn}""
    if ($racqUser) {{
        Write-Host ""SUCCESS: Call Queue resource account found and validated""
        Write-Host ""Account Details: $($racqUser.DisplayName) - $($racqUser.UserPrincipalName)""
    }} else {{
        Write-Host ""ERROR: Call Queue resource account not found. Please create it first.""
    }}
}}
catch {{
    Write-Host ""ERROR: Failed to validate Call Queue resource account: $_""
}}";
        }

        public string GetCreateCallTargetCommand(string racqUpn)
        {
            var sanitizedUpn = _sanitizer.SanitizeString(racqUpn);
            
            return $@"
try {{
    $racqUser = Get-CsOnlineUser ""{sanitizedUpn}""
    if (-not $racqUser) {{
        throw ""Resource account not found. Please ensure it was created successfully.""
    }}
    $racqid = $racqUser.Identity
    $aacalltarget = New-CsAutoAttendantCallableEntity -Identity $racqid -Type ApplicationEndpoint
    Write-Host ""SUCCESS: Created call target""
}}
catch {{
    Write-Host ""ERROR: Failed to create call target: $_""
}}";
        }

        /// <summary>
        /// Removes a call queue by name.
        /// </summary>
        public string GetRemoveCallQueueCommand(string callQueueName)
        {
            var sanitizedName = _sanitizer.SanitizeString(callQueueName);
            
            return $@"
try {{
    $cq = Get-CsCallQueue -NameFilter ""{sanitizedName}"" | Where-Object {{$_.Name -eq ""{sanitizedName}""}} | Select-Object -First 1
    if (-not $cq) {{
        Write-Host ""ERROR: Call queue '{sanitizedName}' not found""
    }} else {{
        Remove-CsCallQueue -Identity $cq.Identity
        Write-Host ""SUCCESS: Call queue '{sanitizedName}' removed successfully""
    }}
}}
catch {{
    Write-Host ""ERROR: Failed to remove call queue '{sanitizedName}': $_""
}}";
        }

        /// <summary>
        /// Removes a resource account (application instance) by UPN.
        /// </summary>
        public string GetRemoveResourceAccountCommand(string upn)
        {
            var sanitizedUpn = _sanitizer.SanitizeString(upn);
            
            return $@"
try {{
    $user = Get-CsOnlineUser ""{sanitizedUpn}"" -ErrorAction Stop
    if (-not $user) {{
        Write-Host ""ERROR: Resource account '{sanitizedUpn}' not found""
    }} else {{
        Remove-CsOnlineUser -Identity $user.Identity -ErrorAction Stop
        Write-Host ""SUCCESS: Resource account '{sanitizedUpn}' removed successfully""
    }}
}}
catch {{
    Write-Host ""ERROR: Failed to remove resource account '{sanitizedUpn}': $_""
}}";
        }

        private string BuildCallQueueParameters(PhoneManagerVariables variables)
        {
            var parameters = new StringBuilder();

            AppendGreetingParameters(parameters, variables);
            AppendMusicOnHoldParameters(parameters, variables);
            AppendOverflowParameters(parameters, variables);
            AppendTimeoutParameters(parameters, variables);
            AppendNoAgentParameters(parameters, variables);

            return parameters.ToString().TrimEnd();
        }

        private void AppendGreetingParameters(StringBuilder parameters, PhoneManagerVariables variables)
        {
            if (variables.CqGreetingType == "AudioFile" && !string.IsNullOrWhiteSpace(variables.CqGreetingAudioFileId))
            {
                parameters.AppendLine($"-WelcomeMusicAudioFileId \"{_sanitizer.SanitizeString(variables.CqGreetingAudioFileId)}\" `");
            }
            else if (variables.CqGreetingType == "TextToSpeech" && !string.IsNullOrWhiteSpace(variables.CqGreetingTextToSpeechPrompt))
            {
                parameters.AppendLine($"-WelcomeTextToSpeechPrompt \"{_sanitizer.SanitizeString(variables.CqGreetingTextToSpeechPrompt)}\" `");
            }
        }

        private void AppendMusicOnHoldParameters(StringBuilder parameters, PhoneManagerVariables variables)
        {
            if (variables.CqMusicOnHoldType == "AudioFile" && !string.IsNullOrWhiteSpace(variables.CqMusicOnHoldAudioFileId))
            {
                parameters.AppendLine($"-MusicOnHoldAudioFileId \"{_sanitizer.SanitizeString(variables.CqMusicOnHoldAudioFileId)}\" `");
                parameters.AppendLine($"-UseDefaultMusicOnHold $false `");
            }
            else
            {
                parameters.AppendLine($"-UseDefaultMusicOnHold $true `");
            }
        }

        private void AppendOverflowParameters(StringBuilder parameters, PhoneManagerVariables variables)
        {
            var threshold = variables.CqOverflowThreshold ?? ConstantsService.CallQueue.OverflowThreshold;
            parameters.AppendLine($"-OverflowThreshold {threshold} `");

            AppendActionParameters(parameters, 
                actionType: variables.CqOverflowAction,
                target: variables.CqOverflowActionTarget,
                prefix: "Overflow",
                voicemailGreetingType: variables.CqOverflowVoicemailGreetingType,
                voicemailTtsPrompt: variables.CqOverflowActionTextToSpeechPrompt,
                voicemailAudioFileId: variables.CqOverflowActionAudioFileId,
                defaultAction: "DisconnectWithBusy");
        }

        private void AppendTimeoutParameters(StringBuilder parameters, PhoneManagerVariables variables)
        {
            var threshold = variables.CqTimeoutThreshold ?? ConstantsService.CallQueue.TimeoutThreshold;
            if (threshold < ConstantsService.CallQueue.MinTimeoutThreshold)
            {
                threshold = ConstantsService.CallQueue.MinTimeoutThreshold;
            }
            parameters.AppendLine($"-TimeoutThreshold {threshold} `");

            AppendActionParameters(parameters, 
                actionType: variables.CqTimeoutAction,
                target: variables.CqTimeoutActionTarget,
                prefix: "Timeout",
                voicemailGreetingType: variables.CqTimeoutVoicemailGreetingType,
                voicemailTtsPrompt: variables.CqTimeoutActionTextToSpeechPrompt,
                voicemailAudioFileId: variables.CqTimeoutActionAudioFileId,
                defaultAction: "Disconnect");
        }

        private void AppendNoAgentParameters(StringBuilder parameters, PhoneManagerVariables variables)
        {
            if (string.IsNullOrWhiteSpace(variables.CqNoAgentAction) || variables.CqNoAgentAction == "QueueCall")
            {
                return; // Default behavior - no parameters needed
            }

            AppendActionParameters(parameters, 
                actionType: variables.CqNoAgentAction,
                target: variables.CqNoAgentActionTarget,
                prefix: "NoAgent",
                voicemailGreetingType: variables.CqNoAgentVoicemailGreetingType,
                voicemailTtsPrompt: variables.CqNoAgentActionTextToSpeechPrompt,
                voicemailAudioFileId: variables.CqNoAgentActionAudioFileId,
                defaultAction: null);

            // ApplyTo setting for NoAgent
            parameters.AppendLine(variables.CqNoAgentApplyToNewCallsOnly 
                ? $"-NoAgentApplyTo NewCalls `" 
                : $"-NoAgentApplyTo AllCalls `");
        }

        /// <summary>
        /// Appends action parameters for overflow, timeout, or no-agent scenarios.
        /// Consolidates common logic for Disconnect, TransferToTarget, and TransferToVoicemail actions.
        /// </summary>
        private void AppendActionParameters(
            StringBuilder parameters,
            string? actionType,
            string? target,
            string prefix,
            string? voicemailGreetingType,
            string? voicemailTtsPrompt,
            string? voicemailAudioFileId,
            string? defaultAction)
        {
            if (string.IsNullOrWhiteSpace(actionType))
            {
                if (defaultAction != null)
                {
                    parameters.AppendLine($"-{prefix}Action {defaultAction} `");
                }
                return;
            }

            switch (actionType)
            {
                case "Disconnect":
                    parameters.AppendLine($"-{prefix}Action {(prefix == "Overflow" ? "DisconnectWithBusy" : "Disconnect")} `");
                    break;

                case "TransferToTarget" when !string.IsNullOrWhiteSpace(target):
                    parameters.AppendLine($"-{prefix}Action Forward `");
                    parameters.AppendLine($"-{prefix}ActionTarget \"{_sanitizer.SanitizeString(target)}\" `");
                    break;

                case "TransferToVoicemail" when !string.IsNullOrWhiteSpace(target):
                    var sanitizedTarget = _sanitizer.SanitizeString(target);
                    var isGuid = Guid.TryParse(target, out _);
                    
                    if (isGuid)
                    {
                        parameters.AppendLine($"-{prefix}Action SharedVoicemail `");
                        parameters.AppendLine($"-{prefix}ActionTarget \"{sanitizedTarget}\" `");
                        AppendVoicemailGreetingParameters(parameters, prefix, voicemailGreetingType, voicemailTtsPrompt, voicemailAudioFileId);
                    }
                    else
                    {
                        parameters.AppendLine($"-{prefix}Action Voicemail `");
                        parameters.AppendLine($"-{prefix}ActionTarget \"{sanitizedTarget}\" `");
                    }
                    break;

                default:
                    if (defaultAction != null)
                    {
                        parameters.AppendLine($"-{prefix}Action {defaultAction} `");
                    }
                    break;
            }
        }

        /// <summary>
        /// Appends voicemail greeting parameters for shared voicemail scenarios.
        /// </summary>
        private void AppendVoicemailGreetingParameters(
            StringBuilder parameters,
            string prefix,
            string? greetingType,
            string? ttsPrompt,
            string? audioFileId)
        {
            if (greetingType == "TextToSpeech" && !string.IsNullOrWhiteSpace(ttsPrompt))
            {
                parameters.AppendLine($"-{prefix}SharedVoicemailTextToSpeechPrompt \"{_sanitizer.SanitizeString(ttsPrompt)}\" `");
            }
            else if (greetingType == "AudioFile" && !string.IsNullOrWhiteSpace(audioFileId))
            {
                parameters.AppendLine($"-{prefix}SharedVoicemailAudioFilePrompt \"{_sanitizer.SanitizeString(audioFileId)}\" `");
            }
        }
    }
}
