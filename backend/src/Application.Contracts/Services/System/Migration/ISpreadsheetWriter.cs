namespace Application.Contracts.Migration
{
    public interface ISpreadsheetWriter
    {
        byte[] Write(IReadOnlyList<SheetTemplate> sheets);
    }
}
