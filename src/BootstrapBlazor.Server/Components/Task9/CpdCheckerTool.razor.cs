
namespace BootstrapBlazor.Server.Components.Task9;
public partial class CpdCheckerTool : IDisposable
{
    #region Inject
    [Inject, NotNull] private ScrapingbeeService? ScrapingbeeService { get; set; }
    [Inject, NotNull] private IFileManagerService? FileManagerService { get; set; }
    [Inject, NotNull] private IExcelService? ExcelService { get; set; }
    [Inject, NotNull] private ILinkCheckerService? LinkCheckerService { get; set; }
    [Inject, NotNull] private IScrapingbeeCheckerService? ScrapingbeeCheckerService { get; set; }
    [Inject, NotNull] private IExcelExportService? ExcelExportService { get; set; }
    [Inject, NotNull] private HttpClient? HttpClient { get; set; }
    [Inject, NotNull] private ToastService? Toast { get; set; }
    [Inject, NotNull] private ClipboardService? ClipboardService { get; set; }
    #endregion
    private CancellationTokenSource? TokenSource { get; set; }
    private CancellationTokenSource? CancellationProgress { get; set; }
    private bool IsChecking { get; set; } = false;

    private List<CpdCheckerToolRow> ExcelDataRows { get; set; } = new();
    private int CurrentProgress { get; set; } = 0;
    private int CurrentProgressPercent  { get; set; } 
    private int TotalRows { get; set; } = 0;
    private bool IsMobileMode { get; set; } = false;
    private bool IsLiveMode { get; set; } = false;
    private bool IsRedirectMode { get; set; } = false;
    private bool IsEnableRenderJS { get; set; } = true;
    private Dictionary<string, List<string>> FolderAndFilesFromMinio { get; set; } = new();


    protected override async Task OnInitializedAsync()
    {  
        base.OnInitialized();
        ExcelDataRows = new();
    }
    
    private async Task ImportExcel(UploadFile file)
    {
        if (file == null || file.File == null)
        {
            await Toast.Information("Vui lòng chọn file Excel");
            return;
        }

        try
        {
            StateHasChanged();

            using var memoryStream = new MemoryStream();
            await file.File.OpenReadStream(maxAllowedSize: 10485760).CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            ExcelDataRows = await ExcelService.ImportCpdCheckerDataAsync(memoryStream);
            await Toast.Success("Success", $"Import Excel thành công. Đã import {ExcelDataRows.Count} dòng.");
        }
        catch (Exception ex)
        {
            await Toast.Error("Error", $"Lỗi khi import Excel: {ex.Message}");
        }
        finally
        {
            StateHasChanged();
        }
    }

    private async Task CheckAllRows()
    {
        if (ExcelDataRows.Count == 0)
        {
            await Toast.Information("Vui lòng nhập dữ liệu trước khi kiểm tra");
            return;
        }

        IsChecking = true;
        TotalRows = ExcelDataRows.Count;
        CurrentProgress = 0;
        CancellationProgress = new CancellationTokenSource();
        try
        {
            var publinkCache = new Dictionary<string, string>();

            if (IsRedirectMode)
            {
                ScrapingbeeCheckerService.ClearRedirectCache();
                await Toast.Information("Đang xử lý redirect mode với logic tối ưu...");
                ExcelDataRows = await ScrapingbeeCheckerService.UpdateExcelLinkWithScrapingbeeAsync(ExcelDataRows, IsMobileMode);
                await Toast.Success("Success", "Hoàn thành xử lý redirect mode!");
                StateHasChanged();
            }

            foreach (var row in ExcelDataRows)
            {
                if (CancellationProgress.IsCancellationRequested)
                {
                    await Toast.Information("Đã hủy kiểm tra");
                    return;
                }

                try
                {
                    if (IsLiveMode)
                    {
                        if (!publinkCache.ContainsKey(row.Publink))
                        {
                            var linkLive = await ScrapingbeeCheckerService.GetLinkLiveWithScrapingbeeAsync(row,IsEnableRenderJS);
                            publinkCache[row.Publink] = linkLive;
                            row.CheckLink = linkLive;
                        }
                        else
                        {
                            row.CheckLink = publinkCache[row.Publink];
                        }
                    }

                    // Check with Scrapingbee
                    var result = await ScrapingbeeCheckerService.CheckWithScrapingbeeAsync(
                        row,  FolderAndFilesFromMinio, FileManagerService,IsMobileMode,IsLiveMode, IsRedirectMode,IsEnableRenderJS );

                    // Update row results with enhanced error information
                    row.ResultTitleStatus = result.TitleStatus;
                    row.ResultAltStatus = result.AltStatus;
                    row.ResultShortlink = result.OriginalUrlFromShortLink;
                    row.ResultShortlinkStatus = result.ShortlinkStatus;
                    row.BannerCheckNote = result.BannerCheckNote;
                    
                    // Log specific failure types for debugging
                    if (result.ShortlinkStatus.Contains("FAILED"))
                    {
                        await Toast.Error("Lỗi", $"Failed check for {row.ShortLink}: {result.ShortlinkStatus}");
                    }
                }
                catch (Exception ex)
                {
                    row.ResultShortlinkStatus = $"FAILED - Exception: {ex.GetType().Name}";
                }

                CurrentProgress++;
                CurrentProgressPercent = TotalRows > 0 ? (int)(((double)CurrentProgress / TotalRows) * 100) : 0;
                // Update monitoring status every 5 rows
                if (CurrentProgress % 5 == 0)
                {
                    await Toast.Information($"Đang kiểm tra {CurrentProgress}/{TotalRows}");
                }
                
                StateHasChanged();
            }
            
            // Final monitoring report
            var successCount = ExcelDataRows.Count(x => x.ResultShortlinkStatus == "OK");
            var failureCount = ExcelDataRows.Count(x => x.ResultShortlinkStatus.Contains("FAILED"));
            await Toast.Information($"Hoàn thành! Thành công: {successCount}, Thất bại: {failureCount}");
        }
        finally
        {
            IsChecking = false;
            StateHasChanged();
        }
    }

    private async Task CopyData()
    {
        string titles ="STT\tPIC\tLink Pub\tBrand\tVị trí\tLink Banner\tShort link\tPub shortlink\tBanner Folder\tFilename Banner\tXPATH\tDimension\tTitle\tAlt\tShortlink Check\tStatus\tAlt Status\tTitle Status\tBanner check note\t";
        var data = ExcelDataRows.Select(row => 
            string.Join("\t", row.No, row.Pic, row.Publink, row.Brand, row.Position, row.OriginalLink, row.ShortLink, row.RedirectShortLink, row.FolderBanner, row.FileNameBanner, row.DeviceCheck, row.Dimension, row.Title, row.Alt, row.ResultShortlink, row.ResultShortlinkStatus, row.ResultAltStatus, row.ResultTitleStatus, row.BannerCheckNote)
        ).ToList();
        data.Insert(0, titles);
        await ClipboardService.Copy(string.Join("\n", data));
        await Toast.Success("Success", "Đã copy dữ liệu vào clipboard");
    }

    private async Task<bool>  ClearAll(UploadFile? item)
    {
        ExcelDataRows.Clear();
        CancellationProgress?.Cancel();
        CurrentProgress = 0;
        TotalRows = 0;
        IsChecking = false;
        if(item != null)
            await Toast.Success("Xoá dữ liệu", $"Xoá dữ liệu {item.FileName} thành công");
        
        StateHasChanged();
        return true;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && TokenSource != null)
        {
            CancellationProgress?.Cancel();
            CancellationProgress?.Dispose();
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
