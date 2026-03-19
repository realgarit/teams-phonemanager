using Xunit;
using teams_phonemanager.Models;

namespace teams_phonemanager.Tests
{
    public class PhoneManagerVariablesTests
    {
        [Fact]
        public void ComputedNames_GenerateCorrectly()
        {
            var vars = new PhoneManagerVariables
            {
                Customer = "acme",
                CustomerGroupName = "sales",
                MsFallbackDomain = "@acme.onmicrosoft.com",
                RaaAnrName = "main"
            };

            Assert.Equal("ttgrp-acme-sales", vars.M365Group);
            Assert.Equal("racq-acme-sales@acme.onmicrosoft.com", vars.RacqUPN);
            Assert.Equal("racq-acme-sales", vars.RacqDisplayName);
            Assert.Equal("cq-acme-sales", vars.CqDisplayName);
            Assert.Equal("raaa-acme-main-sales@acme.onmicrosoft.com", vars.RaaaUPN);
            Assert.Equal("raaa-acme-main-sales", vars.RaaaDisplayName);
            Assert.Equal("aa-acme-main-sales", vars.AaDisplayName);
        }

        [Fact]
        public void HolidayName_GeneratesWithPrefix()
        {
            var vars = new PhoneManagerVariables
            {
                Customer = "acme",
                HolidayNameSuffix = "christmas-2024"
            };

            Assert.Equal("hd-acme-christmas-2024", vars.HolidayName);
        }

        [Fact]
        public void HolidayName_SetStripsDuplicatePrefix()
        {
            var vars = new PhoneManagerVariables { Customer = "acme" };

            vars.HolidayName = "hd-acme-christmas";

            Assert.Equal("christmas", vars.HolidayNameSuffix);
        }

        [Fact]
        public void CustomerChange_UpdatesAllComputedNames()
        {
            var vars = new PhoneManagerVariables
            {
                Customer = "old",
                CustomerGroupName = "grp",
                MsFallbackDomain = "@test.com",
                RaaAnrName = "anr"
            };

            var changedProperties = new List<string>();
            vars.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

            vars.Customer = "new";

            Assert.Contains(nameof(PhoneManagerVariables.M365Group), changedProperties);
            Assert.Contains(nameof(PhoneManagerVariables.RacqUPN), changedProperties);
            Assert.Contains(nameof(PhoneManagerVariables.CqDisplayName), changedProperties);
            Assert.Contains(nameof(PhoneManagerVariables.RaaaUPN), changedProperties);
            Assert.Contains(nameof(PhoneManagerVariables.AaDisplayName), changedProperties);
        }

        [Fact]
        public void DomainChange_UpdatesUPNs()
        {
            var vars = new PhoneManagerVariables
            {
                Customer = "acme",
                CustomerGroupName = "grp",
                MsFallbackDomain = "@old.com",
                RaaAnrName = "anr"
            };

            var changedProperties = new List<string>();
            vars.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

            vars.MsFallbackDomain = "@new.com";

            Assert.Contains(nameof(PhoneManagerVariables.RacqUPN), changedProperties);
            Assert.Contains(nameof(PhoneManagerVariables.RaaaUPN), changedProperties);
            Assert.Equal("racq-acme-grp@new.com", vars.RacqUPN);
        }

        [Fact]
        public void DefaultValues_AreCorrect()
        {
            var vars = new PhoneManagerVariables();

            Assert.Equal(string.Empty, vars.Customer);
            Assert.Equal(string.Empty, vars.CustomerGroupName);
            Assert.Equal(string.Empty, vars.MsFallbackDomain);
            Assert.False(vars.UsePerDaySchedule);
            Assert.Equal(7, vars.WeeklySchedule.Count);
        }

        [Fact]
        public void M365GroupId_PrefillsTargetFields()
        {
            var vars = new PhoneManagerVariables();

            vars.M365GroupId = "group-id-123";

            Assert.Equal("group-id-123", vars.CqOverflowActionTarget);
            Assert.Equal("group-id-123", vars.CqTimeoutActionTarget);
            Assert.Equal("group-id-123", vars.CqNoAgentActionTarget);
        }

        [Fact]
        public void M365GroupId_DoesNotOverwriteExistingTargets()
        {
            var vars = new PhoneManagerVariables
            {
                CqOverflowActionTarget = "existing-id"
            };

            vars.M365GroupId = "new-group-id";

            Assert.Equal("existing-id", vars.CqOverflowActionTarget);
            Assert.Equal("new-group-id", vars.CqTimeoutActionTarget);
        }
    }
}
