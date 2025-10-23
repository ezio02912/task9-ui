
namespace BootstrapBlazor.Server.Components.Task9;
public partial class FacebookComment : IDisposable
{
    #region Inject
    [Inject]
    private IFacebookCommentService FacebookCommentService { get; set; }
    [Inject, NotNull]
    private DownloadService? DownloadService { get; set; }

    [Inject]
    [NotNull]
    private ToastService? Toast { get; set; }
    #endregion
    private CancellationTokenSource? TokenSource { get; set; }
    private bool IsLoading { get; set; } = false;
    private List<FacebookCommentDto> Datas { get; set; } = new();
    private int TotalItems {set;get;} = 0;
    private string SearchText {set;get;} = "";
    
    private DateTimeRangeValue MonthRangeValue { get; set; } = new() { Start = DateTime.Today.AddDays(-1), End = DateTime.Today };
    private static IEnumerable<int> PageItemsSource => new int[]
    {
        10,
        20,
        40,
        80,
        100
    };

    private Table<FacebookCommentDto>? TableRef { get; set; }

    protected override async Task OnInitializedAsync()
    {  
        base.OnInitialized();
        var result = await FacebookCommentService.GetListAsync(new BaseFilterPagingDto(){
            Skip = 0,
            Take = 10,
            FilterText = SearchText,
        });
        if (result.IsSuccess)
        {
            Datas = result.Data;
            TotalItems = result.Data.Count;
        }
    }
    private async Task TriggerSearch(DateTimeRangeValue val, int index)
    {
        await TableRef?.QueryAsync();
    }
    private async Task<bool> OnSaveAsync(FacebookCommentDto item, ItemChangedType changedType)
    {
        if (changedType == ItemChangedType.Add)
        {
            return true;
        }
        else
        {
            var oldItem = Datas.FirstOrDefault(i => i.id == item.id);
            if (oldItem != null)
            {
                await Toast.Success("Success", "Cập nhật thành công!");
                await TableRef!.QueryAsync();
                return true;
            }
        }
        return true;
    }

    private async Task<bool> OnDeleteAsync(IEnumerable<FacebookCommentDto> items)
    {
        await Task.WhenAll(items.Select(async i => {
            var result = await FacebookCommentService.DeleteAsync(i.id);
            if (result.IsSuccess)
            {
                Datas.Remove(i);
            }
        }));
        return true;
    }

     private async Task<QueryData<FacebookCommentDto>> OnQueryAsync(QueryPageOptions options)
    {
        await Task.Delay(200);

        var result = await FacebookCommentService.GetListAsync(new BaseFilterPagingDto(){
            Skip = (options.PageIndex - 1) * options.PageItems,
            Take = options.PageItems,
            FilterText = options.SearchText,
            FromDate = new DateTime(MonthRangeValue.Start.Year, MonthRangeValue.Start.Month,  MonthRangeValue.Start.Day , 0, 0, 0),
            ToDate = new DateTime(MonthRangeValue.End.Year, MonthRangeValue.End.Month, MonthRangeValue.End.Day, 23, 59, 59)
        });
        if (result.IsSuccess)
        {
            Datas = result.Data;
            TotalItems = result.TotalItems ?? 0;
        }
        return new QueryData<FacebookCommentDto>()
        {
            Items = Datas,
            TotalCount = TotalItems
        };
    }

    private async Task OnExportExcel()
    {
        try
        {
            await Toast.Information("Đang xử lý dữ liệu xuất excel...");

            var excelData = await FacebookCommentService.ExportExcelAsync(new BaseFilterPagingDto(){
                Skip = 0,
                Take = int.MaxValue,
                FilterText = SearchText,
                FromDate = new DateTime(MonthRangeValue.Start.Year, MonthRangeValue.Start.Month,  MonthRangeValue.Start.Day , 0, 0, 0),
                ToDate = new DateTime(MonthRangeValue.End.Year, MonthRangeValue.End.Month, MonthRangeValue.End.Day, 23, 59, 59)
            });

            if (excelData != null && excelData.Length > 0)
            {
                await DownloadService.DownloadFromByteArrayAsync($"Facebook_Comment_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx", excelData);
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
