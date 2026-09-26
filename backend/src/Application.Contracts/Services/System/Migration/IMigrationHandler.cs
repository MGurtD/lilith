namespace Application.Contracts.Migration
{
    /// <summary>Handles template, export and import for one selectable entity group and its auxiliary sheets.</summary>
    public interface IMigrationHandler
    {
        string Key { get; }
        string DisplayNameKey { get; }

        IReadOnlyList<SheetTemplate> BuildTemplate();
        Task<IReadOnlyList<SheetTemplate>> BuildExport();
        Task<ImportReport> Import(IReadOnlyList<SheetData> sheets);
    }
}
