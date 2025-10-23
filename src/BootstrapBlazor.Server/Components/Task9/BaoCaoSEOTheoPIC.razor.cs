
using BootstrapBlazor.Server.Identity;
using Microsoft.AspNetCore.Components.Authorization;

namespace BootstrapBlazor.Server.Components.Task9;
public partial class BaoCaoSEOTheoPIC : IDisposable
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
    private IKeywordRankingService KeywordRankingService { get; set; }

    [Inject]
    [NotNull]
    private ToastService? Toast { get; set; }

    #endregion
    private bool ViewDetail { get; set; } = false;
    private DateTime YearValue { get; set; } = DateTime.Today;
    private bool IsLoading { get; set; }  = false;
    private CancellationTokenSource? TokenSource { get; set; }
    private ReportType SelectedReportType { get; set; } = ReportType.Keyword;
    private List<SeoPerformanceDetailDto> Data = new();
    private List<SeoPerformanceDetailByResultDto> DataDetail = new();
    private KeywordRankingFilter Filter { get; set; } = new();
    private List<string>? Pics { get; set; }
    private KeywordRankingReportResponseDto ChartData = new();
    private List<string>? Mainsites { get; set; }
    [NotNull]
    private List<SelectedItem<BrandDto>>? Brands { get; set; }
    private List<SelectedItem<GroupDto>>? Groups { get; set; }
    private List<GroupDto>? SelectedGroups { get; set; } = null;
    private List<BrandDto>? SelectedBrands { get; set; } = null;
    private IEnumerable<SelectedItem> PicItems => Pics?.Select(i => new SelectedItem(i, i)).ToList() ?? new();
    private DateTimeRangeValue MonthRangeValue { get; set; } = new() { Start = DateTime.Today.AddMonths(-1), End = DateTime.Today };

    private List<SelectedItem>? SelectedPics { get; set; }
    
    [CascadingParameter]
    [NotNull]
    private PageLayout? RootPage { get; set; }
    public string Role => RootPage?.Role ?? "";

    [NotNull]
    private Chart BarChart { get; set; } = new Chart();

    // Key để force re-render Chart component
    private string ChartKey { get; set; } = Guid.NewGuid().ToString();

    // Flag để kiểm soát việc hiển thị Chart
    private bool ShowChart { get; set; } = true;
    private bool ShowDetail { get; set; } = false;


    private KeywordSumarySeoReportDto SummaryData = new();

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(LoadBrands(), LoadPics());
    }
   private async Task OnViewReport()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            StateHasChanged();
            UpdateFilter();
            // Load data in parallel
            var reportTask = KeywordRankingService.GetSeoPerformanceReportAsync(Filter);
            await Task.WhenAll(reportTask);
            var result = await reportTask;
            if (result.Status == true)
            {
                Data = result.Data.RawData;
                DataDetail = result.Data.RawDataByKeywordOrDomain;
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
        Filter.BrandIds = SelectedBrands?.Select(x => x.Id).ToList() ?? [];
        Filter.Brands = SelectedBrands?.Select(x => x.Name).ToList() ?? [];
        Filter.PicNames = SelectedPics?.Select(x => x.Text).ToList() ?? [];
        Filter.ReportType = SelectedReportType;
        Filter.FromDate = new DateTime(MonthRangeValue.Start.Year, MonthRangeValue.Start.Month, 1, 0, 0, 0);
        int dateOfMonth = DateTime.DaysInMonth(MonthRangeValue.End.Year, MonthRangeValue.End.Month);
        Filter.ToDate = new DateTime(MonthRangeValue.End.Year, MonthRangeValue.End.Month, dateOfMonth, 23, 59, 59);
    }
    #region Select components handler


    private async Task LoadBrands()
    {
        var result = await BrandService.GetListAsync();
        if (result.Status == true)
        {
            Brands = [.. result.Data.Select(i => new SelectedItem<BrandDto>(i, i.Name!))];
            SelectedBrands = null;
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
            Pics = result.Select(x => x.FullName).ToList();
            
            // If role is SEO, auto-select the first (and only) item
            if (Role == "SEO" && Pics.Count() > 0)
            {
                SelectedPics = new List<SelectedItem> { new SelectedItem(Pics[0], Pics[0]) };
            }
            else
            {
                SelectedPics = null;
            }
        }
        else
        {
            Pics = null;
            SelectedPics = null;
        }
    }

    private async Task OnSelectedPicsChanged(IEnumerable<SelectedItem> items)
    {
        SelectedPics = items.ToList();
        await OnViewReport();
        StateHasChanged();
    }

    private Task OnViewDetailChanged(bool val)
    {
        ViewDetail = val;
        StateHasChanged();
        return Task.CompletedTask;
    }
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
