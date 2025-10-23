using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace BootstrapBlazor.Server.Components.Task9;
public partial class DomainByPic : IDisposable
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
    private CancellationTokenSource? TokenSource { get; set; }
    private bool IsLoading { get; set; } = false;
    private List<DomainSearchDto> Datas { get; set; } = new();
    private List<DomainSearchDto> DataFilters { get; set; } = new();
    private List<DomainSearchDto> DataCreates { get; set; } = new();
    private int TotalItems {set;get;} = 0;
    private int TotalFilterItems {set;get;} = 0;
    [NotNull]
    private UniverSheet? _sheetPlugin = null;
    [NotNull]
    private Mask? CustomMask1 { get; set; }

    private bool IsPaging { get; set; } = false;

    private string? _jsonData = null;
    private List<string> MainSites {get;set;} = new(){
        "bong99.biz",
        "one88.info",
        "ta88.pro",
        "388bet.org",
        "lode88.ai",
        "lucky88.co",
        "11bet.com",
        "fabet.fan",
        "dabet.biz",
        "oxbet.fun",
        "nbet.fan",
        "kong88.com",
        "fc88.vin",
        "va88.com",
        "bec88.com",
        "saobet.com",
        "sinbet.com",
        "big88.net",
        "tot88.com",
        "iwin.net",
        "fa88.tv",
        "macau.mn",
        "vinwin.club",
        "v8.fan",
        "shbet.luxe",
        "789bet.navy",
        "hb88.vc",
        "jun88.onl",
        "188bet.pro",
        "fa88.tv"
    };
    
    private readonly Dictionary<string, string> _plugins = new()
    {
        { "ReportPlugin", "univer-sheet/plugin.js" }
    };

    private ITableSearchModel CustomerSearchModel { get; set; } = new DomainSearchFilterDto();
    private ITableSearchModel CustomerFilterSearchModel { get; set; } = new DomainSearchFilterDto();
    private static IEnumerable<int> PageItemsSource => new int[]
    {
        10,
        20,
        40,
        80,
        100
    };

    private Table<DomainSearchDto>? TableRef { get; set; }
    private Table<DomainSearchDto>? TableFilterRef { get; set; }
    
    protected override async Task OnInitializedAsync()
    {  
        base.OnInitialized();
        var result = await DomainSearchService.GetListAsync(new DomainSearchFilterPagingDto(){
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

    private async Task<bool> OnSaveAsync(DomainSearchDto item, ItemChangedType changedType)
    {
        if (changedType == ItemChangedType.Add)
        {
            var result = await DomainSearchService.CreateAsync(item);
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
                oldItem.Team = item.Team;
                oldItem.Pic = item.Pic;
                oldItem.Domain = item.Domain;
                var result = await DomainSearchService.UpdateAsync(item.Id, item);
                if (result.IsSuccess)
                {             
                    await TableRef!.QueryAsync();
                    return true;
                }
            }
        }
        return true;
    }

    private async Task<bool> OnDeleteAsync(IEnumerable<DomainSearchDto> items)
    {
        await Task.WhenAll(items.Select(async i => {
            var result = await DomainSearchService.DeleteAsync(i.Id);
            if (result.IsSuccess)
            {
                Datas.Remove(i);
            }
        }));
        return true;
    }

    private async Task OnTriggerFilterQueryAsync()
    {
        await TableFilterRef!.QueryAsync();
    }
    private async Task OnTriggerQueryAsync()
    {
        await TableRef!.QueryAsync();
    }

    private async Task<QueryData<DomainSearchDto>> OnQueryFilterAsync(QueryPageOptions options)
    {
        await Task.Delay(200);
        string domainSearch = (CustomerFilterSearchModel as DomainSearchFilterDto)?.Domain;
        if(!string.IsNullOrEmpty(domainSearch))
        {
            var result = await DomainSearchService.GetListAsync(new DomainSearchFilterPagingDto(){
                Skip = 0,
                Take = int.MaxValue,
                Text = options.SearchText,
                Domain = domainSearch,
            });

            //options.CustomerSearches = new();
            if (result.IsSuccess)
            {
                List<string> listDomain = new();
                int countDomainNotInResult = 0;
                if(domainSearch != null){

                    if(domainSearch.Contains('\n'))
                    {
                        listDomain.AddRange(domainSearch.Split('\n')
                            .Select(x => x.Trim().Replace("https://", "").Replace("http://", "").Replace("/", ""))
                            .Where(x => !string.IsNullOrWhiteSpace(x))); // Filter out empty domains
                    }
                    else
                    {
                        listDomain.AddRange(domainSearch.Split(" ")
                            .Select(x => x.Trim().Replace("https://", "").Replace("http://", "").Replace("/", ""))
                            .Where(x => !string.IsNullOrWhiteSpace(x))); // Filter out empty domains
                    }
                }

                if(listDomain.Count > 0)
                { 
                    // Add domains not in result with "None" values
                    var domainsNotInResult = listDomain.Where(x => !result.Data.Any(y => y.Domain == x)).ToList();
                    result.Data.AddRange(domainsNotInResult.Select(x => new DomainSearchDto()
                    {
                        Domain = x,
                        Team = "None",
                        Pic = "None"
                    }));
                    
                    // Sort result.Data to match the order of listDomain
                    var orderedData = new List<DomainSearchDto>();
                    foreach (var domain in listDomain)
                    {
                        var item = result.Data.FirstOrDefault(x => x.Domain == domain);
                        if (item != null)
                        {
                            item.IsMainSite = MainSites.Contains(item.Domain);
                            orderedData.Add(item);
                        }
                    }
                    result.Data = orderedData;
                }

                DataFilters = result.Data;
                TotalFilterItems = result.Data.Count();
            }
        }
       
        return new QueryData<DomainSearchDto>()
        {
            Items = DataFilters,
            TotalCount = TotalFilterItems
        };
    }
    
    private async Task<QueryData<DomainSearchDto>> OnQueryAsync(QueryPageOptions options)
    {
        await Task.Delay(200);
        string domainSearch = (CustomerSearchModel as DomainSearchFilterDto)?.Domain;
        var result = await DomainSearchService.GetListAsync(new DomainSearchFilterPagingDto(){
            Skip = (options.PageIndex - 1) * options.PageItems,
            Take = options.PageItems,
            Text = options.SearchText,
            Domain = domainSearch,
            Team = (CustomerSearchModel as DomainSearchFilterDto)?.Team,
            Pic = (CustomerSearchModel as DomainSearchFilterDto)?.Pic
        });

        //options.CustomerSearches = new();
        if (result.IsSuccess)
        {
            Datas = result.Data;
            TotalItems = (result.TotalItems ?? 0);
        }
        return new QueryData<DomainSearchDto>()
        {
            Items = Datas,
            TotalCount = TotalItems
        };
    }

    
    private async Task CopyData()
    {
        string titles ="Tên miền\tThuộc team\tNgười phụ trách\t";
        var data = Datas.Select(row => 
            string.Join("\t", row.Domain, row.Team, row.Pic)
        ).ToList();
        data.Insert(0, titles);
        await ClipboardService.Copy(string.Join("\n", data));
        await Toast.Success("Success", "Đã copy dữ liệu vào clipboard");
    }
    
    private async Task CopyFilterData()
    {
        string titles ="Tên miền\tThuộc team\tNgười phụ trách\t";
        var data = DataFilters.Select(row => 
            string.Join("\t", row.Domain, row.Team, row.Pic)
        ).ToList();
        data.Insert(0, titles);
        await ClipboardService.Copy(string.Join("\n", data));
        await Toast.Success("Success", "Đã copy dữ liệu vào clipboard");
    }

    private async Task OnSaveExcelData()
    {
        try
        {   
            var result = await _sheetPlugin.PushDataAsync(new UniverSheetData()
            {
                CommandName = "GetWorkbook"
            });
            _jsonData = result?.Data?.ToString();
            DataCreates  = UniverSheetHelper.Parse(_jsonData);
            await MaskService.Show(new MaskOption()
            {
                ChildContent = builder => builder.AddContent(0, new MarkupString("<i class=\"text-white fa-solid fa-3x fa-spinner fa-spin-pulse\"></i><span class=\"ms-3 fs-2 text-white\">Đang thêm dữ liệu vui lòng chờ ....</span>")),
            }, CustomMask1);

            var resultCreate = await DomainSearchService.CreateListAsync(DataCreates);
            if (resultCreate.IsSuccess)
            {
                await TableRef!.QueryAsync();
                await Toast.Success("Thông báo", "Thêm dữ liệu thành công.");
            }else{
                await Toast.Error("Thông báo", "Thêm dữ liệu thất bại.");
            }
        }
        catch (Exception ex)
        {
            await Toast.Error("Thông báo", ex.Message);
        }finally{
            await MaskService.Close(CustomMask1);
            StateHasChanged();
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


public static class UniverSheetHelper
{
    public static List<DomainSearchDto> Parse(string json)
    {
        var list = new List<DomainSearchDto>();

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
                string team = rowObj["1"]?["v"]?.ToString();
                string pic = rowObj["2"]?["v"]?.ToString();

                // Bỏ qua nếu không có domain
                if (string.IsNullOrWhiteSpace(domain))
                    continue;

                list.Add(new DomainSearchDto
                {
                    Domain = domain,
                    Team = team,
                    Pic = pic
                });
            }
        }

        return list;
    }
}