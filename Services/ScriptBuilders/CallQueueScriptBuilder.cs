using teams_phonemanager.Models;
using teams_phonemanager.Services.Interfaces;
using teams_phonemanager.Services;

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
    }
}
