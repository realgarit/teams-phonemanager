using teams_phonemanager.Services.Interfaces;
using teams_phonemanager.Services;

namespace teams_phonemanager.Services.ScriptBuilders
{
    /// <summary>
    /// Builds PowerShell scripts for gathering tenant documentation data.
    /// </summary>
    public class DocumentationScriptBuilder
    {
        private readonly CommonScriptBuilder _commonBuilder;

        public DocumentationScriptBuilder(CommonScriptBuilder commonBuilder)
        {
            _commonBuilder = commonBuilder;
        }

        /// <summary>
        /// Gets all auto attendants with detailed configuration including menu options and routing.
        /// </summary>
        public string GetExportAutoAttendantsCommand()
        {
            return @"
try {
    $autoAttendants = Get-CsAutoAttendant
    if ($autoAttendants) {
        Write-Host ""DOCDATA_AA_START""
        foreach ($aa in $autoAttendants) {
            Write-Host ""DOCDATA_AA: $($aa.Name)|$($aa.Identity)|$($aa.LanguageId)|$($aa.TimeZoneId)|$($aa.VoiceId)|$($aa.DefaultCallFlow.Name)""
            
            # Default call flow menu options
            if ($aa.DefaultCallFlow.Menu.MenuOptions) {
                foreach ($opt in $aa.DefaultCallFlow.Menu.MenuOptions) {
                    $target = ''
                    if ($opt.CallTarget) { $target = $opt.CallTarget.Id }
                    Write-Host ""DOCDATA_AA_MENU: $($aa.Name)|DefaultCallFlow|$($opt.DtmfResponse)|$($opt.Action)|$target""
                }
            }
            
            # All call flows (including after-hours)
            foreach ($cf in $aa.CallFlows) {
                Write-Host ""DOCDATA_AA_CF: $($aa.Name)|$($cf.Name)|$($cf.Menu.Name)""
                if ($cf.Menu.MenuOptions) {
                    foreach ($opt in $cf.Menu.MenuOptions) {
                        $target = ''
                        if ($opt.CallTarget) { $target = $opt.CallTarget.Id }
                        Write-Host ""DOCDATA_AA_MENU: $($aa.Name)|$($cf.Name)|$($opt.DtmfResponse)|$($opt.Action)|$target""
                    }
                }
            }
            
            # Call handling associations (business hours, holidays)
            foreach ($cha in $aa.CallHandlingAssociations) {
                Write-Host ""DOCDATA_AA_CHA: $($aa.Name)|$($cha.Type)|$($cha.ScheduleId)|$($cha.CallFlowId)""
            }

            # Operator
            if ($aa.Operator) {
                Write-Host ""DOCDATA_AA_OP: $($aa.Name)|$($aa.Operator.Type)|$($aa.Operator.Id)""
            }
        }
        Write-Host ""DOCDATA_AA_END""
        Write-Host ""SUCCESS: Exported $($autoAttendants.Count) auto attendants""
    } else {
        Write-Host ""DOCDATA_AA_START""
        Write-Host ""DOCDATA_AA_END""
        Write-Host ""INFO: No auto attendants found""
    }
}
catch {
    Write-Host ""ERROR: Failed to export auto attendants: $_""
}";
        }

