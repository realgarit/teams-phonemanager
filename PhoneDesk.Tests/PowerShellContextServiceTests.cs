using System.Diagnostics;
using PhoneDesk.Services;

namespace PhoneDesk.Tests
{
    /// <summary>
    /// Integration coverage for the async execution foundations added in issue #61: cooperative
    /// cancellation (which must leave the persistent runspace reusable) and native PowerShell progress
    /// forwarding. These spin up a real runspace, so they are slower than the pure unit tests but are the
    /// only way to prove the runspace-reuse contract.
    /// </summary>
    public class PowerShellContextServiceTests
    {
        /// <summary>Captures IProgress reports synchronously (the service reports on the pipeline thread).</summary>
        private sealed class CapturingProgress : IProgress<PowerShellProgress>
        {
            private readonly object _gate = new();
            public List<PowerShellProgress> Reports { get; } = new();

            public void Report(PowerShellProgress value)
            {
                lock (_gate) { Reports.Add(value); }
            }
        }

        private static PowerShellContextService CreateService()
            => new(new LoggingService());

        [Fact]
        public async Task Cancellation_stops_long_running_command_and_leaves_runspace_reusable()
        {
            using var service = CreateService();
            using var cts = new CancellationTokenSource();

            // A command that would otherwise block for 30s; cancellation must interrupt it far sooner.
            var running = service.ExecuteCommandWithDetailsAsync(
                "Start-Sleep -Seconds 30; 'should-not-reach'", null, null, cts.Token);

            // Give the pipeline a moment to actually start before requesting cancellation.
            await Task.Delay(500);

            var sw = Stopwatch.StartNew();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => running);
            sw.Stop();

            // Proves cancellation was cooperative (Stop() interrupted the sleep) rather than waiting it out.
            Assert.True(sw.Elapsed < TimeSpan.FromSeconds(15),
                $"Cancellation took {sw.Elapsed.TotalSeconds:F1}s — it should interrupt the pipeline promptly.");

            // The runspace must remain open and reusable: a follow-up command runs normally.
            var followUp = await service.ExecuteCommandWithDetailsAsync("'still-alive'", null, null, default);
            Assert.Contains("still-alive", followUp.Output);
            Assert.False(followUp.HadErrors);
        }

        [Fact]
        public async Task Progress_records_are_forwarded_to_the_supplied_IProgress()
        {
            using var service = CreateService();
            var progress = new CapturingProgress();

            var result = await service.ExecuteCommandWithDetailsAsync(
                "Write-Progress -Activity 'Provisioning' -Status 'Working' -PercentComplete 42; 'done'",
                null, progress, default);

            Assert.Contains("done", result.Output);
            Assert.Contains(progress.Reports, r =>
                r.Activity == "Provisioning" &&
                r.PercentComplete == 42 &&
                !r.IsIndeterminate);
        }

        [Fact]
        public async Task Command_without_percent_is_reported_as_indeterminate()
        {
            using var service = CreateService();
            var progress = new CapturingProgress();

            await service.ExecuteCommandWithDetailsAsync(
                "Write-Progress -Activity 'Scanning' -Status 'Please wait'; 'ok'",
                null, progress, default);

            Assert.Contains(progress.Reports, r => r.Activity == "Scanning" && r.IsIndeterminate);
        }

        [Fact]
        public async Task Concurrent_executions_are_serialized_not_interleaved()
        {
            using var service = CreateService();

            // Two commands that each append a distinct token with a small delay between two writes.
            // If they interleaved on the shared runspace the outputs would be intermixed; the semaphore
            // guarantees the first completes fully before the second starts.
            var first = service.ExecuteCommandWithDetailsAsync(
                "'A-start'; Start-Sleep -Milliseconds 400; 'A-end'", null, null, default);
            var second = service.ExecuteCommandWithDetailsAsync(
                "'B-start'; 'B-end'", null, null, default);

            var results = await Task.WhenAll(first, second);

            // Each result contains only its own tokens — no cross-contamination between pipelines.
            Assert.Contains("A-start", results[0].Output);
            Assert.Contains("A-end", results[0].Output);
            Assert.DoesNotContain("B-start", results[0].Output);

            Assert.Contains("B-start", results[1].Output);
            Assert.DoesNotContain("A-start", results[1].Output);
        }
    }
}
