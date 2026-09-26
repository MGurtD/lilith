using Application.Contracts.Migration;
using ClosedXML.Excel;

namespace Infrastructure.DataMigration
{
    /// <summary>XLSX implementation of the spreadsheet read/write ports using ClosedXML.</summary>
    public sealed class XlsxSpreadsheet : ISpreadsheetReader, ISpreadsheetWriter
    {
        public byte[] Write(IReadOnlyList<SheetTemplate> sheets)
        {
            using var workbook = new XLWorkbook();

            foreach (var sheet in sheets)
            {
                var worksheet = workbook.Worksheets.Add(SanitizeSheetName(sheet.Name));

                for (var c = 0; c < sheet.Columns.Count; c++)
                {
                    var column = sheet.Columns[c];
                    var headerCell = worksheet.Cell(1, c + 1);
                    headerCell.Value = column.Header;
                    headerCell.Style.Font.Bold = true;

                    if (!string.IsNullOrEmpty(column.Comment))
                        headerCell.GetComment().AddText(column.Comment);
                }

                for (var r = 0; r < sheet.Rows.Count; r++)
                {
                    var row = sheet.Rows[r];
                    for (var c = 0; c < row.Count; c++)
                        worksheet.Cell(r + 2, c + 1).Value = row[c] ?? string.Empty;
                }

                worksheet.SheetView.FreezeRows(1);
                worksheet.Columns().AdjustToContents();
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public IReadOnlyList<SheetData> Read(Stream stream)
        {
            using var workbook = new XLWorkbook(stream);
            var result = new List<SheetData>();

            foreach (var worksheet in workbook.Worksheets)
            {
                var sheet = new SheetData { Name = worksheet.Name };

                var firstRow = worksheet.FirstRowUsed();
                if (firstRow == null)
                {
                    result.Add(sheet);
                    continue;
                }

                var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
                var headerIndex = new Dictionary<int, string>();

                for (var c = 1; c <= lastColumn; c++)
                {
                    var header = firstRow.Cell(c).GetString().Trim();
                    if (string.IsNullOrEmpty(header))
                        continue;
                    headerIndex[c] = header;
                    sheet.Headers.Add(header);
                }

                var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? firstRow.RowNumber();
                for (var r = firstRow.RowNumber() + 1; r <= lastRow; r++)
                {
                    var xlRow = worksheet.Row(r);
                    if (xlRow.IsEmpty())
                        continue;

                    var row = new SheetRow { Number = r };
                    foreach (var (columnNumber, header) in headerIndex)
                        row.Values[header] = xlRow.Cell(columnNumber).GetString().Trim();

                    sheet.Rows.Add(row);
                }

                result.Add(sheet);
            }

            return result;
        }

        private static string SanitizeSheetName(string name)
        {
            var invalid = new[] { ':', '\\', '/', '?', '*', '[', ']' };
            var sanitized = new string(name.Where(ch => !invalid.Contains(ch)).ToArray());
            return sanitized.Length > 31 ? sanitized[..31] : sanitized;
        }
    }
}
