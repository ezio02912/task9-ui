using Microsoft.AspNetCore.Components;

namespace BootstrapBlazor.Server.Components.Task9.UserManager;

public partial class UserList : ComponentBase
{
    [Inject]
    [NotNull]
    private IUserManagerService? UserManagerService { get; set; }
    
    [Inject]
    [NotNull]
    private ToastService? ToastService { get; set; }
    
    [Inject]
    [NotNull]
    private NavigationManager? NavigationManager { get; set; }
    
    [Inject]
    [NotNull]
    private SwalService? SwalService { get; set; }
    
    private List<UserWithNavigationPropertiesDto> Users { get; set; } = new();
    private int TotalItems { get; set; } = 0;
    private bool IsLoading { get; set; } = false;
    
    private static IEnumerable<int> PageItemsSource => new int[] { 10, 20, 40, 80, 100 };
    
    private Table<UserWithNavigationPropertiesDto>? TableRef { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            IsLoading = true;
            Users = await UserManagerService.GetListWithNavigationAsync();
            TotalItems = Users.Count;
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải danh sách người dùng: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task<QueryData<UserWithNavigationPropertiesDto>> OnQueryAsync(QueryPageOptions options)
    {
        try
        {
            IsLoading = true;
            
            var allUsers = await UserManagerService.GetListWithNavigationAsync();
            
            // Apply search filter
            if (!string.IsNullOrEmpty(options.SearchText))
            {
                allUsers = allUsers.Where(u => 
                    u.User.UserCode.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase) ||
                    u.User.UserName.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase) ||
                    u.User.FirstName.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase) ||
                    u.User.LastName.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (u.User.Email != null && u.User.Email.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            
            TotalItems = allUsers.Count;
            
            // Apply pagination
            Users = allUsers
                .Skip((options.PageIndex - 1) * options.PageItems)
                .Take(options.PageItems)
                .ToList();
            
            return new QueryData<UserWithNavigationPropertiesDto>()
            {
                Items = Users,
                TotalCount = TotalItems
            };
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải danh sách người dùng: {ex.Message}");
            return new QueryData<UserWithNavigationPropertiesDto>()
            {
                Items = new List<UserWithNavigationPropertiesDto>(),
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
        NavigationManager.NavigateTo("/user-manager/create");
    }
    
    private void OnEditClick(int id)
    {
        NavigationManager.NavigateTo($"/user-manager/edit/{id}");
    }
    
    private string GetGenderText(Gender gender)
    {
        return gender switch
        {
            Gender.Male => "Nam",
            Gender.Female => "Nữ",
            Gender.Unknown => "Không xác định",
            _ => "N/A"
        };
    }
}

