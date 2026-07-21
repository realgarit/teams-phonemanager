using PhoneDesk.Models;
using Xunit;

namespace PhoneDesk.Tests
{
    public class AppVersionTests
    {
        [Theory]
        [InlineData("3.17.0", 3, 17, 0)]
        [InlineData("v3.17.0", 3, 17, 0)]
        [InlineData("Version 3.16.0", 3, 16, 0)]
        [InlineData("  v10.2.33 ", 10, 2, 33)]
        [InlineData("4.1", 4, 1, 0)]
        public void TryParse_AcceptsKnownFormats(string text, int major, int minor, int patch)
        {
            Assert.True(AppVersion.TryParse(text, out var v));
            Assert.Equal(new AppVersion(major, minor, patch), v);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("not-a-version")]
        [InlineData("v")]
        [InlineData("3")]
        [InlineData("3.x.0")]
        [InlineData("1.2.3.4")]
        public void TryParse_RejectsMalformedInput(string? text)
        {
            Assert.False(AppVersion.TryParse(text, out _));
        }

        [Theory]
        [InlineData("3.17.0", "3.16.0", true)]
        [InlineData("4.0.0", "3.99.99", true)]
        [InlineData("3.16.1", "3.16.0", true)]
        [InlineData("3.16.0", "3.16.0", false)]
        [InlineData("3.15.9", "3.16.0", false)]
        public void IsNewerThan_ComparesSemantically(string left, string right, bool expected)
        {
            Assert.True(AppVersion.TryParse(left, out var l));
            Assert.True(AppVersion.TryParse(right, out var r));
            Assert.Equal(expected, l.IsNewerThan(r));
        }
    }
}
