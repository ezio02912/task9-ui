using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BootstrapBlazor.Server.Components.Task9.KeywordManager;

public partial class EditKeyword : ComponentBase
{
    [Parameter]
    public long Id { get; set; }
    
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
    
    private CreateUpdateKeywordDto? Model { get; set; }
    
    private bool IsLoading { get; set; } = false;
    private bool IsLoadingData { get; set; } = false;
    
    private List<SelectedItem> BrandItems { get; set; } = new();
    
    private string KeywordId => Id.ToString();
    
    protected override async Task OnInitializedAsync()
    {
        await LoadBrands();
        await LoadKeyword();
    }
    
    private async Task LoadBrands()
    {
        try
        {
            var result = await BrandService.GetListAsync();
            if (result.Status)
            {
                BrandItems = result.Data?.Select(b => new SelectedItem(b.Id.ToString(), b.Name)).ToList() ?? new();
            }
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải danh sách brand: {ex.Message}");
        }
    }
    
    private async Task LoadKeyword()
    {
        try
        {
            IsLoadingData = true;
            var result = await KeywordService.GetByIdAsync(Id);
            
            if (result.Status && result.Data != null)
            {
                Model = new CreateUpdateKeywordDto
                {
                    BrandId = result.Data.BrandId,
                    KeywordText = result.Data.KeywordText,
                    IsActive = result.Data.IsActive
                };
            }
            else
            {
                await ToastService.Error("Lỗi", "Không tìm thấy keyword");
                Model = null;
            }
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải thông tin keyword: {ex.Message}");
            Model = null;
        }
        finally
        {
            IsLoadingData = false;
        }
    }
    
    private async Task OnValidSubmit(EditContext context)
    {
        if (Model == null) return;
        
        IsLoading = true;
        try
        {
            // Validate required fields
            if (Model.BrandId <= 0)
            {
                await ToastService.Error("Lỗi", "Vui lòng chọn brand");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(Model.KeywordText))
            {
                await ToastService.Error("Lỗi", "Vui lòng nhập keyword text");
                return;
            }
            
            var result = await KeywordService.UpdateAsync(Id, Model);
            
            if (result.Status && result.Data != null)
            {
                await ToastService.Success("Thành công", "Cập nhật keyword thành công!");
                
                // Navigate back to list after 1 second
                await Task.Delay(1000);
                NavigationManager.NavigateTo("/keyword-manager");
            }
            else
            {
                await ToastService.Error("Lỗi", result.Message ?? "Không thể cập nhật keyword. Vui lòng kiểm tra lại thông tin.");
            }
        }
        catch (BootstrapBlazor.Server.Exceptions.BadRequestException ex)
        {
            await ToastService.Error("Lỗi", ex.Message);
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Có lỗi xảy ra: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private void OnCancel()
    {
        NavigationManager.NavigateTo("/keyword-manager");
    }
}

