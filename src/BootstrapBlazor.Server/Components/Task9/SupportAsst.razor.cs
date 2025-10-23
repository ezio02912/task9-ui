using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using HtmlAgilityPack;
using System.Linq;
using Microsoft.JSInterop;

namespace BootstrapBlazor.Server.Components.Task9;
public partial class SupportAsst : IDisposable
{
    #region Inject
    [Inject, NotNull]
    private IWebHostEnvironment? WebHost { get; set; }
    [Inject]
    private IDomainSearchService DomainSearchService { get; set; }
    [Inject, NotNull] private ClipboardService? ClipboardService { get; set; }
    [Inject, NotNull]
    private MaskService? MaskService { get; set; }
    [Inject]
    [NotNull]
    private ToastService? Toast { get; set; }
    #endregion
    [Inject, NotNull]
    private IJSRuntime? JSRuntime { get; set; }
    private CancellationTokenSource? TokenSource { get; set; }
    private bool IsLoading { get; set; } = false;
    private List<TicketInfo> Datas { get; set; } = new();
    private int TotalItems {set;get;} = 0;
    private int TotalFilterItems {set;get;} = 0;
    [NotNull]
    private UniverSheet? _sheetPlugin = null;
    [NotNull]
    private Mask? CustomMask1 { get; set; }

    private string DomainListTextArea { get; set; } = string.Empty;

    private string? _jsonData = null;
    
    private readonly Dictionary<string, string> _plugins = new()
    {
        { "ReportPlugin", "univer-sheet/plugin.js" }
    };

   private readonly Dictionary<string, List<string>> LABELS = new()
    {
        ["ngay_kt"] = new() { "ngay kt", "ngay kiem tra", "ngay k.t", "ngay lap", "ngay phieu", "ngay tao" },
        // Prefer using payment due date (Hạn TT) for Ngày KT according to requirement
        ["han_tt"] = new() { "han tt", "han thanh toan", "han" },
        ["id_phieu"] = new() { "id phieu", "ma phieu", "so phieu", "ma chung tu", "so chung tu" },
        ["nguoi_tao"] = new() { "nguoi tao", "nguoi lap", "nhan vien tao", "created by" },
        ["nha_cung_cap"] = new() { "nha cung cap", "ncc", "supplier" },
        ["so_tai_khoan"] = new() { "sdt", "so tai khoan", "so tk", "so tai khoan", "so tai khoan", "so tai khoan" },
        ["noi_dung_phieu"] = new() { "noi dung phieu", "noi dung", "dien giai", "mo ta" },
        ["brand"] = new() { "brand", "thuong hieu", "hang" },
        ["so_tien"] = new() { "so tien", "thanh tien", "tong tien", "tong cong", "amount", "gia tri" },
    };

    
    protected override async Task OnInitializedAsync()
    {  
        base.OnInitialized();
    }

    private async Task OnCheck()
    {
        if(string.IsNullOrWhiteSpace(DomainListTextArea))
        {
            await Toast.Error("Error", "Vui lòng nhập nội dung");
            return;
        }
        var info  = ExtractFromHtml(DomainListTextArea);
        Datas.Add(info);
        DomainListTextArea = string.Empty;
    }
    private void Reset()
    {
        Datas.Clear();
        DomainListTextArea = string.Empty;
    }
    private async Task CopyData()
    {
        var csv = BuildTsv(Datas);
        await ClipboardService.Copy(csv);
        await Toast.Success("Success", $"Đã copy {Datas.Count} dòng CSV vào clipboard");
    }
    
    private async Task DownloadCsv()
    {
        var csv = BuildCsv(Datas);
        var bytes = Encoding.UTF8.GetBytes(csv);
        var b64 = Convert.ToBase64String(bytes);
        await JSRuntime.InvokeVoidAsync("__downloadCsv", $"tickets_{DateTime.Now:yyyyMMdd_HHmmss}.csv", b64);
    }
    
