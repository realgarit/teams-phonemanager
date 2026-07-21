using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoneDesk.Services;

namespace PhoneDesk.Tests
{
    /// <summary>
    /// Covers the adaptive bulk pacer (issue #62): the base inter-item delay, pace reduction after a
    /// throttle event, the ceiling, and recovery on clean items. The delay is injected so nothing sleeps.
    /// </summary>
    public class BulkPacerTests
    {
        private static (BulkPacer pacer, List<TimeSpan> paced) Create(ThrottleRetryOptions options)
        {
            var paced = new List<TimeSpan>();
            var pacer = new BulkPacer(options, (ts, _) => { paced.Add(ts); return Task.CompletedTask; });
            return (pacer, paced);
        }

        [Fact]
        public async Task Base_inter_item_delay_is_applied()
        {
            var (pacer, paced) = Create(new ThrottleRetryOptions { BulkInterItemDelaySeconds = 1 });

            Assert.Equal(1.0, pacer.CurrentDelay.TotalSeconds);
            await pacer.PaceAsync();
            Assert.Equal(new[] { 1.0 }, paced.ConvertAll(t => t.TotalSeconds));
        }

        [Fact]
        public void Registering_a_throttle_reduces_the_pace()
        {
            var (pacer, _) = Create(new ThrottleRetryOptions
            {
                BulkInterItemDelaySeconds = 1,
                BulkMaxInterItemDelaySeconds = 30,
                BulkPaceMultiplierOnThrottle = 2
            });

            pacer.RegisterThrottle();
            Assert.Equal(2.0, pacer.CurrentDelay.TotalSeconds);

            pacer.RegisterThrottle();
            Assert.Equal(4.0, pacer.CurrentDelay.TotalSeconds);
        }

        [Fact]
        public void Pace_reduction_is_capped_at_the_maximum()
        {
            var (pacer, _) = Create(new ThrottleRetryOptions
            {
                BulkInterItemDelaySeconds = 1,
                BulkMaxInterItemDelaySeconds = 3,
                BulkPaceMultiplierOnThrottle = 2
            });

            for (var i = 0; i < 6; i++)
            {
                pacer.RegisterThrottle();
            }

            Assert.Equal(3.0, pacer.CurrentDelay.TotalSeconds);
        }

        [Fact]
        public void Clean_items_decay_the_pace_back_toward_the_base()
        {
            var (pacer, _) = Create(new ThrottleRetryOptions
            {
                BulkInterItemDelaySeconds = 1,
                BulkMaxInterItemDelaySeconds = 30,
                BulkPaceMultiplierOnThrottle = 2
            });

            pacer.RegisterThrottle();
            pacer.RegisterThrottle(); // -> 4s
            Assert.Equal(4.0, pacer.CurrentDelay.TotalSeconds);

            pacer.RegisterSuccess(); // -> 2s
            Assert.Equal(2.0, pacer.CurrentDelay.TotalSeconds);

            pacer.RegisterSuccess(); // -> 1s (base)
            Assert.Equal(1.0, pacer.CurrentDelay.TotalSeconds);

            pacer.RegisterSuccess(); // floored at base
            Assert.Equal(1.0, pacer.CurrentDelay.TotalSeconds);
        }

        [Fact]
        public async Task Pace_after_throttle_waits_the_increased_delay()
        {
            var (pacer, paced) = Create(new ThrottleRetryOptions
            {
                BulkInterItemDelaySeconds = 1,
                BulkMaxInterItemDelaySeconds = 30,
                BulkPaceMultiplierOnThrottle = 2
            });

            pacer.RegisterThrottle();
            await pacer.PaceAsync();

            Assert.Equal(new[] { 2.0 }, paced.ConvertAll(t => t.TotalSeconds));
        }

        [Fact]
        public async Task Pace_honors_cancellation_when_no_delay_is_configured()
        {
            var (pacer, _) = Create(new ThrottleRetryOptions { BulkInterItemDelaySeconds = 0 });
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pacer.PaceAsync(cts.Token));
        }
    }
}
