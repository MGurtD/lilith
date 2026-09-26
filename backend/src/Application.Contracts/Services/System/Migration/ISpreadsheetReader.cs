namespace Application.Contracts.Migration
{
    public interface ISpreadsheetReader
    {
        IReadOnlyList<SheetData> Read(Stream stream);
    }
}
