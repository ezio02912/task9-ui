using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace BootstrapBlazor.Server.Components.Task9;
public partial class ReportBrandKeyword : IDisposable
{
    #region Inject
    [Inject, NotNull]
    private IWebHostEnvironment? WebHost { get; set; }
    [Inject]
    private IDomainCheckDailyService DomainCheckDailyService { get; set; }
    [Inject, NotNull]
    private MaskService? MaskService { get; set; }
    [Inject]
    [NotNull]
    private ToastService? Toast { get; set; }
    #endregion
    private CancellationTokenSource? TokenSource { get; set; }
    private bool IsLoading { get; set; } = false;
    private List<DomainCheckDailyDto> Datas { get; set; } = new();
    private List<DomainCheckDailyDto> DataCreates { get; set; } = new();
    private int TotalItems {set;get;} = 0;
    [NotNull]
    private UniverSheet? _sheetPlugin = null;
    [NotNull]
    private Mask? CustomMask1 { get; set; }


    private string? _jsonData = null;
    
    private readonly Dictionary<string, string> _plugins = new()
    {
        { "ReportPlugin", "univer-sheet/plugin.js" }
    };

    private ITableSearchModel CustomerSearchModel { get; set; } = new DomainSearchFilterDto();
    private static IEnumerable<int> PageItemsSource => new int[]
    {
        10,
        20,
        40,
        80,
        100
    };

    private Table<DomainCheckDailyDto>? TableRef { get; set; }

    protected override async Task OnInitializedAsync()
    {  
        base.OnInitialized();
        var result = await DomainCheckDailyService.GetListAsync(new DomainCheckDailyFilterPagingDto(){
            Skip = 0,
            Take = 10,
        });
        if (result.IsSuccess)
        {
            Datas = result.Data;
            TotalItems = result.Data.Count;
        }
    }
    private async Task OnReadyAsync()
    {
        await Toast.Information("Thông báo", "Excel đã sẵn sàng để thêm dữ liệu.");
    }



    private async Task<bool> OnSaveAsync(DomainCheckDailyDto item, ItemChangedType changedType)
    {
        if (changedType == ItemChangedType.Add)
        {
            var result = await DomainCheckDailyService.CreateAsync(item);
            if (result.IsSuccess)
            {             
                await TableRef!.QueryAsync();
                return true;
            }
        }
        else
        {
            var oldItem = Datas.FirstOrDefault(i => i.Id == item.Id);
            if (oldItem != null)
            {
                oldItem.Type = item.Type;
                oldItem.IsMainsite = item.IsMainsite;
                oldItem.BackgroundColor = item.BackgroundColor;
                oldItem.TextColor = item.TextColor;
                oldItem.IsBold = item.IsBold;
                oldItem.Domain = item.Domain;
                var result = await DomainCheckDailyService.UpdateAsync(item.Id, item);
                if (result.IsSuccess)
                {             
                    await TableRef!.QueryAsync();
                    return true;
                }
            }
        }
        return true;
    }

    private async Task<bool> OnDeleteAsync(IEnumerable<DomainCheckDailyDto> items)
    {
        await Task.WhenAll(items.Select(async i => {
            var result = await DomainCheckDailyService.DeleteAsync(i.Id);
            if (result.IsSuccess)
            {
                Datas.Remove(i);
            }
        }));
        return true;
    }

     private async Task<QueryData<DomainCheckDailyDto>> OnQueryAsync(QueryPageOptions options)
    {
        await Task.Delay(200);
        var result = await DomainCheckDailyService.GetListAsync(new DomainCheckDailyFilterPagingDto()
        {
            Skip = (options.PageIndex - 1) * options.PageItems,
            Take = options.PageItems,
            Domain = options.SearchText
        });

        //options.CustomerSearches = new();
        if (result.IsSuccess)
        {
            Datas = result.Data;
            TotalItems = result.TotalItems ?? 0;
        }
        return new QueryData<DomainCheckDailyDto>()
        {
            Items = Datas,
            TotalCount = TotalItems
        };
    }

