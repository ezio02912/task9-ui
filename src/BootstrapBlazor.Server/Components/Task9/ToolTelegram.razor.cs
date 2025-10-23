
namespace BootstrapBlazor.Server.Components.Task9;
public partial class ToolTelegram : IDisposable
{
    #region Inject
    [Inject]
    private ITelegramContentGroupService TelegramContentGroupService { get; set; }
    [Inject]
    [NotNull]
    private ToastService? Toast { get; set; }
    #endregion
    private CancellationTokenSource? TokenSource { get; set; }
    private bool IsLoading { get; set; } = false;
    private List<TelegramContentGroupDto> Datas { get; set; } = new();
    private int TotalItems {set;get;} = 0;
    private string SearchText {set;get;} = "";
    private static IEnumerable<int> PageItemsSource => new int[]
    {
        5,
        10,
        20,
        40,
        80,
        100
    };

    protected override async Task OnInitializedAsync()
    {  
        base.OnInitialized();
        var result = await TelegramContentGroupService.GetListAsync(new BaseFilterPagingDto(){
            Skip = 0,
            Take = 5,
            FilterText = SearchText,
        });
        if (result.IsSuccess)
        {
            Datas = result.Data;
            TotalItems = result.Data.Count;
        }
    }
     private async Task<QueryData<TelegramContentGroupDto>> OnQueryAsync(QueryPageOptions options)
    {
        await Task.Delay(200);

        var result = await TelegramContentGroupService.GetListAsync(new BaseFilterPagingDto(){
            Skip = (options.PageIndex - 1) * options.PageItems,
            Take = options.PageItems,
            FilterText = options.SearchText
        });
        if (result.IsSuccess)
        {
            Datas = result.Data;
            TotalItems = result.TotalItems ?? 0;
        }
        return new QueryData<TelegramContentGroupDto>()
        {
            Items = Datas,
            TotalCount = TotalItems
        };
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
