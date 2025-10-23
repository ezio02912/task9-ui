using System.Text.RegularExpressions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.ComponentModel;
using System.Reflection;

namespace BootstrapBlazor.Server.Helper
{
      public class ObjectHelper
    {
        public static List<string> GetPropDescsOrNames<T>() where T : class
        {
            var propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var list = propertyInfos.Select(p =>
                Attribute.IsDefined((MemberInfo) p, typeof(DescriptionAttribute)) ?
                    (Attribute.GetCustomAttribute((MemberInfo) p, typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description :
                    p.Name
            ).ToList();

            return list;
        }
    }

    public class ExportHelper
    {
        public static byte[] GenerateUnitReviewExcelBytes<T>(List<T> rows,  Type type, string sheetName = "Sheet1") where T : class
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            var excel = new ExcelPackage();
            var sheet = PushDataIntoSheet(excel, rows, type, sheetName);
            sheet.Column(3).AutoFit(0, 80);
            // Like Comment Share Total
            sheet.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sheet.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            sheet.Column(7).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            sheet.Column(8).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            sheet.Column(9).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            var data = excel.GetAsByteArray();
            excel.Dispose();

            return data;
        }

        public static byte[] GenerateDocumentExcelBytes<T>(List<T> rows, Type type, string sheetName = "Sheet1") where T : class
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            var excel = new ExcelPackage();
            var sheet = PushDataIntoSheet(excel, rows,type, sheetName);
            sheet.Column(3).AutoFit(0, 80);
            // Like Comment Share Total
            sheet.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            sheet.Column(7).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            sheet.Column(8).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            sheet.Column(9).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var data = excel.GetAsByteArray();
            excel.Dispose();

            return data;
        }
        public static byte[] GenerateProjectExcelBytes<T>(List<T> rows, Type type, string sheetName = "Sheet1") where T : class
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            var excel = new ExcelPackage();
            var sheet = PushDataIntoSheet(excel, rows,  type, sheetName);
            sheet.Column(3).AutoFit(0, 80);
            // Like Comment Share Total
            sheet.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            sheet.Column(7).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            sheet.Column(8).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            sheet.Column(9).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var data = excel.GetAsByteArray();
            excel.Dispose();

            return data;
        }

        private static ExcelWorksheet PushDataIntoSheet<T>(ExcelPackage excelPackage, List<T> rows,  Type type, string sheetName = "Sheet1") where T : class
        {
            var workSheet = excelPackage.Workbook.Worksheets.Add(sheetName);
            workSheet.DefaultRowHeight = 12;
            workSheet.Row(1).Height = 20;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;

            workSheet.Cells.LoadFromCollection(rows, true);

            int i = 1;
            var headers = ObjectHelper.GetPropDescsOrNames<T>();
            foreach (var header in headers)
            {
                workSheet.Cells[1, i].Value = $"{type.Name}:{header}";
                workSheet.Column(i).AutoFit();
                i++;
            }

            return workSheet;
        }


