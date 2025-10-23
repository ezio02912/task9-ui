
using System.Text.Json.Serialization;
using BootstrapBlazor.Server.Identity;
using Microsoft.AspNetCore.Components.Authorization;

namespace BootstrapBlazor.Server.Components.Task9;

[JSModuleAutoLoader("Task9/BaoCaoSEOTong.razor.js", JSObjectReference = true)]
public partial class BaoCaoSEOTong : IDisposable
{
    #region Inject
    [Inject]
    private IBrandService BrandService { get; set; }
    [Inject]
    private IPicDomainAssignmentService PicDomainAssignmentService { get; set; }
    [Inject]
    private IUserManagerService UserManagerService { get; set; }
    [Inject]
    private AuthenticationStateProvider AuthStateProvider { get; set; }

    [Inject]
    private IGroupService GroupService { get; set; }
    [Inject]
    private IKeywordRankingService KeywordRankingService { get; set; }

    [Inject]
    private IMainsiteService MainsiteService { get; set; }
    [Inject]
    [NotNull]
    private ToastService? Toast { get; set; }
    [Inject]
    [NotNull]
    private CodeSnippetService? CodeSnippetService { get; set; }

    #endregion

    private DateTimeRangeValue MonthRangeValue { get; set; } = new() { Start = DateTime.Today.AddMonths(-1), End = DateTime.Today };
    private bool IsLoading { get; set; }
    private string ButtonIcon { get; } = "fa-solid fa-cloud-arrow-up";
    private string LoadingIcon { get; } = "fa-solid fa-spinner fa-spin-pulse";
    private string Icon => IsLoading ? LoadingIcon : ButtonIcon;
    private CancellationTokenSource? TokenSource { get; set; }
    private ReportType SelectedReportType { get; set; } = ReportType.Keyword;
    private LevelSearchVolumne SelectedLevelSearchVolumne { get; set; } = LevelSearchVolumne.All;

    private KeywordRankingFilter Filter { get; set; } = new();
    private List<string>? Pics { get; set; }
    private KeywordRankingReportResponseDto ChartData = new();
    private List<string>? Mainsites { get; set; }
    [NotNull]
    private List<SelectedItem<BrandDto>>? Brands { get; set; }
    private List<SelectedItem<GroupDto>>? Groups { get; set; }
    private List<GroupDto>? SelectedGroups { get; set; } = null;
    private BrandDto SelectedBrand = new();
    private IEnumerable<SelectedItem> PicItems => Pics?.Select(i => new SelectedItem(i, i)).ToList() ?? new();
    private SelectedItem? SelectedPic { get; set; }
    
    [CascadingParameter]
    [NotNull]
    private PageLayout? RootPage { get; set; }
    public string Role => RootPage?.Role ?? "";

    [NotNull]
    private Chart BarChart { get; set; } = new Chart();
    private ChartDataForJS ChartDataJS { get; set; } = new();
    private KeywordSumarySeoReportDto SummaryData = new();

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(LoadBrands(), LoadPics(), LoadMainsites());
    }
    #region init chart
    private List<int> GetDataForRank(KeywordRankingReportResponseDto data, List<string> labels, int rankIndex)
    {
        var result = new List<int>();

        foreach (var label in labels)
        {
            var dataForLabel = data.AllData.Where(i => i.ResultGroup == label).FirstOrDefault();
            if (dataForLabel != null)
            {
                var value = rankIndex switch
                {
                    1 => dataForLabel.Rank1,
                    2 => dataForLabel.Rank2,
                    3 => dataForLabel.Rank3,
                    4 => dataForLabel.Rank4,
                    5 => dataForLabel.Rank5,
                    6 => dataForLabel.Rank6,
                    7 => dataForLabel.Rank7,
                    8 => dataForLabel.Rank8,
                    9 => dataForLabel.Rank9,
                    10 => dataForLabel.Rank10,
                    _ => 0
                };
                result.Add(value);
            }
            else
            {
                result.Add(0);
            }
        }

        return result;
    }
    
