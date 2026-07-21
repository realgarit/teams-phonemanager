using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using PhoneDesk.Planning;
using PhoneDesk.Services.Interfaces;

namespace PhoneDesk.Planning
{
    /// <summary>
    /// Serializes a <see cref="DryRunPlan"/> to JSON or CSV for change-approval workflows. Pure text
    /// projection — it produces strings only and performs no file or network IO (the Presentation layer
    /// owns the actual save dialog). Enums are written as names for human-readable, diff-friendly output.
    /// </summary>
    public class DryRunPlanExporter : IDryRunPlanExporter
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        public string ToJson(DryRunPlan plan)
        {
            // Project to an explicit shape so the exported document is stable and independent of the
            // Domain record's member layout / computed-property serialization quirks.
            var dto = new
            {
                plan.Source,
                GeneratedUtc = plan.GeneratedUtc,
                plan.EntryCount,
                plan.ValidEntryCount,
                plan.InvalidEntryCount,
                plan.TotalObjectCount,
                plan.IsFullyValid,
                Entries = plan.Entries.Select(e => new
                {
                    e.RowNumber,
                    e.Label,
                    e.IsValid,
                    ValidationErrors = e.ValidationErrors,
                    PreflightChecks = e.PreflightChecks.Select(c => new { c.Name, Status = c.Status, c.Detail }),
                    Objects = e.Objects.Select(o => new
                    {
                        Type = o.Type,
                        Action = o.Action,
                        o.DisplayName,
                        o.Upn,
                        Settings = o.Settings.ToDictionary(s => s.Name, s => s.Value)
                    })
                })
            };

            return JsonSerializer.Serialize(dto, JsonOptions);
        }

        public string ToCsv(DryRunPlan plan)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", new[]
            {
                "Entry", "Label", "EntryValid", "ObjectType", "Action", "DisplayName", "UPN", "Settings", "ValidationErrors"
            }));

            foreach (var entry in plan.Entries)
            {
                var errors = entry.ValidationSummary;

                if (entry.Objects.Count == 0)
                {
                    sb.AppendLine(Row(entry, "", "", "", "", "", errors));
                    continue;
                }

                foreach (var obj in entry.Objects)
                {
                    var settings = string.Join("; ", obj.Settings.Select(s => $"{s.Name}={s.Value}"));
                    sb.AppendLine(Row(entry, obj.Type.ToString(), obj.Action.ToString(), obj.DisplayName, obj.Upn ?? "", settings, errors));
                }
            }

            return sb.ToString();
        }

        private static string Row(DryRunEntry entry, string type, string action, string displayName, string upn, string settings, string errors)
        {
            var fields = new[]
            {
                entry.RowNumber.ToString(),
                entry.Label,
                entry.IsValid ? "true" : "false",
                type,
                action,
                displayName,
                upn,
                settings,
                errors
            };
            return string.Join(",", fields.Select(EscapeCsv));
        }

        private static string EscapeCsv(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            if (field.Contains('"') || field.Contains(',') || field.Contains('\n') || field.Contains('\r'))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            return field;
        }
    }
}
