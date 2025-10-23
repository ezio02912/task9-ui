using System.Globalization;
using System.Text.RegularExpressions;

namespace BootstrapBlazor.Server.Helper
{
    public static class StringHelpers
    {
        public static string GetTwoLetterOfName(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }

            var chars = str.Split(" ");
            if (chars.Length > 1)
            {
                return chars[0][0].ToString() + chars[1][0].ToString();
            }

            return chars[0][0].ToString();
        }


        public static string FormatThousand(object value)
        {
            // 1000000 -> 1M
            if (value == null) return "0";
            double number = Convert.ToDouble(value);
            if (number >= 1000000)
            {
                return (number / 1000000).ToString("n1") + "M";
            }
            else if (number >= 1000)
            {
                return (number / 1000).ToString("n0") + "K";
            }

            return ((double)value).ToString("n0", CultureInfo.CreateSpecificCulture("vi-VN"));
        }



        // Get anchor tag content shortlink
        public static string GetAnchorTagContent(string htmlContent, string shortlink)
        {
            try
            {
                // Chuẩn bị các pattern để tìm kiếm
                var searchPatterns = new List<string>
                {
                    shortlink,                       // URL thường
                    Uri.EscapeDataString(shortlink)  // URL đã mã hóa
                };

                // Tìm kiếm thẻ a với từng pattern
                foreach (var pattern in searchPatterns)
                {
                    // Tìm trực tiếp href chứa pattern
                    var hrefVariations = new string[]
                    {
                        $"href=\"{pattern}\"",
                        $"href='{pattern}'",
                        $"href={pattern} ",
                        $"href={pattern}>",
                        $"href=\"/goto?url={pattern}\"",
                        $"href='/goto?url={pattern}'",
                        $"href=/goto?url={pattern} ",
                        $"href=/goto?url={pattern}>"
                    };

                    foreach (var hrefVar in hrefVariations)
                    {
                        var hrefIndex = htmlContent.IndexOf(hrefVar, StringComparison.OrdinalIgnoreCase);

                        if (hrefIndex != -1)
                        {
                            // Tìm vị trí thẻ mở <a gần nhất phía trước href
                            var tagStartIndex = htmlContent.LastIndexOf("<a", hrefIndex);
                            if (tagStartIndex != -1)
                            {
                                // Tìm vị trí đóng thẻ > đầu tiên sau <a
                                var openTagEndIndex = htmlContent.IndexOf(">", tagStartIndex);
                                if (openTagEndIndex != -1)
                                {
                                    // Tìm thẻ đóng </a> đầu tiên sau vị trí mở thẻ
                                    var closeTagIndex = htmlContent.IndexOf("</a>", openTagEndIndex);
                                    if (closeTagIndex != -1)
                                    {
                                        // Lấy toàn bộ nội dung thẻ a bao gồm cả thẻ mở và đóng
                                        var fullTagContent = htmlContent.Substring(tagStartIndex, (closeTagIndex + 4) - tagStartIndex);
                                        return fullTagContent;
                                    }
                                }
                            }
                        }
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static string ExtractImageFromShortLink(string anchorTag, string shortlink)
        {
            try
            {
                if (string.IsNullOrEmpty(anchorTag))
                {
                    return string.Empty;
                }

                // Tìm kiếm thẻ img trong thẻ a với regex linh hoạt hơn
                // Không phân biệt vị trí của thuộc tính src và hỗ trợ nhiều attributes
                var imgSrcRegexPatterns = new string[]
                {
                    @"<img\s+[^>]*?src\s*=\s*[""']([^""']+)[""'][^>]*?>",  // src="url" - chuẩn nhất
                    @"<img\s+[^>]*?src\s*=\s*([^\s""'>]+)[^>]*?>",         // src=url không có dấu ngoặc kép
                    @"<img\s+[^>]*?src\s*=\s*([^\s>]+)[^>]*?>"             // linh hoạt hơn
                };

                foreach (var pattern in imgSrcRegexPatterns)
                {
                    var imgSrcMatch = Regex.Match(anchorTag, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    if (imgSrcMatch.Success)
                    {
                        return imgSrcMatch.Groups[1].Value;
                    }
                }

                // Phương pháp thủ công: tìm thẻ img và src theo cách khác
                int imgTagIndex = anchorTag.IndexOf("<img", StringComparison.OrdinalIgnoreCase);
                if (imgTagIndex != -1)
                {
                    int endImgTagIndex = anchorTag.IndexOf(">", imgTagIndex);
                    if (endImgTagIndex != -1)
                    {
                        string imgTag = anchorTag.Substring(imgTagIndex, endImgTagIndex - imgTagIndex + 1);

                        // Tìm src= trong thẻ img bằng string manipulation
                        int srcIndex = imgTag.IndexOf("src=", StringComparison.OrdinalIgnoreCase);
                        if (srcIndex != -1)
                        {
                            // Xác định ký tự bắt đầu của URL
                            int startUrlIndex = srcIndex + 4; // Sau "src="
                            char firstChar = imgTag[startUrlIndex];

                            if (firstChar == '"' || firstChar == '\'')
                            {
                                // URL trong dấu nháy
                                int endUrlIndex = imgTag.IndexOf(firstChar, startUrlIndex + 1);
                                if (endUrlIndex != -1)
                                {
                                    string srcUrl = imgTag.Substring(startUrlIndex + 1, endUrlIndex - startUrlIndex - 1);
                                    return srcUrl.Replace("\n", "");
                                }
                            }
                            else
                            {
                                // URL không trong dấu nháy, kết thúc khi gặp khoảng trắng hoặc >
                                int endUrlIndex = imgTag.IndexOfAny(new[] { ' ', '\t', '\r', '\n', '>' }, startUrlIndex);
                                if (endUrlIndex != -1)
                                {
                                    string srcUrl = imgTag.Substring(startUrlIndex, endUrlIndex - startUrlIndex);
                                    return srcUrl;
                                }
                            }
                        }
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
        public static string GetDomain(string url)
        {
            var uri = new Uri(url);
            return $"{uri.Scheme}://{uri.Host}";
        }

        public static string GetHost(string url)
        {
            var uri = new Uri(url);
            return $"{uri.Host}";
        }
    }
}
