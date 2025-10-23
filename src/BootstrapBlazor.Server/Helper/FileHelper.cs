using OfficeOpenXml;
using BootstrapBlazor.Server.Exceptions;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using Microsoft.AspNetCore.Components.Forms;

namespace BootstrapBlazor.Server.Helper
{
    public static class FileHelper
    {
        public static async Task<byte[]> GetBytesOfExcelFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new NotFoundException("Not Found File");
            }

            FileInfo existingFile = new FileInfo(path);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage package = new ExcelPackage(existingFile))
            {
                return await package.GetAsByteArrayAsync();
            }

        }

        public static string ShortenFileName(string fileName)
        {
            if (fileName.Length > 30)
                return fileName.Substring(0, 10) + "..." + fileName.Substring(fileName.Length - 10);
            return fileName;
        }
    }
}
