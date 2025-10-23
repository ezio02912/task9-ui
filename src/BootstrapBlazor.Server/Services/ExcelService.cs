using OfficeOpenXml;
using System.Diagnostics;

namespace BootstrapBlazor.Server.Services
{
    public interface IExcelService
    {
        Task<List<CpdCheckerToolRow>> ImportCpdCheckerDataAsync(Stream fileStream);
    }

    public class ExcelService : IExcelService
    {
        public async Task<List<CpdCheckerToolRow>> ImportCpdCheckerDataAsync(Stream fileStream)
        {
            var excelDataRows = new List<CpdCheckerToolRow>();

            try
            {
                // Set EPPlus License Context
                ExcelPackage.License.SetNonCommercialPersonal("NKai");
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets[0]; // Get first worksheet

                // Check if worksheet has data
                if (worksheet.Dimension == null)
                {
                    return excelDataRows;
                }

                // Get total rows and columns
                int rowCount = worksheet.Dimension.Rows;
                int columnCount = worksheet.Dimension.Columns;

                // Start from row 2 (skip header)
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        // Check if row is empty by checking key cells
                        var noValue = GetCellValue(worksheet, row, 1);
                        var picValue = GetCellValue(worksheet, row, 2);
                        var publinkValue = GetCellValue(worksheet, row, 3);
                        var shortlinkValue = GetCellValue(worksheet, row, 7);

                        // Skip completely empty rows
                        if (string.IsNullOrWhiteSpace(noValue) &&
                            string.IsNullOrWhiteSpace(picValue) &&
                            string.IsNullOrWhiteSpace(publinkValue) &&
                            string.IsNullOrWhiteSpace(shortlinkValue))
                        {
                            continue;
                        }

                        string originalLink = GetCellValue(worksheet, row, 6);
                        if (originalLink.Contains("/?"))
                        {
                            originalLink = originalLink.Replace("/?", "?");
                        }

                        var rowData = new CpdCheckerToolRow
                        {
                            No = GetCellValue(worksheet, row, 1),
                            Pic = GetCellValue(worksheet, row, 2),
                            Publink = GetCellValue(worksheet, row, 3),
                            Brand = GetCellValue(worksheet, row, 4),
                            Position = GetCellValue(worksheet, row, 5),
                            OriginalLink = originalLink,
                            ShortLink = GetCellValue(worksheet, row, 7),
                            FolderBanner = GetCellValue(worksheet, row, 8),
                            FileNameBanner = GetCellValue(worksheet, row, 9),
                            DeviceCheck = GetCellValue(worksheet, row, 10),
                            Dimension = GetCellValue(worksheet, row, 11),
                            Title = GetCellValue(worksheet, row, 12),
                            Alt = GetCellValue(worksheet, row, 13),
                        };

                        // Only add row if it has meaningful data
                        if (!string.IsNullOrWhiteSpace(rowData.Publink) || !string.IsNullOrWhiteSpace(rowData.ShortLink))
                        {
                            excelDataRows.Add(rowData);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing row {row}: {ex.Message}");
                        // Continue processing other rows instead of stopping
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excel import error: {ex.Message}");
                throw new Exception($"Không thể đọc file Excel. Vui lòng kiểm tra file có bị hỏng không hoặc thử file khác. Lỗi: {ex.Message}");
            }

            return excelDataRows;
        }

        private string GetCellValue(ExcelWorksheet worksheet, int row, int column)
        {
            try
            {
                var cell = worksheet.Cells[row, column];
                if (cell == null)
                    return string.Empty;

                var value = cell.Value;
                if (value == null)
                    return string.Empty;

                // Handle different cell value types
                return value.ToString()?.Trim() ?? string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting cell value at row {row}, column {column}: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
