
namespace BootstrapBlazor.Server.Services
{
    public interface IExcelExportService
    {
        Task<byte[]> ExportCpdCheckerResultsAsync(List<CpdCheckerToolRow> data);
    }

    public class ExcelExportService : IExcelExportService
    {
        public async Task<byte[]> ExportCpdCheckerResultsAsync(List<CpdCheckerToolRow> data)
        {
            try
            {
                // var workbook = new XLWorkbook();
                // var worksheet = workbook.Worksheets.Add("Results");

                // // Add headers
                // var headers = new[]
                // {
                //     "No", "Pic", "Publink", "Brand", "Position", "Original Link", "Short Link",
                //     "File Name Banner", "Dimension", "Title", "Alt",
                //     "Result Shortlink", "Result Shortlink Status", "Result Alt Status",
                //     "Result Title Status", "Banner Check Note"
                // };

                // for (int i = 0; i < headers.Length; i++)
                // {
                //     worksheet.Cell(1, i + 1).Value = headers[i];
                // }

                // // Add data
                // for (int i = 0; i < data.Count; i++)
                // {
                //     var row = data[i];
                //     worksheet.Cell(i + 2, 1).Value = row.No;
                //     worksheet.Cell(i + 2, 2).Value = row.Pic;
                //     worksheet.Cell(i + 2, 3).Value = row.Publink;
                //     worksheet.Cell(i + 2, 4).Value = row.Brand;
                //     worksheet.Cell(i + 2, 5).Value = row.Position;
                //     worksheet.Cell(i + 2, 6).Value = row.OriginalLink;
                //     worksheet.Cell(i + 2, 7).Value = row.ShortLink;
                //     worksheet.Cell(i + 2, 8).Value = row.FileNameBanner;
                //     worksheet.Cell(i + 2, 10).Value = row.Dimension;
                //     worksheet.Cell(i + 2, 11).Value = row.Title;
                //     worksheet.Cell(i + 2, 12).Value = row.Alt;
                //     worksheet.Cell(i + 2, 14).Value = row.ResultShortlink;
                //     worksheet.Cell(i + 2, 15).Value = row.ResultShortlinkStatus;
                //     worksheet.Cell(i + 2, 16).Value = row.ResultAltStatus;
                //     worksheet.Cell(i + 2, 17).Value = row.ResultTitleStatus;
                //     worksheet.Cell(i + 2, 18).Value = row.BannerCheckNote;
                // }

                // // Save to memory stream
                // using var stream = new MemoryStream();
                // workbook.SaveAs(stream);
                // stream.Position = 0;

                // return stream.ToArray();
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tạo file Excel: {ex.Message}", ex);
            }
        }
    }
}
