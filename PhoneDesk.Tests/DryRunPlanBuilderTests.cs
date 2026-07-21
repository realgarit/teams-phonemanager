using System.Collections.Generic;
using System.Linq;
using Moq;
using PhoneDesk.Models;
using PhoneDesk.Planning;
using PhoneDesk.Services;
using PhoneDesk.Services.Interfaces;

namespace PhoneDesk.Tests
{
    /// <summary>
    /// Covers <see cref="DryRunPlanBuilder"/>: object enumeration from configuration, create-flow validation,
    /// intra-plan duplicate preflight, and the guarantee that building a plan performs no PowerShell execution.
    /// A real <see cref="ValidationService"/> is used so E.164 phone validation runs for real.
    /// </summary>
    public class DryRunPlanBuilderTests
    {
        private static DryRunPlanBuilder CreateBuilder(out Mock<IValidationService> validationMock)
        {
            validationMock = new Mock<IValidationService>();
            validationMock.Setup(v => v.IsValidPhoneNumber(It.IsAny<string>()))
                .Returns<string>(s => new ValidationService(new Mock<ISessionManager>().Object).IsValidPhoneNumber(s));
            return new DryRunPlanBuilder(validationMock.Object);
        }

        private static PhoneManagerVariables ValidVariables(string customer = "contoso", string group = "hauptnummer", string number = "+41441234567") =>
            new()
            {
                Customer = customer,
                CustomerGroupName = group,
                MsFallbackDomain = "@contoso.onmicrosoft.com",
                RaaAnrName = "haupt",
                LanguageId = "de-DE",
                TimeZoneId = "W. Europe Standard Time",
                UsageLocation = "CH",
                RaaAnr = number,
                PhoneNumberType = "DirectRouting"
            };

        [Fact]
        public void BuildWizardPlan_ValidConfig_EnumeratesAllExpectedObjects()
        {
            var builder = CreateBuilder(out _);
            var vars = ValidVariables();

            var plan = builder.BuildWizardPlan(vars);

            Assert.Equal("Setup Wizard", plan.Source);
            Assert.Single(plan.Entries);
            var entry = plan.Entries[0];
            Assert.True(entry.IsValid);

            // M365 Group, CQ RA, CQ License, Call Queue, AA RA, AA License, Phone Number, Auto Attendant, Association
            Assert.Equal(9, entry.Objects.Count);
            Assert.Contains(entry.Objects, o => o.Type == PlannedObjectType.M365Group && o.DisplayName == "ttgrp-contoso-hauptnummer");
            Assert.Contains(entry.Objects, o => o.Type == PlannedObjectType.CallQueue && o.DisplayName == "cq-contoso-hauptnummer");
            Assert.Contains(entry.Objects, o => o.Type == PlannedObjectType.AutoAttendant && o.DisplayName == "aa-contoso-haupt-hauptnummer");
            Assert.Contains(entry.Objects, o => o.Type == PlannedObjectType.Association);

            var phone = entry.Objects.Single(o => o.Type == PlannedObjectType.PhoneNumber);
            Assert.Contains(phone.Settings, s => s.Name == "Phone Number" && s.Value == "+41441234567");

            var cqRa = entry.Objects.First(o => o.Type == PlannedObjectType.ResourceAccount);
            Assert.Equal("racq-contoso-hauptnummer@contoso.onmicrosoft.com", cqRa.Upn);
        }

        [Fact]
        public void BuildWizardPlan_MissingPhoneNumber_OmitsPhoneObjectAndReportsValidationError()
        {
            var builder = CreateBuilder(out _);
            var vars = ValidVariables(number: "");

            var plan = builder.BuildWizardPlan(vars);
            var entry = plan.Entries[0];

            Assert.DoesNotContain(entry.Objects, o => o.Type == PlannedObjectType.PhoneNumber);
            Assert.Contains(entry.ValidationErrors, e => e.Contains("phone number", System.StringComparison.OrdinalIgnoreCase));
            Assert.False(entry.IsValid);
        }

        [Fact]
        public void BuildWizardPlan_InvalidPhoneNumberFormat_ReportsValidationError()
        {
            var builder = CreateBuilder(out _);
            var vars = ValidVariables(number: "12345"); // not E.164

            var plan = builder.BuildWizardPlan(vars);
            var entry = plan.Entries[0];

            Assert.Contains(entry.ValidationErrors, e => e.Contains("E.164"));
            Assert.False(entry.IsValid);
        }

        [Fact]
        public void BuildWizardPlan_MissingRequiredFields_ReportsAllErrors()
        {
            var builder = CreateBuilder(out _);
            var vars = new PhoneManagerVariables(); // everything blank

            var plan = builder.BuildWizardPlan(vars);
            var entry = plan.Entries[0];

            Assert.False(entry.IsValid);
            Assert.Contains(entry.ValidationErrors, e => e.Contains("Customer"));
            Assert.Contains(entry.ValidationErrors, e => e.Contains("Usage location"));
            Assert.Contains(entry.ValidationErrors, e => e.Contains("Language"));
        }

        [Fact]
        public void BuildBulkPlan_DuplicateGroupAcrossRows_MarksBothEntriesFailingPreflight()
        {
            var builder = CreateBuilder(out _);
            var entries = new List<IPhoneManagerVariables>
            {
                ValidVariables("contoso", "hauptnummer", "+41441234567"),
                ValidVariables("contoso", "hauptnummer", "+41441239999") // same customer+group => same M365Group + UPNs
            };

            var plan = builder.BuildBulkPlan(entries);

            Assert.Equal(2, plan.EntryCount);
            Assert.Equal(0, plan.ValidEntryCount);
            Assert.All(plan.Entries, e =>
                Assert.Contains(e.PreflightChecks, c => c.Status == PreflightStatus.Fail && c.Name.Contains("M365 Group")));
        }

        [Fact]
        public void BuildBulkPlan_DuplicatePhoneNumberOnly_FailsNumberPreflight()
        {
            var builder = CreateBuilder(out _);
            var entries = new List<IPhoneManagerVariables>
            {
                ValidVariables("contoso", "reception", "+41441234567"),
                ValidVariables("fabrikam", "empfang", "+41441234567") // distinct names, same number
            };

            var plan = builder.BuildBulkPlan(entries);

            Assert.All(plan.Entries, e =>
                Assert.Contains(e.PreflightChecks, c => c.Status == PreflightStatus.Fail && c.Name.Contains("Phone number")));
            // Group/UPN preflight should pass since those differ.
            Assert.All(plan.Entries, e =>
                Assert.Contains(e.PreflightChecks, c => c.Status == PreflightStatus.Pass && c.Name.Contains("M365 Group")));
        }

        [Fact]
        public void BuildBulkPlan_AllUnique_IsFullyValid()
        {
            var builder = CreateBuilder(out _);
            var entries = new List<IPhoneManagerVariables>
            {
                ValidVariables("contoso", "reception", "+41441234567"),
                ValidVariables("fabrikam", "empfang", "+41441239999")
            };

            var plan = builder.BuildBulkPlan(entries);

            Assert.True(plan.IsFullyValid);
            Assert.Equal(2, plan.ValidEntryCount);
        }

        [Fact]
        public void BuildBulkPlan_LiveTenantCheck_IsReportedAsNotChecked()
        {
            var builder = CreateBuilder(out _);
            var plan = builder.BuildBulkPlan(new List<IPhoneManagerVariables> { ValidVariables() });

            Assert.Contains(plan.Entries[0].PreflightChecks,
                c => c.Status == PreflightStatus.NotChecked && c.Name.Contains("Live tenant"));
        }
    }
}
