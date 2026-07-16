using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using teams_phonemanager.Planning;

namespace teams_phonemanager.Tests
{
    /// <summary>
    /// Covers <see cref="DryRunPlanExporter"/>: JSON shape/round-trip and CSV row layout + escaping. Plans are
    /// constructed directly from Domain value objects so the exporter is exercised in isolation.
    /// </summary>
    public class DryRunPlanExporterTests
    {
        private static DryRunPlan SamplePlan()
        {
            var objects = new List<PlannedObject>
            {
                new(PlannedObjectType.M365Group, PlannedAction.Create, "ttgrp-contoso-hauptnummer", null,
                    new List<PlannedSetting> { new("Name", "ttgrp-contoso-hauptnummer") }),
                new(PlannedObjectType.ResourceAccount, PlannedAction.Create, "racq-contoso-hauptnummer", "racq-contoso-hauptnummer@contoso.onmicrosoft.com",
                    new List<PlannedSetting> { new("UPN", "racq-contoso-hauptnummer@contoso.onmicrosoft.com") })
            };
            var entry = new DryRunEntry(1, "contoso - hauptnummer", objects,
                Array.Empty<string>(),
                new List<PreflightCheck> { new("Live tenant", PreflightStatus.NotChecked, "Not verified") });
            return new DryRunPlan("Setup Wizard", DateTimeOffset.UtcNow, new List<DryRunEntry> { entry });
        }

        [Fact]
        public void ToJson_ProducesParseableDocumentWithEnumNamesAndEntries()
        {
            var exporter = new DryRunPlanExporter();

            var json = exporter.ToJson(SamplePlan());

            using var doc = JsonDocument.Parse(json); // must be valid JSON
            var root = doc.RootElement;
            Assert.Equal("Setup Wizard", root.GetProperty("Source").GetString());
            Assert.Equal(1, root.GetProperty("EntryCount").GetInt32());
            var firstObject = root.GetProperty("Entries")[0].GetProperty("Objects")[0];
            Assert.Equal("M365Group", firstObject.GetProperty("Type").GetString()); // enum as name, not number
            Assert.Equal("Create", firstObject.GetProperty("Action").GetString());
        }

        [Fact]
        public void ToCsv_EmitsHeaderAndOneRowPerObject()
        {
            var exporter = new DryRunPlanExporter();

            var csv = exporter.ToCsv(SamplePlan());
            var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            Assert.StartsWith("Entry,Label,EntryValid,ObjectType,Action,DisplayName,UPN,Settings,ValidationErrors", lines[0]);
            Assert.Equal(3, lines.Length); // header + 2 objects
            Assert.Contains("M365Group", lines[1]);
            Assert.Contains("ResourceAccount", lines[2]);
        }

        [Fact]
        public void ToCsv_EscapesFieldsContainingCommas()
        {
            var exporter = new DryRunPlanExporter();
            var objects = new List<PlannedObject>
            {
                new(PlannedObjectType.M365Group, PlannedAction.Create, "Doe, Inc group", null,
                    new List<PlannedSetting> { new("Name", "Doe, Inc group") })
            };
            var entry = new DryRunEntry(1, "Doe, Inc - g", objects, new[] { "err, with comma" }, Array.Empty<PreflightCheck>());
            var plan = new DryRunPlan("Bulk", DateTimeOffset.UtcNow, new List<DryRunEntry> { entry });

            var csv = exporter.ToCsv(plan);

            // The label and displayName with commas must be wrapped in quotes so columns don't shift.
            Assert.Contains("\"Doe, Inc - g\"", csv);
            Assert.Contains("\"Doe, Inc group\"", csv);
            Assert.Contains("\"err, with comma\"", csv);
        }
    }
}
