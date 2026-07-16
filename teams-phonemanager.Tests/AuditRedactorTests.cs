using System.Collections.Generic;
using teams_phonemanager.Audit;

namespace teams_phonemanager.Tests
{
    public class AuditRedactorTests
    {
        // A structurally-valid-looking JWT (header.payload.signature) used as a stand-in access token.
        private const string SampleJwt =
            "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

        [Fact]
        public void RedactText_ReplacesJwtToken()
        {
            var input = $"Login failed with token {SampleJwt} while connecting";

            var result = AuditRedactor.RedactText(input);

            Assert.DoesNotContain(SampleJwt, result);
            Assert.DoesNotContain("eyJ", result);
            Assert.Contains(AuditRedactor.Placeholder, result);
        }

        [Fact]
        public void RedactText_ReplacesBearerHeader()
        {
            var result = AuditRedactor.RedactText("Authorization: Bearer abc123DEF456ghi789");

            Assert.DoesNotContain("abc123DEF456ghi789", result);
            Assert.Contains(AuditRedactor.Placeholder, result);
        }

        [Fact]
        public void RedactText_NullStaysNull()
        {
            Assert.Null(AuditRedactor.RedactText(null));
        }

        [Theory]
        [InlineData("ClientSecret")]
        [InlineData("client_secret")]
        [InlineData("Password")]
        [InlineData("accessToken")]
        [InlineData("ApiKey")]
        [InlineData("Authorization")]
        public void RedactParameters_ReplacesValueForSensitiveKey(string key)
        {
            var parameters = new Dictionary<string, string> { [key] = "super-secret-value" };

            var result = AuditRedactor.RedactParameters(parameters);

            Assert.NotNull(result);
            Assert.Equal(AuditRedactor.Placeholder, result![key]);
        }

        [Fact]
        public void RedactParameters_KeepsNonSensitiveValues_ButScrubsTokensInThem()
        {
            var parameters = new Dictionary<string, string>
            {
                ["TenantId"] = "contoso.onmicrosoft.com",
                ["Note"] = $"embedded {SampleJwt} token"
            };

            var result = AuditRedactor.RedactParameters(parameters);

            Assert.NotNull(result);
            Assert.Equal("contoso.onmicrosoft.com", result!["TenantId"]);
            Assert.DoesNotContain(SampleJwt, result["Note"]);
        }

        [Fact]
        public void Redact_ScrubsAllFreeTextAndParameters()
        {
            var record = new AuditRecord
            {
                Operation = "ConnectGraph",
                Target = "contoso",
                ErrorDetail = $"failed: {SampleJwt}",
                Parameters = new Dictionary<string, string>
                {
                    ["ClientSecret"] = "top-secret",
                    ["AccessToken"] = SampleJwt
                }
            };

            var safe = AuditRedactor.Redact(record);

            Assert.DoesNotContain(SampleJwt, safe.ErrorDetail);
            Assert.Equal(AuditRedactor.Placeholder, safe.Parameters!["ClientSecret"]);
            Assert.Equal(AuditRedactor.Placeholder, safe.Parameters!["AccessToken"]);
            // Non-sensitive fields are preserved.
            Assert.Equal("ConnectGraph", safe.Operation);
            Assert.Equal("contoso", safe.Target);
        }
    }
}
