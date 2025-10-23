
using BootstrapBlazor.Server.Identity;
using Microsoft.AspNetCore.Components.Authorization;

namespace BootstrapBlazor.Server.Components.Task9;
public partial class BaoCaoSEOChiTietBOD : IDisposable
{
    #region Inject
    [Inject]
    private IKeywordRankingService KeywordRankingService { get; set; }
    [Inject]
    private IUserManagerService UserManagerService { get; set; }
    [Inject]
    private AuthenticationStateProvider AuthStateProvider { get; set; }

    [Inject]
    private IPicDomainAssignmentService PicDomainAssignmentService { get; set; }

    [Inject]
    [NotNull]
    private ToastService? Toast { get; set; }
    [Inject, NotNull]
    private DownloadService? DownloadService { get; set; }

    #endregion
    private DateTimeRangeValue MonthRangeValue { get; set; } = new() { Start = DateTime.Today.AddMonths(-1), End = DateTime.Today };
    private CancellationTokenSource? TokenSource { get; set; }
    private KeywordRankingFilter Filter { get; set; } = new();
    private List<string>? Pics { get; set; }
    private bool IsLoading { get; set; } = false;
    private IEnumerable<SelectedItem> PicItems => Pics?.Select(i => new SelectedItem(i, i)).ToList() ?? new();
    private SelectedItem? SelectedPic { get; set; }
    private SeoStatRow Data = new();
    
    [CascadingParameter]
    [NotNull]
    private PageLayout? RootPage { get; set; }
    public string Role => RootPage?.Role ?? "";

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(LoadPics());
    }
    private async Task OnViewReport()
    {
        try
        {
            IsLoading = true;
            StateHasChanged();
            UpdateFilter();
            var reportTask = KeywordRankingService.GetReportKeywordRankingDetailBODAsync(Filter);
            await Task.WhenAll(reportTask);
            var result = await reportTask;
            if (result.Status)
            {
                Data = result.Data;
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
        Filter.PicName = SelectedPic?.Value ?? string.Empty;
        Filter.FromDate = new DateTime(MonthRangeValue.Start.Year, MonthRangeValue.Start.Month, 1, 0, 0, 0);
        int dateOfMonth = DateTime.DaysInMonth(MonthRangeValue.End.Year, MonthRangeValue.End.Month);
        Filter.ToDate = new DateTime(MonthRangeValue.End.Year, MonthRangeValue.End.Month, dateOfMonth, 23, 59, 59);
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
    private async Task OnExportExcel()
    {
        try
        {
            await Toast.Information("Đang xử lý dữ liệu xuất excel...");

            var excelData = await KeywordRankingService.ExportExcelReportRankingDetailAsync(Filter);

            if (excelData != null && excelData.Length > 0)
            {
                await DownloadService.DownloadFromByteArrayAsync($"SEO_Performance_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx", excelData);
                await Toast.Success("Đã xuất Excel thành công!");
            }
            else
            {
                await Toast.Error("Lỗi","Không có dữ liệu để xuất Excel");
            }
        }
        catch (Exception ex)
        {
            await Toast.Error("Lỗi","Lỗi khi xuất Excel: " + ex.Message);
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
