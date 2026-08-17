namespace Application.Contracts.Migration
{
    public interface IDataMigrationService
    {
        IReadOnlyList<MigrationEntityInfo> GetAvailableEntities();
        byte[] GenerateTemplate(IEnumerable<string> keys);
        Task<byte[]> Export(IEnumerable<string> keys);
        Task<ImportReport> Import(Stream fileStream, IEnumerable<string> keys);
    }
}
