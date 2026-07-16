using System;
using System.Collections.Generic;

namespace teams_phonemanager.Audit
{
    /// <summary>
    /// One immutable entry in the persistent operation audit trail. Pure value object — lives in the
    /// Domain layer so the Application port, the Infrastructure JSON-lines writer and the Presentation
    /// history viewer can all reference it without coupling to any framework.
    ///
    /// A record is a factual account of a single attempted operation: who (operator UPN), where
    /// (tenant), what (operation + target + redacted parameters), and how it ended (outcome +
    /// error detail). Secrets must never reach this type in the clear — the writer re-applies
    /// <see cref="AuditRedactor"/> as a defence in depth, but callers should already avoid passing
    /// tokens/secrets in <see cref="Parameters"/> or <see cref="ErrorDetail"/>.
    /// </summary>
    public sealed record AuditRecord
    {
        /// <summary>When the operation completed, in UTC.</summary>
        public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>The signed-in operator (UPN / account) that performed the operation, when known.</summary>
        public string? Operator { get; init; }

        /// <summary>The Microsoft 365 tenant id the operation targeted, when connected.</summary>
        public string? TenantId { get; init; }

        /// <summary>Friendly tenant name, when known.</summary>
        public string? TenantName { get; init; }

        /// <summary>Short operation name (e.g. "Create Call Queue", "RetrieveM365Groups").</summary>
        public string Operation { get; init; } = string.Empty;

        /// <summary>Best-effort identifier of the object(s) the operation acted on.</summary>
        public string? Target { get; init; }

        /// <summary>Non-secret parameters supplied to the operation. Values are redacted before persistence.</summary>
        public IReadOnlyDictionary<string, string>? Parameters { get; init; }

        /// <summary>How the operation ended.</summary>
        public AuditOutcome Outcome { get; init; }

        /// <summary>Human-readable error detail on failure (redacted before persistence). Null on success.</summary>
        public string? ErrorDetail { get; init; }

        /// <summary>Correlation id linking this record to the underlying operation result / application log.</summary>
        public string CorrelationId { get; init; } = string.Empty;

        /// <summary>Application version that produced the record.</summary>
        public string AppVersion { get; init; } = string.Empty;

        /// <summary>Whether this was a mutating operation or a lower-level read/query.</summary>
        public AuditKind Kind { get; init; }
    }
}