        public static void SetRichTextFromHtml(ExcelRangeBase range) //, string defaultFontName, short defaultFontSize
        {
            // Reset the cell value, just in case there is an existing RichText value.
            string html = range.Text;
             range.Value ="";
            // Sanity check for blank values, skips creating Regex objects for performance.
            if (String.IsNullOrEmpty(html))
            {
                range.IsRichText = false;
                return;
            }

            // Change all BR tags to line breaks. http://epplus.codeplex.com/discussions/238692/
            // Cells with line breaks aren't necessarily considered rich text, so this is performed
            // before parsing the HTML tags.
            html = System.Text.RegularExpressions.Regex.Replace(html, @"<hr>", "\r\n ------------------------------- \r\n", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"<br[^>]*>", "\r\n", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string tag;
            string text;
            ExcelRichText thisrt = null;
            bool isFirst = true;

            // Get all pairs of legitimate tags and the text between them. This loop will
            // only execute if there is at least one start or end tag.
            foreach (Match m in System.Text.RegularExpressions.Regex.Matches(html, @"<(/?[a-z]+)[^<>]*>([\s\S]*?)(?=</?[a-z]+[^<>]*>|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase))
            {
                if (isFirst)
                {
                    // On the very first match, set up the initial rich text object with
                    // the defaults for the text BEFORE the match.
                    range.IsRichText = true;
                    thisrt = range.RichText.Add(CleanText(html.Substring(0, m.Index))); // May be 0-length
                    //thisrt.Size = defaultFontSize;                                      // Set the default font size
                    //thisrt.FontName = defaultFontName;                                  // Set the default font name
                    isFirst = false;
                }
                // Get the tag and the block of text until the NEXT tag or EOS. If there are HTML entities
                // encoded, unencode them, they should be passed to RichText as normal characters (other
                // than non-breaking spaces, which should be replaced with normal spaces, they break Excel.
                tag = m.Groups[1].Captures[0].Value;
                text = CleanText(m.Groups[2].Captures[0].Value);

                if (thisrt.Text == "")
                {
                    // The most recent rich text block wasn't *actually* used last time around, so update
                    // the text and keep it as the "current" block. This happens with the first block if
                    // it starts with a tag, and may happen later if tags come one right after the other.
                    thisrt.Text = text;
                }
                else
                {
                    // The current rich text block has some text, so create a new one. RichText.Add()
                    // automatically applies the settings from the previous block, other than vertical
                    // alignment.
                    thisrt = range.RichText.Add(text);
                }
                // Override the settings based on the current tag, keep all other settings.
                SetStyleFromTag(tag, thisrt);
            }

            if (thisrt == null)
            {
                // No HTML tags were found, so treat this as a normal text value.
                range.IsRichText = false;
                range.Value = CleanText(html);
            }
            else if (String.IsNullOrEmpty(thisrt.Text))
            {
                // Rich text was found, but the last node contains no text, so remove it. This can happen if,
                // say, the end of the string is an end tag or unsupported tag (common).
                range.RichText.Remove(thisrt);

                // Failsafe -- the HTML may be just tags, no text, in which case there may be no rich text
                // directives that remain. If that is the case, turn off rich text and treat this like a blank
                // cell value.
                if (range.RichText.Count == 0)
                {
                    range.IsRichText = false;
                    range.Value = "";
                }

            }

        }

        private static void SetStyleFromTag(string tag, ExcelRichText settings)
        {
            switch (tag.ToLower())
            {
                case "b":
                case "strong":
                    settings.Bold = true;
                    break;

                case "i":
                case "em":
                    settings.Italic = true;
                    break;
                case "u":
                    settings.UnderLine = true;
                    break;

                case "s":
                case "strike":
                    settings.Strike = true;
                    break;

                case "sup":
                    settings.VerticalAlign = ExcelVerticalAlignmentFont.Superscript;
                    break;
                case "sub":
                    settings.VerticalAlign = ExcelVerticalAlignmentFont.Subscript;
                    break;
                case "/b":
                case "/strong":
                    settings.Bold = false;
                    break;
                case "/i":
                case "/em":
                    settings.Italic = false;
                    break;
                case "/u":
                    settings.UnderLine = false;
                    break;
                case "/s":
                case "/strike":
                    settings.Strike = false;
                    break;
                case "/sup":
                case "/sub":
                    settings.VerticalAlign = ExcelVerticalAlignmentFont.None;
                    break;



                default:
                    // unsupported HTML, no style change
                    break;
            }
        }
        private static string CleanText(string s)
        {
            // Need to convert HTML entities (named or numbered) into actual Unicode characters

            s = System.Web.HttpUtility.HtmlDecode(s);
            // Remove any non-breaking spaces, kills Excel
            s = s.Replace("\u00A0", " ");
            return s;
        }

    }

}