        /// <summary>
        /// Gets all call queues with detailed configuration including agents.
        /// </summary>
        public string GetExportCallQueuesCommand()
        {
            return @"
try {
    $callQueues = Get-CsCallQueue
    if ($callQueues) {
        Write-Host ""DOCDATA_CQ_START""
        foreach ($cq in $callQueues) {
            $agentCount = if ($cq.Agents) { $cq.Agents.Count } else { 0 }
            Write-Host ""DOCDATA_CQ: $($cq.Name)|$($cq.Identity)|$($cq.RoutingMethod)|$($cq.AgentAlertTime)|$($cq.LanguageId)|$agentCount|$($cq.OverflowThreshold)|$($cq.TimeoutThreshold)|$($cq.OverflowAction)|$($cq.TimeoutAction)""
            
            # Agent details
            if ($cq.Agents) {
                foreach ($agent in $cq.Agents) {
                    Write-Host ""DOCDATA_CQ_AGENT: $($cq.Name)|$($agent.ObjectId)|$($agent.OptIn)""
                }
            }

            # Distribution lists / groups used
            if ($cq.DistributionLists) {
                foreach ($dl in $cq.DistributionLists) {
                    Write-Host ""DOCDATA_CQ_DL: $($cq.Name)|$dl""
                }
            }

            # Overflow/Timeout targets
            if ($cq.OverflowActionTarget) {
                Write-Host ""DOCDATA_CQ_OVERFLOW: $($cq.Name)|$($cq.OverflowAction)|$($cq.OverflowActionTarget.Id)|$($cq.OverflowThreshold)""
            }
            if ($cq.TimeoutActionTarget) {
                Write-Host ""DOCDATA_CQ_TIMEOUT: $($cq.Name)|$($cq.TimeoutAction)|$($cq.TimeoutActionTarget.Id)|$($cq.TimeoutThreshold)""
            }
        }
        Write-Host ""DOCDATA_CQ_END""
        Write-Host ""SUCCESS: Exported $($callQueues.Count) call queues""
    } else {
        Write-Host ""DOCDATA_CQ_START""
        Write-Host ""DOCDATA_CQ_END""
        Write-Host ""INFO: No call queues found""
    }
}
catch {
    Write-Host ""ERROR: Failed to export call queues: $_""
}";
        }

        /// <summary>
        /// Gets all online schedules with date range details.
        /// </summary>
        public string GetExportSchedulesCommand()
        {
            return @"
try {
    $schedules = Get-CsOnlineSchedule
    if ($schedules) {
        Write-Host ""DOCDATA_SCHED_START""
        foreach ($sched in $schedules) {
            $type = if ($sched.FixedSchedule) { 'Fixed' } else { 'Weekly' }
            $dateCount = if ($sched.FixedSchedule -and $sched.FixedSchedule.DateTimeRanges) { $sched.FixedSchedule.DateTimeRanges.Count } else { 0 }
            Write-Host ""DOCDATA_SCHED: $($sched.Name)|$($sched.Id)|$type|$dateCount""
            
            # Fixed schedule date ranges (holidays)
            if ($sched.FixedSchedule -and $sched.FixedSchedule.DateTimeRanges) {
                foreach ($dr in $sched.FixedSchedule.DateTimeRanges) {
                    Write-Host ""DOCDATA_SCHED_DR: $($sched.Name)|$($dr.Start)|$($dr.End)""
                }
            }
            
            # Weekly schedule (business hours)
            if ($sched.WeeklyRecurrentSchedule) {
                $ws = $sched.WeeklyRecurrentSchedule
                $days = @('Monday','Tuesday','Wednesday','Thursday','Friday','Saturday','Sunday')
                foreach ($day in $days) {
                    $prop = $ws.""$($day)Hours""
                    if ($prop) {
                        foreach ($tr in $prop) {
                            Write-Host ""DOCDATA_SCHED_WK: $($sched.Name)|$day|$($tr.Start)|$($tr.End)""
                        }
                    }
                }
            }
        }
        Write-Host ""DOCDATA_SCHED_END""
        Write-Host ""SUCCESS: Exported $($schedules.Count) schedules""
    } else {
        Write-Host ""DOCDATA_SCHED_START""
        Write-Host ""DOCDATA_SCHED_END""
        Write-Host ""INFO: No schedules found""
    }
}
catch {
    Write-Host ""ERROR: Failed to export schedules: $_""
}";
        }

        /// <summary>
        /// Gets all resource accounts with phone numbers and associations.
        /// </summary>
        public string GetExportResourceAccountsCommand()
        {
            return @"
try {
    $resourceAccounts = Get-CsOnlineApplicationInstance
    if ($resourceAccounts) {
        Write-Host ""DOCDATA_RA_START""
        foreach ($ra in $resourceAccounts) {
            Write-Host ""DOCDATA_RA: $($ra.DisplayName)|$($ra.UserPrincipalName)|$($ra.ObjectId)|$($ra.ApplicationId)|$($ra.PhoneNumber)""
        }
        Write-Host ""DOCDATA_RA_END""

        # Get RA-to-AA/CQ associations
        Write-Host ""DOCDATA_ASSOC_START""
        foreach ($ra in $resourceAccounts) {
            try {
                $assoc = Get-CsOnlineApplicationInstanceAssociation -Identity $ra.ObjectId -ErrorAction SilentlyContinue
                if ($assoc) {
                    Write-Host ""DOCDATA_ASSOC: $($ra.DisplayName)|$($ra.ObjectId)|$($assoc.ConfigurationId)|$($assoc.ConfigurationType)""
                }
            } catch { }
        }
        Write-Host ""DOCDATA_ASSOC_END""

        Write-Host ""SUCCESS: Exported $($resourceAccounts.Count) resource accounts""
    } else {
        Write-Host ""DOCDATA_RA_START""
        Write-Host ""DOCDATA_RA_END""
        Write-Host ""DOCDATA_ASSOC_START""
        Write-Host ""DOCDATA_ASSOC_END""
        Write-Host ""INFO: No resource accounts found""
    }
}
catch {
    Write-Host ""ERROR: Failed to export resource accounts: $_""
}";
        }

