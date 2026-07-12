using System;
using System.Threading;
using System.Threading.Tasks;

namespace teams_phonemanager.Services.Interfaces
{
    /// <summary>
    /// Client-side pacing for bulk loops: inserts a delay between items to stay under Microsoft
    /// Graph/Teams throttling limits, and adaptively slows down (reduces pace) after a throttle event,
    /// recovering toward the base pace on clean items. Stateful — resolve a fresh instance per bulk run.
    /// </summary>
    public interface IBulkPacer
    {
        /// <summary>The delay currently inserted before each item.</summary>
        TimeSpan CurrentDelay { get; }

        /// <summary>Waits the <see cref="CurrentDelay"/> before the next item. Honors cancellation.</summary>
        Task PaceAsync(CancellationToken cancellationToken = default);

        /// <summary>Records that a throttle was observed; increases the inter-item delay up to the configured ceiling.</summary>
        void RegisterThrottle();

        /// <summary>Records a clean item; gently decays the inter-item delay back toward the base pace.</summary>
        void RegisterSuccess();
    }
}
