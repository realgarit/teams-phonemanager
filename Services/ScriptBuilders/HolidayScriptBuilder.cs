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
    }
}
