using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using teams_phonemanager.Audit;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Services
{
    /// <summary>
    /// File-based <see cref="IAuditLog"/>: appends JSON-lines records under the platform app-data
    /// directory, one subfolder per tenant, rotated by UTC date and by size. This is an out-of-band
    /// sink — it re-applies <see cref="AuditRedactor"/> before writing and never throws into the
    /// operation path, so audit logging can never change or break an operation.
    ///
    /// Layout: <c>{app-data}/TeamsPhoneManager/audit/{tenant}/audit-{yyyyMMdd}[.{n}].jsonl</c>.
    /// </summary>
    public sealed class FileAuditLog : IAuditLog
    {
        /// <summary>Size ceiling for a single JSONL file before a same-day roll-over segment is started.</summary>
        private const long MaxFileBytes = 5 * 1024 * 1024;

        private const string UnknownTenant = "unknown-tenant";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly string _rootDir;
        private readonly object _sync = new();

        /// <summary>Production constructor: resolves the platform app-data audit root.</summary>
        public FileAuditLog()
            : this(DefaultRootDirectory())
        {
        }

        /// <summary>Test/host constructor: writes under an explicit root directory.</summary>
        public FileAuditLog(string rootDirectory)
        {
            _rootDir = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
        }

        public string LogDirectoryPath => _rootDir;

        private static string DefaultRootDirectory()
        {
            var appData = Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData,
                Environment.SpecialFolderOption.Create);

            if (string.IsNullOrEmpty(appData))
            {
                appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }

            return Path.Combine(appData, "TeamsPhoneManager", "audit");
        }

        public void Append(AuditRecord record)
        {
            if (record is null)
            {
                return;
            }

            try
            {
                var safe = AuditRedactor.Redact(record);
                var tenantDir = Path.Combine(_rootDir, SanitizeTenant(safe.TenantId));
                var line = JsonSerializer.Serialize(safe, JsonOptions);

                lock (_sync)
                {
                    Directory.CreateDirectory(tenantDir);
                    var file = ResolveFile(tenantDir, safe.TimestampUtc);
                    File.AppendAllText(file, line + Environment.NewLine, new UTF8Encoding(false));
                }
            }
            catch (Exception)
            {
                // Out-of-band sink: an audit write must never surface into the operation path.
            }
        }

        public IReadOnlyList<AuditRecord> Read()
        {
            var results = new List<AuditRecord>();

            if (!Directory.Exists(_rootDir))
            {
                return results;
            }

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(_rootDir, "*.jsonl", SearchOption.AllDirectories);
            }
            catch (Exception)
            {
                return results;
            }

            foreach (var file in files)
            {
                foreach (var raw in SafeReadLines(file))
                {
                    if (string.IsNullOrWhiteSpace(raw))
                    {
                        continue;
                    }

                    try
                    {
                        var rec = JsonSerializer.Deserialize<AuditRecord>(raw, JsonOptions);
                        if (rec is not null)
                        {
                            results.Add(rec);
                        }
                    }
                    catch (JsonException)
                    {
                        // Skip a corrupt / partially-written line rather than failing the whole read.
                    }
                }
            }

            results.Sort(static (a, b) => b.TimestampUtc.CompareTo(a.TimestampUtc));
            return results;
        }

        /// <summary>
        /// Resolves the current day's segment, starting a numbered roll-over (<c>.1</c>, <c>.2</c>, …)
        /// once a segment reaches <see cref="MaxFileBytes"/>.
        /// </summary>
        private static string ResolveFile(string dir, DateTimeOffset timestamp)
        {
            var day = timestamp.UtcDateTime.ToString("yyyyMMdd");
            var basePath = Path.Combine(dir, $"audit-{day}.jsonl");

            if (!File.Exists(basePath) || new FileInfo(basePath).Length < MaxFileBytes)
            {
                return basePath;
            }

            var segment = 1;
            string rolled;
            do
            {
                rolled = Path.Combine(dir, $"audit-{day}.{segment}.jsonl");
                segment++;
            }
            while (File.Exists(rolled) && new FileInfo(rolled).Length >= MaxFileBytes);

            return rolled;
        }

        private static IEnumerable<string> SafeReadLines(string file)
        {
            try
            {
                // Share read/write so a concurrent Append does not block or fault the viewer's read.
                using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var lines = new List<string>();
                string? line;
                while ((line = reader.ReadLine()) is not null)
                {
                    lines.Add(line);
                }
                return lines;
            }
            catch (IOException)
            {
                return Array.Empty<string>();
            }
            catch (UnauthorizedAccessException)
            {
                return Array.Empty<string>();
            }
        }

        private static string SanitizeTenant(string? tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return UnknownTenant;
            }

            var invalid = Path.GetInvalidFileNameChars();
            var chars = tenantId.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
            var sanitized = new string(chars).Trim();
            return sanitized.Length == 0 ? UnknownTenant : sanitized;
        }
    }
}
