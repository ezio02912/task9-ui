using Microsoft.AspNetCore.Components;

namespace BootstrapBlazor.Server.Components.Task9.KeywordManager;

public partial class KeywordList : ComponentBase
{
    [Inject]
    [NotNull]
    private IKeywordService? KeywordService { get; set; }
    
    [Inject]
    [NotNull]
    private IBrandService? BrandService { get; set; }
    
    [Inject]
    [NotNull]
    private ToastService? ToastService { get; set; }
    
    [Inject]
    [NotNull]
    private NavigationManager? NavigationManager { get; set; }
    
    [Inject]
    [NotNull]
    private DialogService? DialogService { get; set; }
    
    private List<KeywordDto> Keywords { get; set; } = new();
    private List<BrandDto> Brands { get; set; } = new();
    private int TotalItems { get; set; } = 0;
    private bool IsLoading { get; set; } = false;
    
    private static IEnumerable<int> PageItemsSource => new int[] { 10, 20, 40, 80, 100 };
    
    private Table<KeywordDto>? TableRef { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            IsLoading = true;
            await LoadBrands();
            await LoadKeywords();
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải dữ liệu: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task LoadBrands()
    {
        try
        {
            var result = await BrandService.GetListAsync();
            if (result.Status)
            {
                Brands = result.Data ?? new();
            }
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải danh sách brand: {ex.Message}");
        }
    }
    
    private async Task LoadKeywords()
    {
        try
        {
            var filter = new KeywordFilter
            {
                Skip = 0,
                Take = 10
            };
            
            var result = await KeywordService.GetListSimpleAsync(filter);
            if (result.Status)
            {
                Keywords = result.Data ?? new();
                TotalItems = result.Total;
            }
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải danh sách keyword: {ex.Message}");
        }
    }
    
    private async Task<QueryData<KeywordDto>> OnQueryAsync(QueryPageOptions options)
    {
        try
        {
            IsLoading = true;
            
            var filter = new KeywordFilter
            {
                Skip = (options.PageIndex - 1) * options.PageItems,
                Take = options.PageItems,
                KeywordText = options.SearchText
            };
            
            var result = await KeywordService.GetListSimpleAsync(filter);
            
            if (result.Status)
            {
                Keywords = result.Data ?? new();
                TotalItems = result.Total;
                
                return new QueryData<KeywordDto>()
                {
                    Items = Keywords,
                    TotalCount = TotalItems
                };
            }
            else
            {
                await ToastService.Error("Lỗi", result.Message ?? "Không thể tải danh sách keyword");
                return new QueryData<KeywordDto>()
                {
                    Items = new List<KeywordDto>(),
                    TotalCount = 0
                };
            }
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải danh sách keyword: {ex.Message}");
            return new QueryData<KeywordDto>()
            {
                Items = new List<KeywordDto>(),
                TotalCount = 0
            };
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private void OnCreateClick()
    {
        NavigationManager.NavigateTo("/keyword-manager/create");
    }
    
    private void OnEditClick(long id)
    {
        NavigationManager.NavigateTo($"/keyword-manager/edit/{id}");
    }
    
    private async Task OnDeleteClick(KeywordDto keyword)
    {
        var confirmed = await DialogService!.ShowModal("Xác nhận xóa", 
            $"Bạn có chắc chắn muốn xóa keyword '{keyword.KeywordText}'?",
            new ResultDialogOption()
            {
                Size = Size.Small
            });

        if (confirmed == DialogResult.Yes)
        {
            try
            {
                IsLoading = true;
                var result = await KeywordService.DeleteAsync(keyword.Id);
                
                if (result.Status && result.Data)
                {
                    await ToastService.Success("Thành công", "Xóa keyword thành công!");
                    await TableRef!.QueryAsync();
                }
                else
                {
                    await ToastService.Error("Lỗi", result.Message ?? "Không thể xóa keyword");
                }
            }
            catch (Exception ex)
            {
                await ToastService.Error("Lỗi", $"Không thể xóa keyword: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
    
    private string GetBrandName(int brandId)
    {
        var brand = Brands.FirstOrDefault(b => b.Id == brandId);
        return brand?.Name ?? "N/A";
    }
}

