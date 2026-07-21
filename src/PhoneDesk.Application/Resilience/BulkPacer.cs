using System;
using System.Threading;
using System.Threading.Tasks;
using PhoneDesk.Services.Interfaces;

namespace PhoneDesk.Services
{
    /// <summary>
    /// Adaptive client-side pacer for bulk loops (see <see cref="IBulkPacer"/>). Starts at the configured
    /// base inter-item delay, multiplies it after each throttle event up to a ceiling (reduce pace), and
    /// decays it back toward the base on clean items. Stateful — use one instance per bulk run.
    ///
    /// The delay source is injectable so pacing is instant and observable under unit test.
    ///
    /// NOTE (#62): this is a DI-registered foundation only. It is not yet wired into a live loop, because
    /// bulk operations currently run as a single monolithic PowerShell batch with no per-item C# seam.
    /// Live pacing waits for the per-item bulk loop introduced by a later roadmap issue; wiring it in
    /// without that loop would require changing the (frozen) generated script.
    /// </summary>
    public sealed class BulkPacer : IBulkPacer
    {
        private readonly ThrottleRetryOptions _options;
        private readonly Func<TimeSpan, CancellationToken, Task> _delay;
        private double _currentSeconds;

        public BulkPacer(
            ThrottleRetryOptions? options = null,
            Func<TimeSpan, CancellationToken, Task>? delay = null)
        {
            _options = options ?? ThrottleRetryOptions.Default;
            _delay = delay ?? Task.Delay;
            _currentSeconds = Math.Max(0, _options.BulkInterItemDelaySeconds);
        }

        public TimeSpan CurrentDelay => TimeSpan.FromSeconds(_currentSeconds);

        public Task PaceAsync(CancellationToken cancellationToken = default)
        {
            if (_currentSeconds <= 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }

            return _delay(TimeSpan.FromSeconds(_currentSeconds), cancellationToken);
        }

        public void RegisterThrottle()
        {
            var multiplier = _options.BulkPaceMultiplierOnThrottle <= 1 ? 2.0 : _options.BulkPaceMultiplierOnThrottle;

            // Grow from the current delay, but ensure forward progress even when the base delay is 0.
            var increased = _currentSeconds <= 0
                ? Math.Max(_options.BulkInterItemDelaySeconds, 1.0)
                : _currentSeconds * multiplier;

            _currentSeconds = Math.Min(increased, _options.BulkMaxInterItemDelaySeconds);
        }

        public void RegisterSuccess()
        {
            var multiplier = _options.BulkPaceMultiplierOnThrottle <= 1 ? 2.0 : _options.BulkPaceMultiplierOnThrottle;
            var decayed = _currentSeconds / multiplier;
            _currentSeconds = Math.Max(decayed, Math.Max(0, _options.BulkInterItemDelaySeconds));
        }
    }
}
