using teams_phonemanager.Models;
using teams_phonemanager.Services.Interfaces;
using teams_phonemanager.Services;
using System.Text;
using System.Collections.Generic;

namespace teams_phonemanager.Services.ScriptBuilders
{
    public class HolidayScriptBuilder
    {
        private readonly IPowerShellSanitizationService _sanitizer;

        public HolidayScriptBuilder(IPowerShellSanitizationService sanitizer)
        {
            _sanitizer = sanitizer;
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
            // Convert simple DateTime list to HolidayEntry list (backward compatibility)
            var entries = holidayDates.Select(d => new HolidayEntry(d, d.TimeOfDay)).ToList();
            return GetCreateHolidaySeriesFromEntriesCommand(holidayName, entries);
        }

        /// <summary>
        /// Creates a holiday series command supporting optional end dates for multi-day holidays.
        /// When a HolidayEntry has EndDate set, the -End parameter is included in the date range.
        /// </summary>
        public string GetCreateHolidaySeriesFromEntriesCommand(string holidayName, List<HolidayEntry> holidayEntries)
        {
            var sanitizedHolidayName = _sanitizer.SanitizeString(holidayName);

            var sb = new StringBuilder();
            sb.AppendLine(@"
try {");
            sb.AppendLine("    $HolidayDateRange = @()");

            var displayDates = new List<string>();
            for (int i = 0; i < holidayEntries.Count; i++)
            {
                var entry = holidayEntries[i];
                var formattedStart = entry.DateTime.ToString("d/M/yyyy H:mm", System.Globalization.CultureInfo.InvariantCulture);
                var formattedDateShort = entry.Date.ToString("d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                displayDates.Add(formattedDateShort);

                if (entry.HasEndDate && entry.EndDateTime.HasValue)
                {
                    var formattedEnd = entry.EndDateTime.Value.ToString("d/M/yyyy H:mm", System.Globalization.CultureInfo.InvariantCulture);
                    sb.AppendLine($"    $HolidayDateRange += New-CsOnlineDateTimeRange -Start '{formattedStart}' -End '{formattedEnd}'");
                }
                else
                {
                    sb.AppendLine($"    $HolidayDateRange += New-CsOnlineDateTimeRange -Start '{formattedStart}'");
                }
            }

            var datesList = string.Join(", ", displayDates);

            sb.AppendLine($@"    New-CsOnlineSchedule -Name ""{sanitizedHolidayName}"" -FixedSchedule -DateTimeRanges @($HolidayDateRange)");
            sb.AppendLine($@"    Write-Host ""SUCCESS: Holiday series {sanitizedHolidayName} created successfully for dates: {datesList}""");
            sb.AppendLine("}");
            sb.AppendLine("catch {");
            sb.AppendLine($@"    Write-Host ""ERROR: Failed to create holiday series {sanitizedHolidayName}: $_""");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