        /// <summary>
        /// Gets tenant information and phone number inventory.
        /// </summary>
        public string GetExportTenantInfoCommand()
        {
            return @"
try {
    $tenant = Get-CsTenant
    if ($tenant) {
        Write-Host ""DOCDATA_TENANT: $($tenant.DisplayName)|$($tenant.TenantId)|$($tenant.CountryAbbreviation)|$($tenant.PreferredLanguage)""
        Write-Host ""SUCCESS: Tenant info exported""
    } else {
        Write-Host ""ERROR: Could not retrieve tenant information""
    }
}
catch {
    Write-Host ""ERROR: Failed to export tenant info: $_""
}";
        }

        /// <summary>
        /// Gets all phone numbers assigned in the tenant.
        /// </summary>
        public string GetExportPhoneNumbersCommand()
        {
            return @"
try {
    $numbers = Get-CsPhoneNumberAssignment
    if ($numbers) {
        Write-Host ""DOCDATA_PHONE_START""
        foreach ($num in $numbers) {
            Write-Host ""DOCDATA_PHONE: $($num.TelephoneNumber)|$($num.NumberType)|$($num.AssignedPstnTargetId)|$($num.PstnAssignmentStatus)|$($num.ActivationState)|$($num.City)|$($num.Capability -join ',')""
        }
        Write-Host ""DOCDATA_PHONE_END""
        Write-Host ""SUCCESS: Exported $($numbers.Count) phone numbers""
    } else {
        Write-Host ""DOCDATA_PHONE_START""
        Write-Host ""DOCDATA_PHONE_END""
        Write-Host ""INFO: No phone numbers found""
    }
}
catch {
    Write-Host ""WARNING: Could not export phone numbers (may require additional permissions): $_""
    Write-Host ""DOCDATA_PHONE_START""
    Write-Host ""DOCDATA_PHONE_END""
}";
        }

        /// <summary>
        /// Gets Teams-enabled users with voice configuration.
        /// </summary>
        public string GetExportVoiceUsersCommand()
        {
            return @"
try {
    $users = Get-CsOnlineUser -Filter {EnterpriseVoiceEnabled -eq $true} | Select-Object DisplayName, UserPrincipalName, LineUri, OnlineVoiceRoutingPolicy, TeamsCallingPolicy, DialPlan
    if ($users) {
        Write-Host ""DOCDATA_USER_START""
        foreach ($u in $users) {
            $lineUri = if ($u.LineUri) { $u.LineUri } else { '' }
            $vrp = if ($u.OnlineVoiceRoutingPolicy) { $u.OnlineVoiceRoutingPolicy } else { 'Global' }
            $tcp = if ($u.TeamsCallingPolicy) { $u.TeamsCallingPolicy } else { 'Global' }
            $dp = if ($u.DialPlan) { $u.DialPlan } else { 'Global' }
            Write-Host ""DOCDATA_USER: $($u.DisplayName)|$($u.UserPrincipalName)|$lineUri|$vrp|$tcp|$dp""
        }
        Write-Host ""DOCDATA_USER_END""
        Write-Host ""SUCCESS: Exported $($users.Count) voice users""
    } else {
        Write-Host ""DOCDATA_USER_START""
        Write-Host ""DOCDATA_USER_END""
        Write-Host ""INFO: No enterprise voice users found""
    }
}
catch {
    Write-Host ""WARNING: Could not export voice users: $_""
    Write-Host ""DOCDATA_USER_START""
    Write-Host ""DOCDATA_USER_END""
}";
        }
    }
}
