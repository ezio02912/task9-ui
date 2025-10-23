
using BootstrapBlazor.Server.Identity;
using Microsoft.AspNetCore.Components.Authorization;

namespace BootstrapBlazor.Server.Components.Task9;
public partial class EvaluateSeoPerformance : IDisposable
{
    #region Inject
    [Inject]
    private ISeoKpiWeightService SeoKpiWeightService { get; set; }
    [Inject]
    private IKeywordRankingService KeywordRankingService { get; set; }
    [Inject]
    private IUserManagerService UserManagerService { get; set; }
    [Inject]
    private AuthenticationStateProvider AuthStateProvider { get; set; }


    [Inject]
    [NotNull]
    private ToastService? Toast { get; set; }

    #endregion
    private DateTime YearValue { get; set; } = DateTime.Today;
    private bool IsLoading { get; set; }  = false;
    private CancellationTokenSource? TokenSource { get; set; }
    private EmployeeType SelectedEmployeeType { get; set; } = EmployeeType.Official;
    private List<SeoKpiWeightDto> DataJuniorKpis = new();
    private List<SeoKpiWeightDto> DataMiddleKpis = new();
    private List<SeoKpiWeightDto> DataSeniorKpis = new();
    private List<SeoKpiWeightDto> DataLeadKpis = new();
    private List<SeoKpiWeightDto> Data = new();
    private ReportType SelectedReportType { get; set; } = ReportType.Keyword;
    private List<SeoPerformanceDetailDto> ReportKeywordDatas = new();
    private KeywordRankingFilter Filter { get; set; } = new();
    private List<string>? PicNames { get; set; }
    private List<UserIdentityDto>? Pics { get; set; }
    [NotNull]
    private IEnumerable<SelectedItem> PicItems => PicNames?.Select(i => new SelectedItem(i, i)).ToList() ?? new();
    private DateTimeRangeValue MonthRangeValue { get; set; } = new() { Start = DateTime.Today.AddMonths(-1), End = DateTime.Today.AddMonths(-1) };
    private SelectedItem? SelectedPic { get; set; }
    [NotNull]
    private Tab? TabRef { get; set; }
    private Tab? TabRef2 { get; set; }
    [CascadingParameter]
    [NotNull]
    private PageLayout? RootPage { get; set; }
    public string Role => RootPage?.Role ?? "";

    public int Months { get; set; } = 1;

    // Bảng xếp loại nhân viên (hardcode)
    private static readonly (decimal MinPercent, decimal MaxPercent, string Classification)[] EmployeeClassification = new[]
    {
        ((decimal)0.0, (decimal)19.99, "Kém"),
        ((decimal)20.0, (decimal)49.99, "Yếu"),
        ((decimal)50.0, (decimal)69.99, "Dưới TT"),
        ((decimal)70.0, (decimal)89.99, "TT1"),
        ((decimal)90.0, (decimal)119.99, "TT2"),
        ((decimal)120.0, (decimal)159.99, "TT3"),
        ((decimal)160.0, (decimal)189.99, "TTK1"),
        ((decimal)190.0, (decimal)219.99, "TTK2"),
        ((decimal)220.0, (decimal)249.99, "TTK3"),
        ((decimal)250.0, (decimal)279.99, "Khá 1"),
        ((decimal)280.0, (decimal)309.99, "Khá 2"),
        ((decimal)310.0, (decimal)339.99, "Khá 3"),
        ((decimal)340.0, (decimal)369.99, "KG1"),
        ((decimal)370.0, (decimal)399.99, "KG2"),
        ((decimal)400.0, (decimal)429.99, "KG3"),
        ((decimal)430.0, (decimal)459.99, "G1"),
        ((decimal)460.0, (decimal)489.99, "G2"),
        ((decimal)490.0, (decimal)519.99, "G3"),
        ((decimal)520.0, (decimal)549.99, "TG"),
        ((decimal)550.0, decimal.MaxValue, "Xuất sắc")
    };

    private string GetEmployeeClassification(decimal percentage)
    {
        foreach (var (min, max, classification) in EmployeeClassification)
        {
            if (percentage >= min && percentage <= max)
            {
                return classification;
            }
        }
        return "Không xác định";
    }

