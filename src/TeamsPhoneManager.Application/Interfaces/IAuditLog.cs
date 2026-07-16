using System.Collections.Generic;
using teams_phonemanager.Audit;

namespace teams_phonemanager.Services.Interfaces
{
    /// <summary>
    /// Port for the persistent operation audit trail (Clean Architecture: Application owns the
    /// abstraction, Infrastructure supplies the file-based implementation). Writing an audit record
    /// must never alter the audited operation — implementations append out-of-band and must swallow
    /// their own IO faults rather than propagate them into the operation path.
    /// </summary>
    public interface IAuditLog
    {
        /// <summary>
        /// Appends a record to the tenant's audit file. The implementation redacts secrets before
        /// persistence and rotates the underlying file by date/size. Never throws to the caller.
        /// </summary>
        void Append(AuditRecord record);

        /// <summary>
        /// Reads every persisted record across all tenants, newest first. Used by the history viewer;
        /// malformed lines are skipped rather than failing the whole read.
        /// </summary>
        IReadOnlyList<AuditRecord> Read();

        /// <summary>The root directory under which per-tenant audit files are stored.</summary>
        string LogDirectoryPath { get; }
    }
}
