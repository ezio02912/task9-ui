using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BootstrapBlazor.Server.Components.Task9.KeywordManager;

public partial class CreateKeyword : ComponentBase
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
    
    private CreateUpdateKeywordDto Model { get; set; } = new CreateUpdateKeywordDto();
    
    private bool IsLoading { get; set; } = false;
    
    private List<SelectedItem> BrandItems { get; set; } = new();
    
    protected override async Task OnInitializedAsync()
    {
        await LoadBrands();
        InitializeModel();
    }
    
    private void InitializeModel()
    {
        Model = new CreateUpdateKeywordDto
        {
            IsActive = true
        };
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
    
    private async Task OnValidSubmit(EditContext context)
    {
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
            
            var result = await KeywordService.CreateAsync(Model);
            
            if (result.Status && result.Data != null)
            {
                await ToastService.Success("Thành công", "Tạo keyword mới thành công!");
                
                // Reset form
                InitializeModel();
                StateHasChanged();
                
                // Navigate back to list after 1 second
                await Task.Delay(1000);
                NavigationManager.NavigateTo("/keyword-manager");
            }
            else
            {
                await ToastService.Error("Lỗi", result.Message ?? "Không thể tạo keyword. Vui lòng kiểm tra lại thông tin.");
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