    private async Task OnFillColorExcelData()
    {
        var result = await _sheetPlugin.PushDataAsync(new UniverSheetData()
        {
            CommandName = "GetWorkbook"
        });
        _jsonData = result?.Data?.ToString();
        var domains = UniverConvertToSearchHelper.Parse(_jsonData);

        await MaskService.Show(new MaskOption()
        {
            ChildContent = builder => builder.AddContent(0, new MarkupString("<i class=\"text-white fa-solid fa-3x fa-spinner fa-spin-pulse\"></i><span class=\"ms-3 fs-2 text-white\">Đang tô màu dữ liệu vui lòng chờ ....</span>")),
        }, CustomMask1);

        if(!string.IsNullOrWhiteSpace(domains))
        {
            var resultGet = await DomainCheckDailyService.GetListAsync(new DomainCheckDailyFilterPagingDto(){
                Skip = 0,
                Take = int.MaxValue,
                Domain = domains
            });
            if (resultGet.IsSuccess)
            {
                try
                {
                    // Update lại màu background color, text color, bold json data rồi set lại cho workbook
                    var updatedJsonData = UpdateWorkbookStyles(_jsonData, resultGet.Data);
                    
                    // Kiểm tra JSON trước khi gửi
                    if (IsValidWorkbookJson(updatedJsonData))
                    {
                                            // Set lại workbook với dữ liệu đã được update
                    await _sheetPlugin.PushDataAsync(new UniverSheetData()
                    {
                        CommandName = "setWorkbookCommand",
                        Data = updatedJsonData
                    });
                        
                        await Toast.Success("Thông báo", "Đã cập nhật màu sắc cho dữ liệu thành công.");
                    }
                    else
                    {
                        await Toast.Error("Thông báo", "Dữ liệu JSON không hợp lệ, không thể cập nhật workbook.");
                    }
                }
                catch (Exception ex)
                {
                    await Toast.Error("Thông báo", $"Lỗi khi cập nhật workbook: {ex.Message}");
                }
            }
        }

        await MaskService.Close(CustomMask1);
        StateHasChanged();
    }

    /// <summary>
    /// Update lại màu sắc cho workbook dựa trên dữ liệu từ database
    /// </summary>
    private string UpdateWorkbookStyles(string jsonData, List<DomainCheckDailyDto> domainData)
    {
        try
        {
            var root = JObject.Parse(jsonData);
            
            // Kiểm tra cấu trúc cơ bản
            if (root["sheets"] == null || root["styles"] == null)
            {
                return jsonData;
            }

            var sheets = root["sheets"] as JObject;
            var styles = root["styles"] as JObject;
            
            if (sheets == null || styles == null) return jsonData;

            // Tạo dictionary để map domain với dữ liệu
            var domainDict = domainData.ToDictionary(d => d.Domain, d => d);

            foreach (var sheetProp in sheets.Properties())
            {
                var sheet = sheetProp.Value as JObject;
                if (sheet == null) continue;

                var cellData = sheet["cellData"] as JObject;
                if (cellData == null) continue;

                // Lặp qua tất cả rows
                foreach (var rowProp in cellData.Properties())
                {
                    var rowObj = rowProp.Value as JObject;
                    if (rowObj == null) continue;

                    // Lặp qua tất cả cells trong row
                    foreach (var cellProp in rowObj.Properties())
                    {
                        var cell = cellProp.Value as JObject;
                        if (cell == null) continue;

                        var cellValue = cell["v"]?.ToString();
                        if (string.IsNullOrWhiteSpace(cellValue) || cellValue == "N/A")
                            continue;

                        // Kiểm tra xem domain có trong database không
                        if (domainDict.TryGetValue(cellValue, out var domainInfo))
                        {
                            // Tạo style mới cho cell
                            var newStyleId = $"style_{Guid.NewGuid():N}";
                            var newStyle = CreateCellStyle(domainInfo);
                            
                            // Thêm style mới vào styles
                            styles[newStyleId] = newStyle;

                            // Cập nhật style cho cell
                            cell["s"] = newStyleId;
                        }
                    }
                }
            }

            // Đảm bảo cấu trúc JSON hợp lệ trước khi trả về
            var result = root.ToString();
            
            // Validate JSON trước khi trả về
            try
            {
                JObject.Parse(result);
                return result;
            }
            catch
            {
                // Nếu JSON không hợp lệ, trả về dữ liệu gốc
                return jsonData;
            }
        }
        catch (Exception ex)
        {
            // Log error nếu cần
            return jsonData;
        }
    }

    /// <summary>
    /// Tạo style mới cho cell dựa trên thông tin domain
    /// </summary>
    private JObject CreateCellStyle(DomainCheckDailyDto domainInfo)
    {
        var style = new JObject
        {
            ["ff"] = "Arial",
            ["fs"] = 10,
            ["it"] = 0,
            ["bl"] = domainInfo.IsBold ? 1 : 0,
            ["ul"] = new JObject { ["s"] = 0 },
            ["st"] = new JObject { ["s"] = 0 },
            ["ol"] = new JObject { ["s"] = 0 },
            ["tr"] = new JObject { ["a"] = 0, ["v"] = 0 },
            ["td"] = 0,
            ["ht"] = 0,
            ["vt"] = 3,
            ["tb"] = 0,
            ["pd"] = new JObject
            {
                ["t"] = 0,
                ["b"] = 2,
                ["l"] = 2,
                ["r"] = 2
            }
        };

        // Thêm background color nếu có
        if (!string.IsNullOrWhiteSpace(domainInfo.BackgroundColor))
        {
            style["bg"] = new JObject
            {
                ["rgb"] = domainInfo.BackgroundColor
            };
        }

        // Thêm text color nếu có
        if (!string.IsNullOrWhiteSpace(domainInfo.TextColor))
        {
            style["cl"] = new JObject
            {
                ["rgb"] =  domainInfo.TextColor
            };
        }


        return style;
    }

