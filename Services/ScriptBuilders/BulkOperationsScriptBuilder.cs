using teams_phonemanager.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace teams_phonemanager.Services.ScriptBuilders
{
    /// <summary>
    /// Builds PowerShell scripts for bulk operations based on CSV data.
    /// Each row in the CSV represents a complete phone system setup.
    /// </summary>
    public class BulkOperationsScriptBuilder
    {
        private readonly CommonScriptBuilder _commonBuilder;
        private readonly CallQueueScriptBuilder _callQueueBuilder;
        private readonly AutoAttendantScriptBuilder _autoAttendantBuilder;
        private readonly ResourceAccountScriptBuilder _resourceAccountBuilder;

        public BulkOperationsScriptBuilder(
            CommonScriptBuilder commonBuilder,
            CallQueueScriptBuilder callQueueBuilder,
            AutoAttendantScriptBuilder autoAttendantBuilder,
            ResourceAccountScriptBuilder resourceAccountBuilder)
        {
            _commonBuilder = commonBuilder;
            _callQueueBuilder = callQueueBuilder;
            _autoAttendantBuilder = autoAttendantBuilder;
            _resourceAccountBuilder = resourceAccountBuilder;
        }

        /// <summary>
        /// CSV column headers for the bulk import template.
        /// </summary>
        public static readonly string[] CsvHeaders = new[]
        {
            "Customer",
            "CustomerGroupName",
            "MsFallbackDomain",
            "RaaAnrName",
            "LanguageId",
            "TimeZoneId",
            "UsageLocation",
            "PhoneNumber",
            "PhoneNumberType",
            "OpeningHours1Start",
            "OpeningHours1End",
            "OpeningHours2Start",
            "OpeningHours2End"
        };

        /// <summary>
        /// Generates a CSV template with headers and an example row.
        /// </summary>
        public string GenerateCsvTemplate()
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", CsvHeaders));
            sb.AppendLine("contoso,hauptnummer,@contoso.onmicrosoft.com,haupt,de-DE,W. Europe Standard Time,CH,+41441234567,DirectRouting,08:00,12:00,13:00,17:00");
            return sb.ToString();
        }

        /// <summary>
        /// Parses CSV content into a list of PhoneManagerVariables.
        /// </summary>
        public List<PhoneManagerVariables> ParseCsv(string csvContent)
        {
            var results = new List<PhoneManagerVariables>();
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 2)
                return results; // Need at least header + 1 data row

            var headers = ParseCsvLine(lines[0]);
            var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
                headerMap[headers[i].Trim()] = i;
            }

            for (int lineIdx = 1; lineIdx < lines.Length; lineIdx++)
            {
                var line = lines[lineIdx].Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                var fields = ParseCsvLine(line);
                var vars = new PhoneManagerVariables();

                vars.Customer = GetField(fields, headerMap, "Customer");
                vars.CustomerGroupName = GetField(fields, headerMap, "CustomerGroupName");
                vars.MsFallbackDomain = GetField(fields, headerMap, "MsFallbackDomain");
                vars.RaaAnrName = GetField(fields, headerMap, "RaaAnrName");
                vars.LanguageId = GetField(fields, headerMap, "LanguageId");
                vars.TimeZoneId = GetField(fields, headerMap, "TimeZoneId");
                vars.UsageLocation = GetField(fields, headerMap, "UsageLocation");
                vars.RaaAnr = GetField(fields, headerMap, "PhoneNumber");
                vars.PhoneNumberType = GetField(fields, headerMap, "PhoneNumberType");

                var h1s = GetField(fields, headerMap, "OpeningHours1Start");
                var h1e = GetField(fields, headerMap, "OpeningHours1End");
                var h2s = GetField(fields, headerMap, "OpeningHours2Start");
                var h2e = GetField(fields, headerMap, "OpeningHours2End");

                if (TimeSpan.TryParse(h1s, CultureInfo.InvariantCulture, out var ts1s)) vars.OpeningHours1Start = ts1s;
                if (TimeSpan.TryParse(h1e, CultureInfo.InvariantCulture, out var ts1e)) vars.OpeningHours1End = ts1e;
                if (TimeSpan.TryParse(h2s, CultureInfo.InvariantCulture, out var ts2s)) vars.OpeningHours2Start = ts2s;
                if (TimeSpan.TryParse(h2e, CultureInfo.InvariantCulture, out var ts2e)) vars.OpeningHours2End = ts2e;

                results.Add(vars);
            }

            return results;
        }

        /// <summary>
        /// Generates a combined PowerShell script for all rows.
        /// Each row goes through the full setup: M365 Group → CQ RA → License → CQ → AA RA → License+Phone → AA → Associate.
        /// </summary>
        public string GenerateBulkScript(List<PhoneManagerVariables> entries)
        {
            var sb = new StringBuilder();
            sb.AppendLine(_commonBuilder.GetCommonSetupScript());
            sb.AppendLine();
            sb.AppendLine("# ══════════════════════════════════════════════════════════════");
            sb.AppendLine($"# Bulk Operations — {entries.Count} entries");
            sb.AppendLine("# ══════════════════════════════════════════════════════════════");
            sb.AppendLine();

            for (int i = 0; i < entries.Count; i++)
            {
                var vars = entries[i];
                var num = i + 1;

                sb.AppendLine($"# ──────────────────────────────────────────────────────────────");
                sb.AppendLine($"# Entry {num}/{entries.Count}: {vars.Customer} - {vars.CustomerGroupName}");
                sb.AppendLine($"# ──────────────────────────────────────────────────────────────");
                sb.AppendLine();

                sb.AppendLine($"Write-Host '▶ [{num}/{entries.Count}] Processing: {vars.Customer} - {vars.CustomerGroupName}'");
                sb.AppendLine();

                // Step 1: M365 Group
                sb.AppendLine($"# Step 1: Create M365 Group");
                sb.AppendLine($"Write-Host '  [1/6] Creating M365 Group: {vars.M365Group}'");
                sb.AppendLine(_commonBuilder.GetCreateM365GroupCommand(vars.M365Group));
                sb.AppendLine();

                // Step 2: CQ Resource Account + License
                sb.AppendLine($"# Step 2: Create CQ Resource Account + License");
                sb.AppendLine($"Write-Host '  [2/6] Creating CQ Resource Account: {vars.RacqUPN}'");
                sb.AppendLine(_resourceAccountBuilder.GetCreateResourceAccountCommand(vars));
                sb.AppendLine(_resourceAccountBuilder.GetUpdateResourceAccountUsageLocationCommand(vars.RacqUPN, vars.UsageLocation));
                sb.AppendLine(_commonBuilder.GetAssignLicenseCommand(vars.RacqUPN, vars.SkuId));
                sb.AppendLine();

                // Step 3: Call Queue
                sb.AppendLine($"# Step 3: Create Call Queue");
                sb.AppendLine($"Write-Host '  [3/6] Creating Call Queue: {vars.CqDisplayName}'");
                sb.AppendLine(_callQueueBuilder.GetCreateCallQueueCommand(vars));
                sb.AppendLine();

                // Step 4: AA Resource Account + License + Phone
                sb.AppendLine($"# Step 4: Create AA Resource Account + License + Phone");
                sb.AppendLine($"Write-Host '  [4/6] Creating AA Resource Account: {vars.RaaaUPN}'");
                sb.AppendLine(_resourceAccountBuilder.GetCreateAutoAttendantResourceAccountCommand(vars));
                sb.AppendLine(_resourceAccountBuilder.GetUpdateAutoAttendantResourceAccountUsageLocationCommand(vars.RaaaUPN, vars.UsageLocation));
                sb.AppendLine(_resourceAccountBuilder.GetAssignAutoAttendantLicenseCommand(vars.RaaaUPN, vars.SkuId));
                if (!string.IsNullOrEmpty(vars.RaaAnr))
                {
                    sb.AppendLine(_autoAttendantBuilder.GetAssignPhoneNumberToAutoAttendantCommand(vars.RaaaUPN, vars.RaaAnr, vars.PhoneNumberType));
                }
                sb.AppendLine();

                // Step 5: Auto Attendant (monolithic — runs as one connected script)
                sb.AppendLine($"# Step 5: Create Auto Attendant");
                sb.AppendLine($"Write-Host '  [5/6] Creating Auto Attendant: {vars.AaDisplayName}'");
                sb.AppendLine(_autoAttendantBuilder.GetCreateAutoAttendantCommand(vars));
                sb.AppendLine();

                // Step 6: Associate AA RA
                sb.AppendLine($"# Step 6: Associate AA Resource Account");
                sb.AppendLine($"Write-Host '  [6/6] Associating RA with AA'");
                sb.AppendLine(_autoAttendantBuilder.GetAssociateResourceAccountWithAutoAttendantCommand(vars.RaaaUPN, vars.AaDisplayName));
                sb.AppendLine();

                sb.AppendLine($"Write-Host '✅ [{num}/{entries.Count}] Complete: {vars.Customer} - {vars.CustomerGroupName}'");
                sb.AppendLine();
            }

            sb.AppendLine("Write-Host ''");
            sb.AppendLine($"Write-Host '══════════════════════════════════════════════════════════════'");
            sb.AppendLine($"Write-Host 'Bulk operation complete. {entries.Count} entries processed.'");
            sb.AppendLine($"Write-Host '══════════════════════════════════════════════════════════════'");

            return sb.ToString();
        }

        private static string GetField(string[] fields, Dictionary<string, int> headerMap, string header)
        {
            if (headerMap.TryGetValue(header, out var idx) && idx < fields.Length)
                return fields[idx].Trim().Trim('"');
            return string.Empty;
        }

        private static string[] ParseCsvLine(string line)
        {
            var fields = new List<string>();
            bool inQuotes = false;
            var current = new StringBuilder();

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            fields.Add(current.ToString());
            return fields.ToArray();
        }
    }
}
