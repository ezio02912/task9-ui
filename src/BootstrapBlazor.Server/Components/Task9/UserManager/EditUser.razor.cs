using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BootstrapBlazor.Server.Components.Task9.UserManager;

public partial class EditUser : ComponentBase
{
    [Parameter]
    public int UserId { get; set; }
    
    [Inject]
    [NotNull]
    private IUserManagerService? UserManagerService { get; set; }
    
    [Inject]
    [NotNull]
    private IRoleManagerService? RoleManagerService { get; set; }
    
    [Inject]
    [NotNull]
    private IPositionService? PositionService { get; set; }
    
    [Inject]
    [NotNull]
    private ToastService? ToastService { get; set; }
    
    [Inject]
    [NotNull]
    private NavigationManager? NavigationManager { get; set; }
    
    private UpdateUserDto? Model { get; set; }
    private bool IsLoading { get; set; } = false;
    private bool IsLoadingData { get; set; } = false;
    
    private List<SelectedItem> RoleItems { get; set; } = new();
    private List<SelectedItem> PositionItems { get; set; } = new();
    
    private List<string> SelectedRoles { get; set; } = new();
    private int? SelectedPositionId { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }
    
    private async Task LoadData()
    {
        IsLoadingData = true;
        try
        {
            await LoadRoles();
            await LoadPositions();
            await LoadUser();
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải dữ liệu: {ex.Message}");
        }
        finally
        {
            IsLoadingData = false;
        }
    }
    
    private async Task LoadUser()
    {
        try
        {
            var userWithNav = await UserManagerService.GetWithNavigationProperties(UserId);
            
            if (userWithNav == null || userWithNav.User == null)
            {
                await ToastService.Error("Lỗi", "Không tìm thấy thông tin người dùng");
                return;
            }
            
            Model = new UpdateUserDto
            {
                UserName = userWithNav.User.UserName,
                UserCode = userWithNav.User.UserCode,
                FirstName = userWithNav.User.FirstName,
                LastName = userWithNav.User.LastName,
                Email = userWithNav.User.Email ?? "",
                PhoneNumber = userWithNav.User.PhoneNumber,
                Gender = (int)userWithNav.User.Gender, // Cast enum to int
                DOB = userWithNav.User.DOB,
                IsActive = userWithNav.User.IsActive,
                PositionId = userWithNav.Position?.Id,
                Address = userWithNav.User.Address,
                Roles = userWithNav.RoleNames,
                IsSetPassword = false
            };
            
            SelectedRoles = userWithNav.RoleNames;
            SelectedPositionId = userWithNav.Position?.Id;
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải thông tin người dùng: {ex.Message}");
        }
    }
    
    private async Task LoadRoles()
    {
        try
        {
            var roles = await RoleManagerService.GetListAsync();
            RoleItems = roles.Select(r => new SelectedItem(r.Name, r.Name)).ToList();
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải danh sách vai trò: {ex.Message}");
        }
    }
    
    private async Task LoadPositions()
    {
        try
        {
            var positions = await PositionService.GetListAsync();
            PositionItems = positions.Select(p => new SelectedItem(p.Id.ToString(), p.Name)).ToList();
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải danh sách chức vụ: {ex.Message}");
        }
    }
    
    private async Task OnValidSubmit(EditContext context)
    {
        if (Model == null) return;
        
        IsLoading = true;
        try
        {
            // Map selected roles and position to model
            Model.Roles = SelectedRoles;
            Model.PositionId = SelectedPositionId;
            
            var result = await UserManagerService.UpdateUserWithNavigationPropertiesAsync(Model, UserId);
            
            if (result != null)
            {
                await ToastService.Success("Thành công", "Cập nhật người dùng thành công!");
                
                // Navigate back to list
                NavigationManager.NavigateTo("/user-manager");
            }
            else
            {
                await ToastService.Error("Lỗi", "Không thể cập nhật người dùng. Vui lòng kiểm tra lại thông tin.");
            }
        }
        catch (BootstrapBlazor.Server.Exceptions.BadRequestException ex)
        {
            await ToastService.Error("Lỗi Validation", ex.Message);
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
        NavigationManager.NavigateTo("/user-manager");
    }
    
    private void OnBackToList()
    {
        NavigationManager.NavigateTo("/user-manager");
    }
}

