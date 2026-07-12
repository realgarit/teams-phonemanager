using teams_phonemanager.Services;

namespace teams_phonemanager.Tests
{
    /// <summary>
    /// Covers <see cref="ThrottleInfo.TryGetRetryAfter"/>: the numeric <c>Retry-After</c> forms Graph/Teams
    /// echo into error text are parsed, and unrelated "retry after ..." prose does not produce a false hit.
    /// </summary>
    public class ThrottleInfoTests
    {
        [Theory]
        [InlineData("ERROR: 429 TooManyRequests. Retry-After: 30", 30)]
        [InlineData("Retry-After 12", 12)]
        [InlineData("Retry-After:5", 5)]
        [InlineData("throttled, retry after 8s", 8)]
        [InlineData("please retry after 30 seconds", 30)]
        public void Parses_numeric_retry_after(string text, int expectedSeconds)
        {
            Assert.True(ThrottleInfo.TryGetRetryAfter(text, out var retryAfter));
            Assert.Equal(expectedSeconds, retryAfter.TotalSeconds);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("ERROR: Something inexplicable happened")]
        [InlineData("will retry after the failure 30 times")]
        public void Returns_false_when_no_numeric_hint_present(string? text)
        {
            Assert.False(ThrottleInfo.TryGetRetryAfter(text, out var retryAfter));
            Assert.Equal(System.TimeSpan.Zero, retryAfter);
        }
    }
}