    private string GetClassificationBadgeClass(decimal percentage)
    {
        return percentage switch
        {
            <= 10 => "bg-danger",
            <= 49 => "bg-warning",
            <= 89 => "bg-info",
            <= 159 => "bg-primary",
            <= 219 => "bg-secondary",
            <= 279 => "bg-success",
            <= 369 => "bg-success",
            <= 429 => "bg-success",
            <= 489 => "bg-success",
            <= 549 => "bg-success",
            _ => "bg-success"
        };
    }
    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(LoadKPIs(), LoadPics());
    }
    
    private async Task OnViewReport()
    {
        UpdateFilter();
        if(Filter.PicNames !=null && Filter.PicNames.Count  > 0)
        {
            // Convert all PicNames to lowercase
            Filter.PicNames = Filter.PicNames.Select(name => name.ToLower()).ToList();
            
            var reportTask = KeywordRankingService.GetSeoPerformanceReportAsync(Filter);
            await Task.WhenAll(reportTask);
            var result = await reportTask;
            if (result.Status == true)
            {
                ReportKeywordDatas = result.Data.RawData;
                if(!string.IsNullOrEmpty(SelectedPic?.Value))
                {
                    string level = Pics?.FirstOrDefault(x=>x.FullName == SelectedPic.Value)?.PositionName ?? string.Empty;
                    if(!string.IsNullOrEmpty(level) && TabRef != null && TabRef.Items != null)
                    {
                        TabRef?.ActiveTab(TabRef.Items?.FirstOrDefault(x => x.Text == level) ?? TabRef.Items?.FirstOrDefault());
                        TabRef.Items.FirstOrDefault(x => x.Text == level)?.SetDisabled(false);
                        TabRef.Items.FirstOrDefault(x => x.Text == level)?.SetActive(true);

                        TabRef2?.ActiveTab(TabRef2.Items?.FirstOrDefault(x => x.Text == level) ?? TabRef2.Items?.FirstOrDefault());
                        TabRef2.Items.FirstOrDefault(x => x.Text == level)?.SetDisabled(false);
                        TabRef2.Items.FirstOrDefault(x => x.Text == level)?.SetActive(true);
                    }
                }
            }
        }
    }

    
    private void UpdateFilter()
    {
        // Filter.PicNames = SelectedPics?.Select(x => x.Text).ToList() ?? [];
        Filter.ReportType = SelectedReportType;
        Filter.PicNames = !string.IsNullOrEmpty(SelectedPic?.Value) ? [SelectedPic.Value] : [];
        Filter.FromDate = new DateTime(MonthRangeValue.Start.Year, MonthRangeValue.Start.Month, 1, 0, 0, 0);
        int dateOfMonth = DateTime.DaysInMonth(MonthRangeValue.End.Year, MonthRangeValue.End.Month);
        Filter.ToDate = new DateTime(MonthRangeValue.End.Year, MonthRangeValue.End.Month, dateOfMonth, 23, 59, 59);

        Months = MonthRangeValue.End.Month - MonthRangeValue.Start.Month + 1;
    }

    #region Select components handler


    private async Task LoadKPIs()
    {
        var result = await SeoKpiWeightService.GetListAsync();
        if (result.IsSuccess == true)
        {
            DataJuniorKpis = result.Data.Where(x => x.level == EmployeeLevel.Junior).ToList();
            DataMiddleKpis = result.Data.Where(x => x.level == EmployeeLevel.Middle).ToList();
            DataSeniorKpis = result.Data.Where(x => x.level == EmployeeLevel.Senior).ToList();
            DataLeadKpis = result.Data.Where(x => x.level == EmployeeLevel.Leader).ToList();
        }
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
            PicNames = result.Select(x => x.FullName).ToList();
            Pics = result;
            
            // If role is SEO, auto-select the first (and only) item
            if (Role == "SEO" && PicNames.Count() > 0)
            {
                SelectedPic = new SelectedItem(PicNames[0], PicNames[0]);
            }
            else
            {
                SelectedPic = null;
            }
        }else{
            Pics = null;
            PicNames = null;
            SelectedPic = null;
        }
    }

    // private async Task OnSelectedPicChanged(SelectedItem items)
    // {
    //     SelectedPics = items.ToList();
    // }

    #endregion

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
