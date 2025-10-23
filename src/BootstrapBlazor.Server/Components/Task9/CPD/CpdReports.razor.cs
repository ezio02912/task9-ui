using BootstrapBlazor.Components;
using Microsoft.JSInterop;
using BootstrapBlazor.Server.Components.Task9.CPD;

namespace BootstrapBlazor.Server.Components.Task9.CPD;

/// <summary>
/// Component for displaying CPD reports and analytics
/// </summary>
public partial class CpdReports : ComponentBase, IDisposable
{
    #region Inject
    [Inject]
    [NotNull]
    private ToastService? Toast { get; set; }
    
    [Inject]
    [NotNull]
    private BootstrapBlazor.Server.Services.CPD.ICpdService? CpdService { get; set; }
    
    [Inject]
    [NotNull]
    private IJSRuntime? JSRuntime { get; set; }
    #endregion

    #region Properties
    private DateTimeRangeValue DateRange { get; set; } = new()
    {
        Start = DateTime.Today.AddDays(-30),
        End = DateTime.Today
    };
    private bool IsLoading { get; set; } = false;
    private CpdReportResponse? ReportData { get; set; }
    private List<CpdChangeTypeItem> ChangeTypeItems { get; set; } = new();
    private List<CpdPicItem> PicItems { get; set; } = new();
    #endregion

    /// <summary>
    /// Initialize component
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    private async Task LoadReportData()
    {
        if (DateRange.Start > DateRange.End)
        {
            await Toast!.Error("Lỗi", "Ngày bắt đầu không được lớn hơn ngày kết thúc");
            return;
        }

        IsLoading = true;
        StateHasChanged();

        try
        {
            var result = await CpdService!.GetReportAsync(
                DateOnly.FromDateTime(DateRange.Start), 
                DateOnly.FromDateTime(DateRange.End)
            );
            
            if (result.IsSuccess && result.Data != null)
            {
                ReportData = result.Data;
                
                ConvertDataForTables();
                await RenderCharts();
                await Toast!.Success("Thành công", "Đã tải báo cáo thành công");
            }
            else
            {
                await Toast!.Error("Lỗi", $"Không thể tải báo cáo: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            await Toast!.Error("Lỗi", $"Có lỗi xảy ra: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private void ConvertDataForTables()
    {
        if (ReportData == null) return;

        // Use data directly from CpdReportResponse
        ChangeTypeItems = ReportData.ChangeTypeDetails
            .Select(x => new CpdChangeTypeItem
            {
                ChangeType = x.ChangeType,
                Count = x.Quantity,
                Percentage = x.Percentage
            })
            .ToList();

        PicItems = ReportData.PicDetails
            .Select(x => new CpdPicItem
            {
                Pic = x.Pic,
                Count = x.ChangeCount,
                Percentage = x.Percentage
            })
            .ToList();
    }

    private async Task RenderCharts()
    {
        if (ReportData == null) return;

        try
        {
            // Convert to dictionary format for JavaScript
            var changesByTypeDict = ReportData.ChangesByType.ToDictionary(x => x.ChangeType, x => x.Count);
            var changesByPicDict = ReportData.ChangesByPic.ToDictionary(x => x.Pic, x => x.Count);
            
            await JSRuntime!.InvokeVoidAsync("renderCpdCharts", 
                changesByTypeDict, 
                changesByPicDict);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error rendering charts: {ex.Message}");
        }
    }

    private int GetNewItemsCount()
    {
        return ReportData?.SummaryStats?.NewItems ?? 0;
    }

    private int GetRemovedItemsCount()
    {
        return ReportData?.SummaryStats?.DeletedItems ?? 0;
    }

    private int GetTotalItemsCount()
    {
        return ReportData?.SummaryStats?.TotalItems ?? 0;
    }

    private int GetTotalChangesCount()
    {
        return ReportData?.SummaryStats?.TotalChanges ?? 0;
    }

    private Color GetChangeTypeColor(string changeType)
    {
        return changeType.ToUpperInvariant() switch
        {
            "NEW" => Color.Success,
            "CHANGED" => Color.Warning,
            "REMOVED" => Color.Danger,
            _ => Color.Secondary
        };
    }

    private Color GetChangeTypeProgressColor(string changeType)
    {
        return changeType.ToUpperInvariant() switch
        {
            "NEW" => Color.Success,      // Xanh lá (mới)
            "CHANGED" => Color.Warning,  // Vàng (thay đổi)
            "REMOVED" => Color.Danger,   // Đỏ (xóa)
            _ => Color.Secondary
        };
    }

    private string GetChangeTypeText(string changeType)
    {
        return changeType.ToUpperInvariant() switch
        {
            "NEW" => "Mới",
            "CHANGED" => "Thay đổi",
            "REMOVED" => "Bị xóa",
            _ => changeType
        };
    }

    /// <summary>
    /// Dispose component resources
    /// </summary>
    public void Dispose()
    {
        // Cleanup if needed
    }
}
