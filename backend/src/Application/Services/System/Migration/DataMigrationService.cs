using Application.Contracts;
using Application.Contracts.Migration;

namespace Application.Services.System.Migration
{
    public class DataMigrationService(
        IEnumerable<IMigrationHandler> handlers,
        ISpreadsheetWriter writer,
        ISpreadsheetReader reader) : IDataMigrationService
    {
        public IReadOnlyList<MigrationEntityInfo> GetAvailableEntities() =>
            handlers
                .Select(h => new MigrationEntityInfo { Key = h.Key, DisplayNameKey = h.DisplayNameKey })
                .ToList();

        public byte[] GenerateTemplate(IEnumerable<string> keys)
        {
            var sheets = Resolve(keys).SelectMany(h => h.BuildTemplate()).ToList();
            return writer.Write(sheets);
        }

        public async Task<byte[]> Export(IEnumerable<string> keys)
        {
            var sheets = new List<SheetTemplate>();
            foreach (var handler in Resolve(keys))
                sheets.AddRange(await handler.BuildExport());

            return writer.Write(sheets);
        }

        public async Task<ImportReport> Import(Stream fileStream, IEnumerable<string> keys)
        {
            var sheets = reader.Read(fileStream);
            var report = new ImportReport();

            foreach (var handler in Resolve(keys))
            {
                var handlerReport = await handler.Import(sheets);
                report.Total += handlerReport.Total;
                report.Inserted += handlerReport.Inserted;
                report.Skipped += handlerReport.Skipped;
                report.Errors.AddRange(handlerReport.Errors);
            }

            return report;
        }

        private List<IMigrationHandler> Resolve(IEnumerable<string> keys)
        {
            var selected = keys
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Select(k => k.Trim().ToLowerInvariant())
                .ToHashSet();

            return handlers.Where(h => selected.Contains(h.Key.ToLowerInvariant())).ToList();
        }
    }
}
