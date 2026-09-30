namespace Domain.Entities
{
    /// <summary>
    /// Marks master data: configuration records that documents refer to. The generic
    /// repository refuses to physically delete master data while any record still
    /// refers to it (see docs/adr/0001-master-data-delete-guard.md).
    /// </summary>
    public interface IMasterData
    {
    }
}
