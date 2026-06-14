using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Moq;
using teams_phonemanager.Models;
using teams_phonemanager.Services;
using teams_phonemanager.Services.Interfaces;
using teams_phonemanager.Services.ScriptBuilders;

namespace teams_phonemanager.Tests
{
    /// <summary>
    /// Pins the generated bulk-operations PowerShell script. Proves that routing
    /// BulkOperationsScriptBuilder through the IPowerShellCommandService facade (instead of the
    /// concrete script builders) leaves the emitted script byte-for-byte identical. The hash was
    /// captured from the pre-refactor implementation.
    /// </summary>
    public class BulkOperationsSnapshotTests
    {
        private const string ExpectedBulkScriptHash =
            "B0E842C78C475259377B9AB950D6F4B1BD7CDACFBEA0E74931CDD425BB67567D";

        private static IPowerShellSanitizationService VerbatimSanitizer()
        {
            var mock = new Mock<IPowerShellSanitizationService>();
            mock.Setup(s => s.SanitizeString(It.IsAny<string>())).Returns<string>(x => x);
            mock.Setup(s => s.SanitizeIdentifier(It.IsAny<string>())).Returns<string>(x => x);
            return mock.Object;
        }

        private static List<PhoneManagerVariables> SampleEntries() => new()
        {
            new PhoneManagerVariables
            {
                Customer = "contoso",
                CustomerGroupName = "hauptnummer",
                MsFallbackDomain = "@contoso.onmicrosoft.com",
                RaaAnrName = "haupt",
                LanguageId = "de-DE",
                TimeZoneId = "W. Europe Standard Time",
                UsageLocation = "CH",
                RaaAnr = "+41441234567",
                PhoneNumberType = "DirectRouting",
            },
        };

        [Fact]
        public void Bulk_script_is_byte_for_byte_unchanged_after_facade_refactor()
        {
            var san = VerbatimSanitizer();
            var facade = new PowerShellCommandService(
                new CommonScriptBuilder(san),
                new CallQueueScriptBuilder(san),
                new AutoAttendantScriptBuilder(san),
                new HolidayScriptBuilder(san),
                new ResourceAccountScriptBuilder(san));

            var bulk = new BulkOperationsScriptBuilder(facade, san);

            var script = bulk.GenerateBulkScript(SampleEntries());

            // Normalize line endings: the builders use StringBuilder.AppendLine (Environment.NewLine),
            // which is CRLF on Windows and LF elsewhere. Hash the LF-normalized content so the snapshot
            // is platform-independent while still catching any real content change. The expected hash
            // was captured from LF output.
            var normalized = script.Replace("\r\n", "\n");
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));

            Assert.Equal(ExpectedBulkScriptHash, hash);
        }
    }
}
