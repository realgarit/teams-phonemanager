using System;
using System.Collections.Generic;

namespace PhoneDesk.Services
{
    /// <summary>
    /// Typed result of a PowerShell-backed operation. Replaces string-sniffing of raw output
    /// (<c>Contains("ERROR:")</c> / <c>Contains("SUCCESS:")</c>) in the Presentation layer.
    ///
    /// Carries a success flag, the produced value, a typed <see cref="OperationErrorCategory"/>,
    /// the structured <see cref="PowerShellErrorInfo"/> records, and a correlation id so downstream
    /// features (retry, audit log, dashboard) have real context to work with.
    ///
    /// The success/error marker parsing that used to live in the ViewModels now lives entirely behind
    /// this type (produced by <see cref="PowerShellOperationResultMapper"/>); the markers themselves are
    /// unchanged. <see cref="HasSuccessMarker"/> / <see cref="HasErrorMarker"/> / <see cref="ShouldReportError"/>
    /// expose the same predicates the ViewModels used, without leaking the literals into Presentation.
    /// </summary>
    /// <typeparam name="T">The value type carried on success. For PowerShell commands this is the raw output string.</typeparam>
    public sealed class OperationResult<T>
    {
        private OperationResult(
            bool isSuccess,
            T? value,
            OperationErrorCategory category,
            string? errorMessage,
            IReadOnlyList<PowerShellErrorInfo> errors,
            string correlationId,
            string rawOutput,
            bool hasSuccessMarker,
            bool hasErrorMarker)
        {
            IsSuccess = isSuccess;
            Value = value;
            Category = category;
            ErrorMessage = errorMessage;
            Errors = errors;
            CorrelationId = correlationId;
            RawOutput = rawOutput;
            HasSuccessMarker = hasSuccessMarker;
            HasErrorMarker = hasErrorMarker;
        }

        /// <summary>True when the operation completed without any error signal.</summary>
        public bool IsSuccess { get; }

        /// <summary>The value produced on success (for PowerShell commands, the raw output text). May be present on failure too.</summary>
        public T? Value { get; }

        /// <summary>Typed error classification. <see cref="OperationErrorCategory.None"/> when <see cref="IsSuccess"/> is true.</summary>
        public OperationErrorCategory Category { get; }

        /// <summary>Best-effort human-readable error message (from the first error record or ERROR: line). Null on success.</summary>
        public string? ErrorMessage { get; }

        /// <summary>Structured PowerShell error records (exception type, message, failing command). Empty on success.</summary>
        public IReadOnlyList<PowerShellErrorInfo> Errors { get; }

        /// <summary>Unique id for correlating this operation across logs / audit trails.</summary>
        public string CorrelationId { get; }

        /// <summary>The raw PowerShell output text this result was derived from. Equals <see cref="Value"/> for string results.</summary>
        public string RawOutput { get; }

        /// <summary>True when the raw output contains a <c>SUCCESS</c> marker.</summary>
        public bool HasSuccessMarker { get; }

        /// <summary>True when the raw output contains an <c>ERROR:</c> marker.</summary>
        public bool HasErrorMarker { get; }

        /// <summary>
        /// True when the output should trigger user-facing error handling: an error marker is present and no
        /// success marker is (mirrors the historic <c>Contains("ERROR:") &amp;&amp; !Contains("SUCCESS")</c> check).
        /// </summary>
        public bool ShouldReportError => HasErrorMarker && !HasSuccessMarker;

        /// <summary>
        /// Low-level constructor used by <see cref="PowerShellOperationResultMapper"/>. Kept internal to the
        /// Application layer so all construction routes through the mapper (which owns marker/category parsing).
        /// </summary>
        internal static OperationResult<T> Create(
            bool isSuccess,
            T? value,
            OperationErrorCategory category,
            string? errorMessage,
            IReadOnlyList<PowerShellErrorInfo> errors,
            string correlationId,
            string rawOutput,
            bool hasSuccessMarker,
            bool hasErrorMarker)
            => new(isSuccess, value, category, errorMessage, errors, correlationId, rawOutput, hasSuccessMarker, hasErrorMarker);
    }
}
