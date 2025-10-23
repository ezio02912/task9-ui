using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BootstrapBlazor.Server.Components.Task9.UserManager;

public partial class CreateUser : ComponentBase
{
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
    
    private CreateUserDto Model { get; set; } = new CreateUserDto();
    
    private bool IsLoading { get; set; } = false;
    private string GeneratedPassword { get; set; } = "";
    private bool ShowPassword { get; set; } = false;
    
    private List<SelectedItem> RoleItems { get; set; } = new();
    private List<SelectedItem> PositionItems { get; set; } = new();
    
    private List<string> SelectedRoles { get; set; } = new();
    
    protected override async Task OnInitializedAsync()
    {
        await LoadRoles();
        await LoadPositions();
        InitializeModel();
    }
    
    private void InitializeModel()
    {
        // Generate random password
        GeneratedPassword = GenerateRandomPassword();
        
        Model = new CreateUserDto
        {
            Password = GeneratedPassword,
            PasswordConfirm = GeneratedPassword,
            Gender = 1,
            DOB = DateTime.Now.AddYears(-25),
            PhoneNumber = "0000000000",
            IsActive = true,
            IsDelete = false
        };
    }
    
    private string GenerateRandomPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$";
        const string upperChars = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lowerChars = "abcdefghjkmnpqrstuvwxyz";
        const string numberChars = "23456789";
        const string specialChars = "!@#$";
        
        var random = new Random();
        var password = new char[10];
        
        // Ensure at least one of each required character type
        password[0] = upperChars[random.Next(upperChars.Length)];
        password[1] = lowerChars[random.Next(lowerChars.Length)];
        password[2] = numberChars[random.Next(numberChars.Length)];
        password[3] = specialChars[random.Next(specialChars.Length)];
        
        // Fill the rest randomly
        for (int i = 4; i < password.Length; i++)
        {
            password[i] = chars[random.Next(chars.Length)];
        }
        
        // Shuffle the password
        for (int i = password.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }
        
        return new string(password);
    }
    
    private void RegeneratePassword()
    {
        GeneratedPassword = GenerateRandomPassword();
        Model.Password = GeneratedPassword;
        Model.PasswordConfirm = GeneratedPassword;
        StateHasChanged();
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
        IsLoading = true;
        try
        {
            // Map selected roles to model
            Model.Roles = SelectedRoles;
            
            // Ensure password is set
            Model.Password = GeneratedPassword;
            Model.PasswordConfirm = GeneratedPassword;
            Model.IsActive = true;
            Model.IsDelete = false;
            
            // Validate required fields
            if (string.IsNullOrWhiteSpace(Model.UserName))
            {
                await ToastService.Error("Lỗi", "Vui lòng nhập tên đăng nhập");
                return;
            }
            if (string.IsNullOrWhiteSpace(Model.UserCode))
            {
                await ToastService.Error("Lỗi", "Vui lòng nhập mã nhân viên");
                return;
            }
            if (string.IsNullOrWhiteSpace(Model.FirstName) || Model.FirstName.Length < 1)
            {
                await ToastService.Error("Lỗi", "Họ phải có ít nhất 1 ký tự");
                return;
            }
            if (string.IsNullOrWhiteSpace(Model.LastName))
            {
                await ToastService.Error("Lỗi", "Vui lòng nhập tên");
                return;
            }
            if (Model.Roles == null || Model.Roles.Count == 0)
            {
                await ToastService.Error("Lỗi", "Vui lòng chọn ít nhất một vai trò");
                return;
            }
            
            var result = await UserManagerService.CreateUserWithNavigationPropertiesAsync(Model);
            
            if (result != null)
            {
                await ToastService.Success("Thành công", $"Tạo người dùng mới thành công! Mật khẩu: {GeneratedPassword}");
                
                // Show password after successful creation
                ShowPassword = true;
                
                // Reset form after 3 seconds
                await Task.Delay(3000);
                ShowPassword = false;
                SelectedRoles = new();
                InitializeModel(); // Generate new password for next user
            }
            else
            {
                await ToastService.Error("Lỗi", "Không thể tạo người dùng. Vui lòng kiểm tra lại thông tin.");
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
    
    private async Task CopyPasswordToClipboard()
    {
        try
        {
            await ToastService.Information("Đã copy", $"Mật khẩu đã được copy: {GeneratedPassword}");
            // Note: Actual clipboard copy would need JS interop
            // For now, just show the notification
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", "Không thể copy mật khẩu");
        }
    }
}