#endregion
    private async Task OnViewReport()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            StateHasChanged();
            UpdateFilter();
            // Load data in parallel
            var reportTask = KeywordRankingService.GetReportKeywordRankingAsync(Filter);
            var summaryTask = KeywordRankingService.GetSummaryReportKeywordRankingAsync(Filter);

            await Task.WhenAll(reportTask, summaryTask);

            var result = await reportTask;
            var resultSummary = await summaryTask;

            if (result.Status == true)
            {
                ChartData = result.Data;
                if(resultSummary.Status == true)
                {
                    SummaryData = resultSummary.Data;
                    await Task.Run(() => GroupSimilarKeywords());
                }
                // Update chart với dữ liệu mới
                await UpdateChart();
            }
            else if (!result.Status)
            {
                await Toast.Error("Lỗi khi tải dữ liệu: " + result.Message);
            }
        }
        catch (Exception ex)
        {
            await Toast.Error("Lỗi khi tải dữ liệu: " + ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
    private void UpdateFilter()
    {
        Filter.ReportDateType = ReportDateType.Month;
        Filter.BrandId = SelectedBrand.Id;
        Filter.Brand =  SelectedBrand.Name;
        Filter.GroupIds = SelectedGroups?.Select(x => x.Id).ToList() ?? [];
        Filter.GroupNames = SelectedGroups?.Select(x => x.Name).ToList() ?? [];
        Filter.PicName = SelectedPic?.Value ?? string.Empty;
        Filter.ReportType = SelectedReportType;
        Filter.LevelSearchVolumne = SelectedLevelSearchVolumne;
        Filter.FromDate = new DateTime(MonthRangeValue.Start.Year, MonthRangeValue.Start.Month, 1, 0, 0, 0);
        int dateOfMonth = DateTime.DaysInMonth(MonthRangeValue.End.Year, MonthRangeValue.End.Month);
        Filter.ToDate = new DateTime(MonthRangeValue.End.Year, MonthRangeValue.End.Month, dateOfMonth, 23, 59, 59);
    }
    #region Select components handler


    private async Task LoadMainsites()
    {
        var result = await MainsiteService.GetListAsync(new FilterPagingBase()
        {
            Take = Int32.MaxValue
        });
        if (result.Status == true)
        {
            Mainsites = result.Data.Select(x => x.Domain).ToList();
        }
    }

    private async Task LoadBrands()
    {
        var result = await BrandService.GetListAsync();
        if (result.Status == true)
        {
            Brands = [.. result.Data.Select(i => new SelectedItem<BrandDto>(i, i.Name!))];
            SelectedBrand = result.Data.FirstOrDefault()!;
            await LoadGroups();
        }
    }
    private async Task OnBrandSelectedItemChanged(SelectedItem<BrandDto?> item)
    {
        SelectedBrand = item.Value!;
        await LoadGroups();
        StateHasChanged();
    }


    private async Task LoadPics()
    {
        // Get current user info from JWT token
        var apiAuthProvider = (ApiAuthenticationStateProvider)AuthStateProvider;
        var currentUserIdStr = await apiAuthProvider.GetCurrentUserId();
        var currentUserRole = await apiAuthProvider.GetUserRolesAsync();
        int? currentUserId = int.TryParse(currentUserIdStr, out var userId) ? userId : null;
        
        var result = await UserManagerService.GetBasicSeoUserInfoAsync(currentUserId, currentUserRole);
        if (result != null && result.Count > 0)
        {
            Pics = result.Select(x => x.FullName).ToList();
            
            // If role is SEO, auto-select the first (and only) item
            if (Role == "SEO" && Pics.Count() > 0)
            {
                SelectedPic = new SelectedItem(Pics[0], Pics[0]);
            }
        }
        else
        {
            Pics = null;
        }
    }

    private async Task LoadGroups()
    {
        var result = await GroupService.GetListAsync(0);
        if (result.Status == true)
        {
            var groups = result.Data;
            if (SelectedBrand is not null)
            {
                Groups = [.. groups.Where(x => x.BrandId == SelectedBrand.Id).Select(i => new SelectedItem<GroupDto>(i, i.Name!))];
                SelectedGroups = null;
            }
        }
    }
    #endregion


    #region group keyword

    private void GroupSimilarKeywords()
    {
        try
        {
            if (SummaryData?.RawData == null || !SummaryData.RawData.Any())
                return;

            var result = new List<ByKeywordsSummaryDto>();

            // Nhóm dữ liệu theo người phụ trách
            var groupedByPic = SummaryData.RawData.GroupBy(x => x.PicName)
                                          .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var picGroup in groupedByPic)
            {
                var picName = picGroup.Key;
                var keywords = picGroup.Value;

                // Nhóm từ khóa theo prefix
                var prefixGroups = new Dictionary<string, List<ByKeywordsSummaryDto>>();

                // Tìm tiền tố chung cho mỗi từ khóa
                foreach (var keyword in keywords)
                {
                    // Chuẩn hóa từ khóa (chuyển thành chữ thường, loại bỏ khoảng trắng)
                    string normalizedKeyword = keyword.Keyword.ToLowerInvariant().Replace(" ", "");

                    // Tìm tiền tố phù hợp
                    string matchedPrefix = FindPrefix(normalizedKeyword, prefixGroups.Keys);

                    if (!string.IsNullOrEmpty(matchedPrefix))
                    {
                        // Thêm vào nhóm đã tồn tại
                        prefixGroups[matchedPrefix].Add(keyword);
                    }
                    else
                    {
                        // Tạo nhóm mới với tiền tố là từ khóa đã chuẩn hóa
                        prefixGroups[normalizedKeyword] = new List<ByKeywordsSummaryDto> { keyword };
                    }
                }

                // Sắp xếp các nhóm theo volume lớn nhất trong mỗi nhóm
                var sortedGroups = prefixGroups.Values
                    .OrderByDescending(group => group.Max(k => k.Volume))
                    .ToList();

                // Thêm từng nhóm đã sắp xếp vào kết quả
                var sortedKeywords = new List<ByKeywordsSummaryDto>();
                foreach (var group in sortedGroups)
                {
                    // Trong mỗi nhóm, sắp xếp theo volume giảm dần
                    var sortedGroup = group.OrderByDescending(k => k.Volume).ToList();
                    sortedKeywords.AddRange(sortedGroup);
                }

                result.AddRange(sortedKeywords);
            }

            SummaryData.RawData = result;
        }
        catch (Exception ex)
        {
        }
    }

    /// <summary>
    /// Tìm tiền tố phù hợp từ danh sách các tiền tố đã biết
    /// </summary>
    private string FindPrefix(string keyword, IEnumerable<string> existingPrefixes)
    {
        foreach (var prefix in existingPrefixes)
        {
            // Kiểm tra nếu từ hiện tại bắt đầu bằng prefix hoặc prefix bắt đầu bằng từ hiện tại
            if (keyword.StartsWith(prefix) || prefix.StartsWith(keyword))
            {
                return prefix;
            }
        }

        return string.Empty; // Không tìm thấy tiền tố phù hợp
    }

    #endregion

    private async Task OnClickTabItemAsync(TabItem item)
    {
        if (item.Text == "Biểu đồ")
        {
            // Đợi một chút để DOM render
            await Task.Delay(100);
            
            // Resize chart để đảm bảo responsive
            await InvokeVoidAsync("resizeChart", Id);
        }
    }

    /// <summary>
    /// Update chart với dữ liệu mới
    /// </summary>
    private async Task UpdateChart()
    {
        ChartDataJS = CreateChartDataForJS();
        await InvokeVoidAsync("updatedStacked", Id, ChartDataJS);
    }

    protected override async Task InvokeInitAsync()
    {
        // Chỉ khởi tạo chart nếu canvas element tồn tại
        await Task.Delay(50); // Đợi DOM render
        ChartDataJS = CreateChartDataForJS();
        await InvokeVoidAsync("initStackedDemo", Id, ChartDataJS);
        
        // Ensure chart is properly sized after initialization
        await Task.Delay(100);
        await InvokeVoidAsync("resizeChart", Id);
    }

    
    /// <summary>
    /// Tạo dữ liệu cho JavaScript Chart
    /// </summary>
    private ChartDataForJS CreateChartDataForJS()
    {
        var result = new ChartDataForJS();

        // Improved color palette for better UX - từ quan trọng đến ít quan trọng
        var colors = new string[]
        {
            "rgb(54, 162, 235, 0.5)",  // Blue
            "rgb(75, 192, 192, 0.5)",  // Teal
            "rgb(255, 99, 132, 0.5)",  // Pink Red
            "rgb(255, 159, 64, 0.5)",  // Orange
            "rgb(255, 205, 86, 0.5)",  // Yellow
            "rgb(255, 99, 71, 0.5)",   // Tomato
            "rgb(255, 192, 203, 0.5)", // Light Pink
            "rgb(153, 102, 255, 0.5)", // Purple
            "rgb(144, 238, 144, 0.5)", // Light Green
            "rgb(54, 54, 100, 0.5)"    // Navy
        };


        var borderColors = new string[]
        {
            "rgb(54, 162, 235,1)",  // Blue
            "rgb(75, 192, 192, 1)",  // Teal
            "rgb(255, 99, 132, 1)",  // Pink Red
            "rgb(255, 159, 64, 1)",  // Orange
            "rgb(255, 205, 86, 1)",  // Yellow
            "rgb(255, 99, 71, 1)",   // Tomato
            "rgb(255, 192, 203, 1)", // Light Pink
            "rgb(153, 102, 255, 1)", // Purple
            "rgb(144, 238, 144, 1)", // Light Green
            "rgb(54, 54, 100, 1)" 
        };


        if (ChartData.AllData.Count > 0)
        {
            // Lấy labels (tên domains/groups)
            result.Labels = ChartData.AllData.Select(i => i.ResultGroup).Distinct().ToList();
            
            var rankGroups = new[]
            {
                new { Label = "Top 1", RankNumber = 1 },
                new { Label = "Top 2", RankNumber = 2 },
                new { Label = "Top 3", RankNumber = 3 },
                new { Label = "Top 4", RankNumber = 4 },
                new { Label = "Top 5", RankNumber = 5 },
                new { Label = "Top 6", RankNumber = 6 },
                new { Label = "Top 7", RankNumber = 7 },
                new { Label = "Top 8", RankNumber = 8 },
                new { Label = "Top 9", RankNumber = 9 },
                new { Label = "Top 10", RankNumber = 10 }
            };

            for (int i = 0; i < rankGroups.Length; i++)
            {
                var group = rankGroups[i];
                var groupData = new List<int>();

                foreach (var label in result.Labels)
                {
                    var dataForLabel = ChartData.AllData.FirstOrDefault(x => x.ResultGroup == label);
                    if (dataForLabel != null)
                    {
                        var value = group.RankNumber switch
                        {
                            1 => dataForLabel.Rank1,
                            2 => dataForLabel.Rank2,
                            3 => dataForLabel.Rank3,
                            4 => dataForLabel.Rank4,
                            5 => dataForLabel.Rank5,
                            6 => dataForLabel.Rank6,
                            7 => dataForLabel.Rank7,
                            8 => dataForLabel.Rank8,
                            9 => dataForLabel.Rank9,
                            10 => dataForLabel.Rank10,
                            _ => 0
                        };
                        groupData.Add(value);
                    }
                    else
                    {
                        groupData.Add(0);
                    }
                }

                // Create adjusted data with minimum height for small values
                var adjustedData = groupData.Select(val => {
                    // Set minimum height for values 1 and 2 to fit text label
                    if (val == 1 || val == 2) {
                        return 3; // Minimum height to display label clearly
                    }
                    return val;
                }).ToList();

                result.Datasets.Add(new DataSetForJSChart
                {
                    Label = group.Label,
                    Data = adjustedData,
                    OriginalData = groupData, // Keep original values for label display
                    BackgroundColor = colors[i % colors.Length],
                    BorderColor = borderColors[i % borderColors.Length],
                    BorderWidth = 1
                });
            }
        }
        else
        {
            // Demo data nếu không có dữ liệu thực
            result.Labels = new List<string> { "Demo 1", "Demo 2", "Demo 3" };
            
            for (int i = 0; i < 3; i++)
            {
                var demoData = new List<int> { 0, 0, 0 };
                var adjustedDemoData = demoData.Select(val => val == 1 || val == 2 ? 3 : val).ToList();
                
                result.Datasets.Add(new DataSetForJSChart
                {
                    Label = $"Top {i + 1}",
                    Data = adjustedDemoData,
                    OriginalData = demoData,
                    BackgroundColor = colors[i % colors.Length],
                    BorderColor = borderColors[i % borderColors.Length],
                    BorderWidth = 1
                });
            }
        }

        return result;
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

public class ChartDataForJS
{
    public List<string> Labels { get; set; } = new();
    public List<DataSetForJSChart> Datasets { get; set; } = new();
}

public class DataSetForJSChart
{
    public string Label { get; set; } = string.Empty;
    public List<int> Data { get; set; } = new();
    [JsonPropertyName("originalData")]
    public List<int> OriginalData { get; set; } = new(); // Original values for label display
    public string BackgroundColor { get; set; } = string.Empty;
    public string BorderColor { get; set; } = string.Empty;
    public int BorderWidth { get; set; }
}