    /// <summary>
    /// Chuyển đổi màu sắc sang định dạng RGB
    /// </summary>
    private string ConvertToRgb(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return "rgb(255,255,255)";

        // Nếu đã là định dạng RGB thì giữ nguyên
        if (color.StartsWith("rgb("))
            return color;

        // Nếu là hex color thì chuyển sang RGB
        if (color.StartsWith("#"))
        {
            try
            {
                var hex = color.TrimStart('#');
                if (hex.Length == 6)
                {
                    var r = Convert.ToInt32(hex.Substring(0, 2), 16);
                    var g = Convert.ToInt32(hex.Substring(2, 2), 16);
                    var b = Convert.ToInt32(hex.Substring(4, 2), 16);
                    return $"rgb({r},{g},{b})";
                }
            }
            catch
            {
                // Nếu có lỗi thì trả về màu trắng
            }
        }

        // Mặc định trả về màu trắng
        return "rgb(255,255,255)";
    }

    /// <summary>
    /// Kiểm tra xem JSON có phải là workbook hợp lệ không
    /// </summary>
    private bool IsValidWorkbookJson(string jsonData)
    {
        try
        {
            var root = JObject.Parse(jsonData);
            
            // Kiểm tra các thuộc tính bắt buộc
            if (root["sheets"] == null || root["styles"] == null)
                return false;

            var sheets = root["sheets"] as JObject;
            if (sheets == null || !sheets.Properties().Any())
                return false;

            // Kiểm tra ít nhất một sheet có cấu trúc hợp lệ
            foreach (var sheetProp in sheets.Properties())
            {
                var sheet = sheetProp.Value as JObject;
                if (sheet != null && sheet["cellData"] != null)
                {
                    return true; // Có ít nhất một sheet hợp lệ
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
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
}


public static class UniverConvertToSearchHelper
{
    public static string Parse(string json)
    {
        var domains = new List<string>();
        
        try
        {
            // Parse JSON gốc
            var root = JObject.Parse(json);

            // Lấy phần sheets
            var sheets = root["sheets"] as JObject;
            if (sheets == null) return string.Empty;

            foreach (var sheetProp in sheets.Properties())
            {
                var sheet = sheetProp.Value;
                var cellData = sheet["cellData"] as JObject;
                if (cellData == null) continue;

                // Lấy tất cả rows từ sheet
                foreach (var rowProp in cellData.Properties())
                {
                    var rowObj = rowProp.Value as JObject;
                    if (rowObj == null) continue;

                    // Lấy tất cả cells trong row
                    foreach (var cellProp in rowObj.Properties())
                    {
                        var cell = cellProp.Value as JObject;
                        if (cell == null) continue;

                        // Lấy giá trị của cell
                        var cellValue = cell["v"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(cellValue) && cellValue != "N/A")
                        {
                            // Kiểm tra xem có phải domain/URL không
                            if (IsValidDomain(cellValue))
                            {
                                domains.Add(cellValue);
                            }
                        }
                    }
                }
            }

            // Loại bỏ duplicates và trả về
            return string.Join("\n", domains.Distinct());
        }
        catch (Exception ex)
        {
            // Log error nếu cần
            return string.Empty;
        }
    }

    /// <summary>
    /// Kiểm tra xem string có phải là domain/URL hợp lệ không
    /// </summary>
    private static bool IsValidDomain(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        // Bỏ qua các giá trị không phải domain
        if (value == "N/A" || value.Length < 3)
            return false;

        // Kiểm tra xem có chứa dấu chấm (dot) không - đặc trưng của domain
        if (value.Contains("."))
            return true;

        // Kiểm tra xem có phải là URL không
        if (value.StartsWith("http://") || value.StartsWith("https://"))
            return true;

        return false;
    }
}
public static class UniverSheetDomainDailyHelper
{
    public static List<DomainCheckDailyDto> Parse(string json)
    {
        var list = new List<DomainCheckDailyDto>();

        // Parse JSON gốc
        var root = JObject.Parse(json);

        // Lấy phần sheets
        var sheets = root["sheets"] as JObject;
        if (sheets == null) return list;

        foreach (var sheetProp in sheets.Properties())
        {
            var sheet = sheetProp.Value;
            var cellData = sheet["cellData"] as JObject;
            if (cellData == null) continue;

            foreach (var rowProp in cellData.Properties())
            {
                int rowIndex = int.Parse(rowProp.Name);
                var rowObj = rowProp.Value as JObject;
                if (rowObj == null) continue;

                string domain = rowObj["0"]?["v"]?.ToString();
                string type = rowObj["1"]?["v"]?.ToString();
                string pic = rowObj["2"]?["v"]?.ToString();

                // Bỏ qua nếu không có domain
                if (string.IsNullOrWhiteSpace(domain))
                    continue;

                list.Add(new DomainCheckDailyDto
                {
                    Domain = domain,
                    Type = type,
                    IsMainsite = false,
                    BackgroundColor = "",
                    TextColor = "",
                    IsBold = false
                });
            }
        }

        return list;
    }
}