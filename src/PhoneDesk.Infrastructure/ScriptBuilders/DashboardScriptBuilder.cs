using PhoneDesk.Services;

namespace PhoneDesk.Services.ScriptBuilders
{
    /// <summary>
    /// Builds the single read-only PowerShell query that powers the tenant dashboard (issue #64).
    /// The script only ever *reads* tenant state (Get-Cs* / Get-Mg*) and never mutates anything, so it
    /// is safe to run through the standard execution pipeline as an idempotent (throttle-retryable) read.
    ///
    /// It emits marker lines that <c>TenantTopologyAssembler</c> parses:
    ///   TOPRA:  DisplayName|UserPrincipalName|ObjectId|PhoneNumber|Kind|AccountEnabled
    ///   TOPAA:  Name|Identity|LanguageId|TimeZoneId|ResourceAccountIds|HolidayScheduleIds|CallTargetIds
    ///   TOPCQ:  Name|Identity|RoutingMethod|AgentAlertTime|AgentIds|DistributionListIds|ResourceAccountIds
    ///   TOPGRP: DisplayName|Id|MailNickname|Description
    /// CSV sub-fields are comma-joined id lists. Each section is independently guarded so a failure in
    /// one query still yields partial topology plus a TOPERR diagnostic line.
    ///
    /// This is a NEW read query added to the existing pipeline; it does not modify any existing
    /// ScriptBuilder text or the MSAL/Graph authentication behaviour.
    /// </summary>
    public class DashboardScriptBuilder
    {
        public string GetRetrieveTenantTopologyCommand()
        {
            return @"
$ErrorActionPreference = 'Continue'

# --- Resource accounts (application instances): phone numbers + kind + enabled state ---
try {
    $appInstances = Get-CsOnlineApplicationInstance -ErrorAction Stop
    foreach ($ai in $appInstances) {
        $kind = 'Unknown'
        if ($ai.ApplicationId -eq '" + ConstantsService.TeamsPhone.CallQueueAppId + @"') { $kind = 'CallQueue' }
        elseif ($ai.ApplicationId -eq '" + ConstantsService.TeamsPhone.AutoAttendantAppId + @"') { $kind = 'AutoAttendant' }

        $enabled = 'True'
        try {
            $usr = Get-CsOnlineUser -Identity $ai.ObjectId -ErrorAction Stop
            if ($null -ne $usr.AccountEnabled) { $enabled = [string]$usr.AccountEnabled }
        } catch { }

        Write-Host (""TOPRA: {0}|{1}|{2}|{3}|{4}|{5}"" -f $ai.DisplayName, $ai.UserPrincipalName, $ai.ObjectId, $ai.PhoneNumber, $kind, $enabled)
    }
} catch { Write-Host ""TOPERR: ResourceAccounts: $_"" }

# --- Auto attendants: resource accounts, holiday schedules, call-flow targets ---
try {
    $autoAttendants = Get-CsAutoAttendant -ErrorAction Stop
    foreach ($aa in $autoAttendants) {
        $ras = ($aa.ApplicationInstances -join ',')
        $holidays = (($aa.CallHandlingAssociations | Where-Object { $_.Type -eq 'Holiday' } | ForEach-Object { $_.ScheduleId }) -join ',')
        $targets = @()
        foreach ($cf in $aa.CallFlows) {
            if ($cf.Menu -and $cf.Menu.MenuOptions) {
                foreach ($mo in $cf.Menu.MenuOptions) {
                    if ($mo.CallTarget -and $mo.CallTarget.Id) { $targets += $mo.CallTarget.Id }
                }
            }
        }
        $targetsCsv = ($targets -join ',')
        Write-Host (""TOPAA: {0}|{1}|{2}|{3}|{4}|{5}|{6}"" -f $aa.Name, $aa.Identity, $aa.LanguageId, $aa.TimeZoneId, $ras, $holidays, $targetsCsv)
    }
} catch { Write-Host ""TOPERR: AutoAttendants: $_"" }

# --- Call queues: agents, distribution lists, resource accounts ---
try {
    $callQueues = Get-CsCallQueue -ErrorAction Stop
    foreach ($cq in $callQueues) {
        $agents = (($cq.Agents | ForEach-Object { $_.ObjectId }) -join ',')
        $dls = ($cq.DistributionLists -join ',')
        $ras = ($cq.ApplicationInstances -join ',')
        Write-Host (""TOPCQ: {0}|{1}|{2}|{3}|{4}|{5}|{6}"" -f $cq.Name, $cq.Identity, $cq.RoutingMethod, $cq.AgentAlertTime, $agents, $dls, $ras)
    }
} catch { Write-Host ""TOPERR: CallQueues: $_"" }

# --- M365 groups ---
try {
    $groups = Get-MgGroup -All -ErrorAction Stop
    foreach ($g in $groups) {
        Write-Host (""TOPGRP: {0}|{1}|{2}|{3}"" -f $g.DisplayName, $g.Id, $g.MailNickname, $g.Description)
    }
} catch { Write-Host ""TOPERR: Groups: $_"" }

Write-Host ""SUCCESS: Tenant topology retrieved""
";
        }
    }
}