    private static string BuildCsv(IEnumerable<TicketInfo> items)
    {
        // Build CSV with fixed headers; missing values become empty strings
        // Expanded columns to match the sheet: STT, Ngày KT, Tháng, ID Phiếu, Người tạo, Nhà cung cấp,
        // Số tài khoản, Tên tài khoản, Ngân hàng, Nội dung phiếu, Loại chi phí, Số lượng, Domain, Brand, Số tiền, Link report, PIC
        var headers = new[]
        {
            "STT",
            "Ngày KT",
            "Tháng",
            "ID Phiếu",
            "Người tạo",
            "Nhà cung cấp",
            "Số tài khoản",
            "Tên tài khoản",
            "Ngân hàng",
            "Nội dung phiếu",
            "Loại chi phí",
            "Số lượng",
            "Domain",
            "Brand",
            "Số tiền",
            "Link report",
            "PIC"
        };
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(',', headers.Select(CsvEscape)));
        int index = 1;
        foreach (var it in items)
        {
            // Derive month from Ngày KT if possible
            string monthStr = string.Empty;
            if (DateTime.TryParse(it?.NgayKT, out var dt))
            {
                monthStr = dt.Month.ToString(CultureInfo.InvariantCulture);
            }

            // Try extract link report from nội dung
            var linkReport = ExtractFirstUrl(it?.NoiDungPhieu ?? string.Empty) ?? string.Empty;

            var values = new[]
            {
                 string.Empty,
                it?.NgayKT ?? string.Empty,
                monthStr,
                it?.IDPhieu ?? string.Empty,
                it?.NguoiTao ?? string.Empty,
                it?.NhaCungCap ?? string.Empty,
                it?.SoTaiKhoan ?? string.Empty,
                string.Empty,
                string.Empty,
                it?.NoiDungPhieu ?? string.Empty,
                /* Loại chi phí */ string.Empty,
                /* Số lượng */ string.Empty,
                /* Domain */ string.Empty,
                it?.Brand ?? string.Empty,
                it?.SoTien ?? string.Empty,
                linkReport,
                /* PIC */ string.Empty
            };
            sb.AppendLine(string.Join(',', values.Select(CsvEscape)));
            index++;
        }
        return sb.ToString();
    }

    private static string BuildTsv(IEnumerable<TicketInfo> items)
    {
        // Same columns as CSV but tab-separated
        // var headers = new[]
        // {
        //     "STT",
        //     "Ngày KT",
        //     "Tháng",
        //     "ID Phiếu",
        //     "Người tạo",
        //     "Nhà cung cấp",
        //     "Số tài khoản",
        //     "Tên tài khoản",
        //     "Ngân hàng",
        //     "Nội dung phiếu",
        //     "Loại chi phí",
        //     "Số lượng",
        //     "Domain",
        //     "Brand",
        //     "Số tiền",
        //     "Link report",
        //     "PIC"
        // };
        var sb = new StringBuilder();
        // sb.AppendLine(string.Join('\t', headers.Select(TsvEscape)));
        int index = 1;
        foreach (var it in items)
        {
            string monthStr = string.Empty;
            if (DateTime.TryParse(it?.NgayKT, out var dt)) monthStr = dt.Month.ToString(CultureInfo.InvariantCulture);
            var linkReport = ExtractFirstUrl(it?.NoiDungPhieu ?? string.Empty) ?? string.Empty;
            var values = new[]
            {
                index.ToString(CultureInfo.InvariantCulture),
                it?.NgayKT ?? string.Empty,
                monthStr,
                it?.IDPhieu ?? string.Empty,
                it?.NguoiTao ?? string.Empty,
                it?.NhaCungCap ?? string.Empty,
                it?.SoTaiKhoan ?? string.Empty,
                string.Empty,
                string.Empty,
                it?.NoiDungPhieu ?? string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                it?.Brand ?? string.Empty,
                it?.SoTien ?? string.Empty,
                linkReport,
                string.Empty
            };
            sb.AppendLine(string.Join('\t', values.Select(TsvEscape)));
            index++;
        }
        return sb.ToString();
    }

    private static string TsvEscape(string? s)
    {
        s ??= string.Empty;
        s = s.Replace('\t', ' ').Replace("\r", " ").Replace("\n", " ");
        return s;
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing && TokenSource != null)
        {
            TokenSource.Cancel();
            TokenSource.Dispose();
        }
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    

    private  TicketInfo ExtractFromHtml(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var result = new TicketInfo();
        var found = new Dictionary<string, string>(); // internal key -> value

        // Heuristic 1: Table cells (label in one TD/TH, value in the next TD/TH)
        var cells = doc.DocumentNode.SelectNodes("//td|//th") ?? new HtmlNodeCollection(null);
        foreach (var cell in cells)
        {
            var labelRaw = CleanSpaces(cell.InnerText);
            var labelNorm = Norm(labelRaw);

            foreach (var kv in LABELS)
            {
                if (found.ContainsKey(kv.Key)) continue;
                if (IsLabel(labelNorm, kv.Value))
                {
                    var valueCell = NextSiblingCell(cell);
                    if (valueCell != null)
                    {
                        string value;
                        if (kv.Key == "noi_dung_phieu" || kv.Key == "brand")
                        {
                            value = ExtractMultilineInnerText(valueCell);
                        }
                        else
                        {
                            value = CleanSpaces(valueCell.InnerText);
                        }
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            found[kv.Key] = value;
                            break;
                        }
                    }
                }
            }
        }

        // Heuristic 2: Same element "Label: Value"
        var allElems = doc.DocumentNode.Descendants().Where(n => n.NodeType == HtmlNodeType.Element);
        foreach (var el in allElems)
        {
            var text = CleanSpaces(el.InnerText);
            if (string.IsNullOrWhiteSpace(text) || !text.Contains(':')) continue;

            var textNorm = Norm(text);
            foreach (var kv in LABELS)
            {
                if (found.ContainsKey(kv.Key)) continue;

                foreach (var v in kv.Value)
                {
                    var vNorm = Norm(v);
                    if (textNorm.StartsWith(vNorm + ":") || textNorm.StartsWith(vNorm + " :"))
                    {
                        // Extract substring after the first ':' in the ORIGINAL text (to preserve casing)
                        int idx = text.IndexOf(':');
                        if (idx >= 0 && idx + 1 < text.Length)
                        {
                            var value = text[(idx + 1)..].Trim();
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                found[kv.Key] = value;
                                break;
                            }
                        }
                    }
                }
            }
        }

        // Heuristic 3: Label element, value in next sibling element
        foreach (var el in allElems)
        {
            var labelRaw = CleanSpaces(el.InnerText);
            if (string.IsNullOrWhiteSpace(labelRaw)) continue;
            var labelNorm = Norm(labelRaw);

            foreach (var kv in LABELS)
            {
                if (found.ContainsKey(kv.Key)) continue;
                if (IsLabel(labelNorm, kv.Value))
                {
                    var sib = NextElementSibling(el);
                    if (sib != null)
                    {
                        var cand = CleanSpaces(sib.InnerText);
                        if (!string.IsNullOrWhiteSpace(cand) && !IsLabel(Norm(cand), kv.Value))
                        {
                            found[kv.Key] = cand;
                            break;
                        }
                    }
                }
            }
        }

        // Heuristic 4: Next text node in document order
        foreach (var textNode in doc.DocumentNode.DescendantsAndSelf().SelectMany(n => n.ChildNodes).Where(n => n.NodeType == HtmlNodeType.Text))
        {
            var raw = CleanSpaces(textNode.InnerText);
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var nrm = Norm(raw);

            foreach (var kv in LABELS)
            {
                if (found.ContainsKey(kv.Key)) continue;
                if (IsLabel(nrm, kv.Value))
                {
                    var nextText = NextTextNode(textNode);
                    if (nextText != null)
                    {
                        var cand = CleanSpaces(nextText.InnerText);
                        if (!string.IsNullOrWhiteSpace(cand) && !IsLabel(Norm(cand), kv.Value))
                        {
                            found[kv.Key] = cand;
                            break;
                        }
                    }
                }
            }
        }

        // Map to result object
        // 1) Ngày KT = Hạn TT (fallback to Ngày KT if due date missing)
        result.NgayKT = found.GetValueOrDefault("han_tt", "");
        if (string.IsNullOrWhiteSpace(result.NgayKT))
        {
            result.NgayKT = found.GetValueOrDefault("ngay_kt", "");
        }

        // Fill prelim values from generic extraction
        result.IDPhieu      = found.GetValueOrDefault("id_phieu", "");
        result.NguoiTao     = found.GetValueOrDefault("nguoi_tao", "");
        result.NhaCungCap   = found.GetValueOrDefault("nha_cung_cap", "");
        result.SoTaiKhoan   = found.GetValueOrDefault("so_tai_khoan", "");
        result.NoiDungPhieu = found.GetValueOrDefault("noi_dung_phieu", "");
        result.Brand        = found.GetValueOrDefault("brand", "");
        result.SoTien       = found.GetValueOrDefault("so_tien", "");

        // 2) Normalize "Người tạo": keep only the name part before the timestamp separator
        if (!string.IsNullOrWhiteSpace(result.NguoiTao))
        {
            // Example: "S Leo / 2025-08-29 14:34:15" -> "S Leo"
            var parts = Regex.Split(result.NguoiTao, @"\s*/\s*");
            if (parts.Length > 0)
            {
                result.NguoiTao = CleanSpaces(parts[0]);
            }
        }

        // 3) Extract required fields from the details table (#list-create-transfer-ajax)
        //    ID Phiếu = Line id, Nội dung phiếu = Nội dung, Brand = Brand, Số tiền = Số tiền
        var tbody = doc.DocumentNode.SelectSingleNode("//tbody[@id='list-create-transfer-ajax']");
        if (tbody != null)
        {
            // Find header row and build column index map by normalized header text
            var headerRow = tbody.Descendants("tr").FirstOrDefault(tr => tr.Elements("th").Any());
            if (headerRow != null)
            {
                var headerTexts = headerRow.Elements("th").Select(th => CleanSpaces(th.InnerText)).ToList();
                var colIndexByHeader = new Dictionary<string, int>();
                for (int i = 0; i < headerTexts.Count; i++)
                {
                    var key = Norm(headerTexts[i]);
                    if (!colIndexByHeader.ContainsKey(key)) colIndexByHeader[key] = i;
                }

                // Choose the first data row (skip total rows)
                var dataRow = tbody.Descendants("tr")
                                   .FirstOrDefault(tr => tr.Elements("td").Any() && !Norm(CleanSpaces(tr.InnerText)).StartsWith("tong tien:"));
                if (dataRow != null)
                {
                    var tds = dataRow.Elements("td").ToList();

                    int GetIndex(params string[] headerCandidates)
                    {
                        foreach (var h in headerCandidates)
                        {
                            var n = Norm(h);
                            if (colIndexByHeader.TryGetValue(n, out var idx)) return idx;
                        }
                        return -1;
                    }

                    // Do NOT set ID Phiếu from "Line id". ID Phiếu must be "Mã phiếu" from main info table.

                    // Nội dung phiếu <- Nội dung (preserve <br> as new lines)
                    var idxNoiDung = GetIndex("Nội dung");
                    if (idxNoiDung >= 0 && idxNoiDung < tds.Count)
                    {
                        result.NoiDungPhieu = ExtractMultilineInnerText(tds[idxNoiDung]);
                    }

                    // Brand <- Brand (preserve <br> as new lines)
                    var idxBrand = GetIndex("Brand");
                    if (idxBrand >= 0 && idxBrand < tds.Count)
                    {
                        result.Brand = ExtractMultilineInnerText(tds[idxBrand]);
                    }

                    // Số tiền <- Số tiền
                    var idxSoTien = GetIndex("Số tiền");
                    if (idxSoTien >= 0 && idxSoTien < tds.Count)
                    {
                        result.SoTien = CleanSpaces(tds[idxSoTien].InnerText);
                    }
                }
            }
        }

        // Fallback for amount: scan all text and pick the largest currency-like number
        if (string.IsNullOrWhiteSpace(result.SoTien))
        {
            var allText = CleanSpaces(doc.DocumentNode.InnerText);
            var amt = ExtractAmountFallback(allText);
            if (!string.IsNullOrWhiteSpace(amt)) result.SoTien = amt;
        }

        return result;
    }

    // --- Helpers ---

    private static HtmlNode? NextSiblingCell(HtmlNode cell)
    {
        var n = cell.NextSibling;
        while (n != null && (n.NodeType != HtmlNodeType.Element || (n.Name != "td" && n.Name != "th")))
            n = n.NextSibling;
        return n;
    }

    private static HtmlNode? NextElementSibling(HtmlNode node)
    {
        var n = node.NextSibling;
        while (n != null && n.NodeType != HtmlNodeType.Element)
            n = n.NextSibling;
        return n;
    }

    private static HtmlNode? NextTextNode(HtmlNode node)
    {
        // Walk forward in document order to find the next text node with content
        var iter = node;
        while (iter != null)
        {
            iter = iter.NextSibling ?? iter.ParentNode?.NextSibling;
            if (iter == null) break;
            if (iter.NodeType == HtmlNodeType.Text && !string.IsNullOrWhiteSpace(iter.InnerText))
                return iter;

            // Descend if element
            var firstText = iter.Descendants().FirstOrDefault(d => d.NodeType == HtmlNodeType.Text && !string.IsNullOrWhiteSpace(d.InnerText));
            if (firstText != null) return firstText;
        }
        return null;
    }

    private static string ExtractMultilineInnerText(HtmlNode node)
    {
        if (node == null) return string.Empty;
        // Clone html to replace <br> with line breaks, then extract text
        var html = node.InnerHtml;
        // Normalize all <br> variants to \n
        html = Regex.Replace(html, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
        // Remove remaining tags
        var tmp = new HtmlDocument();
        tmp.LoadHtml(html);
        var raw = tmp.DocumentNode.InnerText;
        // Clean spaces but keep new lines
        raw = raw.Replace("\r", "");
        var lines = raw.Split('\n').Select(CleanSpaces);
        var joined = string.Join("\n", lines.Where(l => !string.IsNullOrWhiteSpace(l)));
        return joined;
    }

    private static string CleanSpaces(string? s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        var t = Regex.Replace(s, @"\u00A0", " "); // non-breaking space
        t = Regex.Replace(t, @"\s+", " ").Trim();
        return t;
    }

    private static string Norm(string? s)
    {
        s ??= string.Empty;
        // Remove diacritics and lowercase
        var formD = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }
        var noAccents = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        noAccents = noAccents.Replace('đ', 'd');
        noAccents = Regex.Replace(noAccents, @"\s+", " ").Trim();
        return noAccents;
    }

    private static bool IsLabel(string normalizedText, List<string> variants)
    {
        foreach (var v in variants)
        {
            var vn = Norm(v);
            if (normalizedText == vn) return true;
            if (normalizedText.StartsWith(vn + " ") || normalizedText.StartsWith(vn + ":") || normalizedText.StartsWith(vn + " :"))
                return true;
        }
        return false;
    }

    private static string? ExtractAmountFallback(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        // Currency-like patterns: 1.234.567 đ, 12,345,678 VNĐ, ₫ 1.234.000, 1 234 000 VND
        var patt = new Regex(@"(?:vnd|vnđ|₫)?\s*([0-9]{1,3}(?:[.,\s][0-9]{3})+)(?:\s*(?:vnd|vnđ|₫))?", RegexOptions.IgnoreCase);
        var matches = patt.Matches(text);
        var candidates = matches.Select(m => m.Groups[1].Value).ToList();
        if (!candidates.Any())
        {
            // plain big numbers with at least 6 digits
            var plain = Regex.Matches(text, @"\b([0-9]{6,})\b");
            candidates = plain.Select(m => m.Groups[1].Value).ToList();
        }
        if (!candidates.Any()) return null;

        int ToInt(string s)
        {
            var normalized = s.Replace(".", "").Replace(",", "").Replace(" ", "");
            return int.TryParse(normalized, out var n) ? n : 0;
        }
        var best = candidates.OrderByDescending(ToInt).FirstOrDefault();
        return best;
    }

    private static string? ExtractFirstUrl(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var m = Regex.Match(text, @"https?://[^\s]+", RegexOptions.IgnoreCase);
        return m.Success ? m.Value : null;
    }

    private static string CsvEscape(string? s)
    {
        s ??= string.Empty;
        if (s.Contains('"') || s.Contains(',') || s.Contains('\n'))
        {
            s = s.Replace("\"", "\"\"");
            return $"\"{s}\"";
        }
        return s;
    }
}


public class TicketInfo
{
    public string NgayKT { get; set; } = string.Empty;
    public string IDPhieu { get; set; } = string.Empty;
    public string NguoiTao { get; set; } = string.Empty;
    public string NhaCungCap { get; set; } = string.Empty;
    public string SoTaiKhoan { get; set; } = string.Empty;
    public string NoiDungPhieu { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string SoTien { get; set; } = string.Empty;
}
