
namespace BootstrapBlazor.Server.Components.Task9;
public partial class FacebookGroup : IDisposable
{
    #region Inject
    [Inject]
    private IFacebookGroupService FacebookGroupService { get; set; }
    [Inject]
    [NotNull]
    private ToastService? Toast { get; set; }
    #endregion
    private CancellationTokenSource? TokenSource { get; set; }
    private bool IsLoading { get; set; } = false;
    private List<FacebookGroupDto> Datas { get; set; } = new();
    private int TotalItems {set;get;} = 0;
    private string SearchText {set;get;} = "";
    private static IEnumerable<int> PageItemsSource => new int[]
    {
        10,
        20,
        40,
        80,
        100
    };

    private Table<FacebookGroupDto>? TableRef { get; set; }

    protected override async Task OnInitializedAsync()
    {  
        base.OnInitialized();
        var result = await FacebookGroupService.GetListAsync(new BaseFilterPagingDto(){
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





    private async Task<bool> OnSaveAsync(FacebookGroupDto item, ItemChangedType changedType)
    {
        if (changedType == ItemChangedType.Add)
        {
            var result = await FacebookGroupService.CreateAsync(item);
            if (result.IsSuccess)
            {             
                await Toast.Success("Success", "Thêm mới thành công!");
                await TableRef!.QueryAsync();
                return true;
            }
        }
        else
        {
            var oldItem = Datas.FirstOrDefault(i => i.Id == item.Id);
            if (oldItem != null)
            {
                oldItem.Name = item.Name;
                oldItem.GroupUrl = item.GroupUrl;
                var result = await FacebookGroupService.UpdateAsync(item.Id, item);
                if (result.IsSuccess)
                {             
                    await Toast.Success("Success", "Cập nhật thành công!");
                    await TableRef!.QueryAsync();
                    return true;
                }
            }
        }
        return true;
    }

    private async Task<bool> OnDeleteAsync(IEnumerable<FacebookGroupDto> items)
    {
        await Task.WhenAll(items.Select(async i => {
            var result = await FacebookGroupService.DeleteAsync(i.Id);
            if (result.IsSuccess)
            {
                Datas.Remove(i);
            }
        }));
        return true;
    }

     private async Task<QueryData<FacebookGroupDto>> OnQueryAsync(QueryPageOptions options)
    {
        await Task.Delay(200);

        var result = await FacebookGroupService.GetListAsync(new BaseFilterPagingDto(){
            Skip = (options.PageIndex - 1) * options.PageItems,
            Take = options.PageItems,
            FilterText = options.SearchText
        });
        if (result.IsSuccess)
        {
            Datas = result.Data;
            TotalItems = result.TotalItems ?? 0;
        }
        return new QueryData<FacebookGroupDto>()
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
