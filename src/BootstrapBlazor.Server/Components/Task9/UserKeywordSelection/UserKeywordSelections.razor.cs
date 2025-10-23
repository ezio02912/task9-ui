
using BootstrapBlazor.Server.Data;
using BootstrapBlazor.Components;
using BootstrapBlazor.Server.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using System.Linq;

namespace BootstrapBlazor.Server.Components.Task9.UserKeywordSelection;
public partial class UserKeywordSelections : ComponentBase, IDisposable
{
    #region Inject
    [Inject]
    private IBrandService BrandService { get; set; }
    [Inject]
    private IKeywordService KeywordService { get; set; }
    [Inject]
    private IUserManagerService UserManagerService { get; set; }
    [Inject]
    private IUserKeywordSelectionService UserKeywordSelectionService { get; set; }
    [Inject]
    private AuthenticationStateProvider AuthStateProvider { get; set; }

    [Inject]
    [NotNull]
    private ToastService? Toast { get; set; }
    [Inject]
    [NotNull]
    private DialogService? DialogService { get; set; }
    
    [CascadingParameter]
    [NotNull]
    private PageLayout? RootPage { get; set; }

    #endregion
    private CancellationTokenSource? TokenSource { get; set; }
   
    private List<KeywordWithUserCountDto> Datas = new();
    private List<KeywordWithUserCountDto> DataFilters = new();
    private Table<KeywordWithUserCountDto>? TableRef { get; set; }
    private KeywordFilter Filter { get; set; } = new();
    private List<UserIdentityDto>? Pics { get; set; }
    private UserIdentityDto? SelectedPic { get; set; }
    private string? CurrentUserFullName { get; set; }
    [NotNull]
    private List<SelectedItem<BrandDto>>? Brands { get; set; }
    
    [NotNull]
    private IEnumerable<SelectedItem> PicItems => Pics?.Select(i => new SelectedItem(i.Id.ToString(), i.FullName)).ToList() ?? new();
    
    private ITableSearchModel CustomerSearchModel { get; set; } = new KeywordSearchFilterDto();
    [NotNull]
    private Tab? TabRef { get; set; }
    private static IEnumerable<int> PageItemsSource => new int[]
    {
        10,
        20,
        40,
        80,
        100
    };
    private int TotalItems {set;get;} = 0;
    protected override async Task OnInitializedAsync()
    {
        base.OnInitialized();
        await LoadPics();
        await LoadBrands();
        
        var result = await KeywordService.GetListAsync(new KeywordFilter(){
            Skip = 0,
            Take = 10,
        });
        if (result.Status)
        {
            Datas = result.Data;
            TotalItems = result.Total;
        }
        
    }
    

    private async Task<QueryData<KeywordWithUserCountDto>> OnQueryFilterAsync(QueryPageOptions options)
    {
        await Task.Delay(200);
        
        try
        {
            var filterModel = CustomerSearchModel as KeywordSearchFilterDto;
            var result = await KeywordService.GetListAsync(new KeywordFilter()
            {
                
                Skip = (options.PageIndex - 1) * options.PageItems,
                Take = options.PageItems,
                KeywordText = filterModel?.KeywordText,
                UserId = filterModel?.Pic != null && filterModel?.Pic?.Value != "" && filterModel?.Pic?.Value != "0" ? int.Parse(filterModel.Pic.Value) : null,
                OnlyWithUsers = filterModel?.OnlyWithUsers ?? false,    
                BrandIds = filterModel.Brands != null && filterModel.Brands.Count > 0 ?  filterModel.Brands.Select(i => i.Id).ToList() : null
            });

            if (result != null && result.Status == true)
            {
                DataFilters = result.Data?.Cast<KeywordWithUserCountDto>().ToList() ?? new List<KeywordWithUserCountDto>();
                TotalItems = result.Total;
            }
            else
            {
                DataFilters = new List<KeywordWithUserCountDto>();
                TotalItems = 0;
            }
        }
        catch (Exception ex)
        {
            DataFilters = new List<KeywordWithUserCountDto>();
            TotalItems = 0;
            if (Toast != null)
            {
                await Toast.Show(new ToastOption()
                {
                    Category = ToastCategory.Error,
                    Title = "Lỗi",
                    Content = $"Không thể tải dữ liệu: {ex.Message}",
                    Delay = 5000
                });
            }
        }

        return new QueryData<KeywordWithUserCountDto>()
        {
            Items = DataFilters,
            TotalCount = TotalItems
        };
    }

    private async Task OnPicChanged(SelectedItem? value)
    {
        if (value != null && Pics != null)
        {
            SelectedPic = Pics.FirstOrDefault(p => p.Id.ToString() == value.Value);
        }
        else
        {
            SelectedPic = null;
        }
        await Task.CompletedTask;
    }

    private void ResetAllInputs()
    {
        Filter = new KeywordFilter();
        SelectedPic = null;
        StateHasChanged();
    }

    private void OnOnlyWithUsersChanged(bool value)
    {
        Filter.OnlyWithUsers = value;
    }

    private void OnSearchKeywordValueChanged(string value)
    {
        Filter.KeywordText = value;
    }

    private async Task OnTriggerFilterQueryAsync()
    {
        await Task.Delay(100);
        await TableRef!.QueryAsync();
        StateHasChanged();
    }
        
    private async Task ShowMonthlyVolumes(KeywordWithUserCountDto keyword)
    {
        if (keyword.MonthlyVolumes?.Any() == true && Toast != null)
        {
            var months = string.Join("\n", keyword.MonthlyVolumes.Select(m => $"{m.YearMonth}: {m.SearchVolume:N0}"));
            await Toast.Show(new ToastOption()
            {
                Category = ToastCategory.Information,
                Title = $"Tháng của keyword: {keyword.KeywordText}",
                Content = months,
                Delay = 0
            });
        }
    }

    private string GetHtmlRenderTooltip(KeywordWithUserCountDto keyword)
    {
        string html = "";
        if (keyword.MonthlyVolumes?.Any() == true)
        {
            for (int i = keyword.MonthlyVolumes.Count - 1; i >= 0; i--)
            {
                html += $"<b>{keyword.MonthlyVolumes[i].YearMonth}</b>: {keyword.MonthlyVolumes[i].SearchVolume:N0}<br/>";
            }
        }
        return html;
    }

    private async Task OnAddUserToKeyword(KeywordWithUserCountDto keyword)
    {
        try
        {
            // Get current user info
            var apiAuthProvider = (ApiAuthenticationStateProvider)AuthStateProvider;
            var currentUserIdStr = await apiAuthProvider.GetCurrentUserId();
            int? currentUserId = int.TryParse(currentUserIdStr, out var userId) ? userId : null;
            
            if (!currentUserId.HasValue)
            {
                await Toast!.Show(new ToastOption()
                {
                    Category = ToastCategory.Error,
                    Title = "Lỗi",
                    Content = "Không thể xác định người dùng hiện tại",
                    Delay = 3000
                });
                return;
            }

            // Show confirmation dialog
            var confirm = await DialogService!.ShowModal("Xác nhận", $"Bạn có chắc chắn muốn thêm chính mình vào keyword '{keyword.KeywordText}'?",
            new ResultDialogOption(){
                Size = Size.Small
            });

            System.Console.WriteLine($"AddUser confirm dialog result: {confirm}");
            
            if (confirm == DialogResult.Yes)
            {
                System.Console.WriteLine("User confirmed adding themselves, calling Create API...");
                
                // Create the assignment
                var createDto = new CreateUpdateUserKeywordSelectionDto
                {
                    KeywordId = keyword.Id,
                    UserId = currentUserId.Value
                };

                var createResult = await UserKeywordSelectionService.Create(createDto);
                System.Console.WriteLine($"Create API result: Status={createResult.Status}");
                
                if (createResult.Status)
                {
                    await Toast.Show(new ToastOption()
                    {
                        Category = ToastCategory.Success,
                        Title = "Thành công",
                        Content = $"Đã thêm chính mình vào keyword '{keyword.KeywordText}'",
                        Delay = 3000
                    });

                    await TableRef!.QueryAsync();
                    StateHasChanged();
                    System.Console.WriteLine("Table refreshed successfully");
                }
                else
                {
                    await Toast.Show(new ToastOption()
                    {
                        Category = ToastCategory.Error,
                        Title = "Lỗi",
                        Content = $"Không thể thêm vào keyword: {createResult.Message ?? "Lỗi không xác định"}",
                        Delay = 3000
                    });
                }
            }
        }
        catch (Exception ex)
        {
            await Toast!.Show(new ToastOption()
            {
                Category = ToastCategory.Error,
                Title = "Lỗi",
                Content = $"Không thể thêm người phụ trách: {ex.Message}",
                Delay = 5000
            });
        }
    }

    private async Task DeactivateUserFromKeyword(KeywordWithUserCountDto keyword, string userName)
    {
        try
        {
            // Get current user info
            var apiAuthProvider = (ApiAuthenticationStateProvider)AuthStateProvider;
            var currentUserIdStr = await apiAuthProvider.GetCurrentUserId();
            int? currentUserId = int.TryParse(currentUserIdStr, out var userId) ? userId : null;
            
            // Find user ID by name
            var user = Pics?.FirstOrDefault(p => p.FullName == userName);
            if (user == null)
            {
                await Toast!.Show(new ToastOption()
                {
                    Category = ToastCategory.Error,
                    Title = "Lỗi",
                    Content = "Không tìm thấy thông tin người dùng",
                    Delay = 3000
                });
                return;
            }
            
            // Check if current user is trying to remove someone else
            if (currentUserId.HasValue && user.Id != currentUserId.Value)
            {
                await Toast!.Show(new ToastOption()
                {
                    Category = ToastCategory.Error,
                    Title = "Không có quyền",
                    Content = "Bạn chỉ được phép gỡ tên của chính mình",
                    Delay = 3000
                });
                return;
            }

            // Get UserKeywordSelection ID
            var userKeywordResult = await UserKeywordSelectionService.GetByKeywordAndUser(keyword.Id, user.Id);
            if (!userKeywordResult.Status || userKeywordResult.Data == null)
            {
                await Toast.Show(new ToastOption()
                {
                    Category = ToastCategory.Error,
                    Title = "Lỗi",
                    Content = "Không tìm thấy thông tin phân công",
                    Delay = 3000
                });
                return;
            }

            // Show confirmation dialog using simple confirmation
            var confirm = await DialogService!.ShowModal("Xác nhận", $"Bạn có chắc chắn muốn gỡ {userName} khỏi keyword '{keyword.KeywordText}'?",
            new ResultDialogOption(){
                Size = Size.Small
            });

            System.Console.WriteLine($"Confirm dialog result: {confirm}");
            
            if (confirm == DialogResult.Yes)
            {
                System.Console.WriteLine("User confirmed deactivation, calling Deactivate API...");
                // Deactivate the assignment
                var deactivateResult = await UserKeywordSelectionService.Deactivate(userKeywordResult.Data.Id);
                System.Console.WriteLine($"Deactivate API result: Status={deactivateResult.Status}");
                if (deactivateResult.Status)
                {
                    await Toast.Show(new ToastOption()
                    {
                        Category = ToastCategory.Success,
                        Title = "Thành công",
                        Content = $"Đã gỡ {userName} khỏi keyword '{keyword.KeywordText}'",
                        Delay = 3000
                    });

                    await TableRef!.QueryAsync();
                    StateHasChanged();
                }
                else
                {
                    await Toast.Show(new ToastOption()
                    {
                        Category = ToastCategory.Error,
                        Title = "Lỗi",
                        Content = "Không thể gỡ người phụ trách",
                        Delay = 3000
                    });
                }
            }
        }
        catch (Exception ex)
        {
            await Toast!.Show(new ToastOption()
            {
                Category = ToastCategory.Error,
                Title = "Lỗi",
                Content = $"Không thể gỡ người phụ trách: {ex.Message}",
                Delay = 5000
            });
        }
    }
    


    #region Select components handler

    private async Task LoadBrands()
    {
        var result = await BrandService.GetListAsync();
        if (result.Status == true)
        {
            Brands = [.. result.Data.Select(i => new SelectedItem<BrandDto>(i, i.Name!))];
        }
    }
    private async Task LoadPics()
    {
        try
        {
            // Get current user info
            var apiAuthProvider = (ApiAuthenticationStateProvider)AuthStateProvider;
            var currentUserIdStr = await apiAuthProvider.GetCurrentUserId();
            int? currentUserId = int.TryParse(currentUserIdStr, out var userId) ? userId : null;
            
            var result = await UserManagerService.GetBasicSeoUserInfoAsync();
            if (result.Count > 0)
            {
                Pics = new List<UserIdentityDto>();
                Pics = result.ToList();
                Pics.Insert(0, new UserIdentityDto()
                {
                    Id = 0,
                    FullName = "Chọn tất cả",
                });
                
                // Set current user full name for UI check
                if (currentUserId.HasValue)
                {
                    var currentUser = result.FirstOrDefault(p => p.Id == currentUserId.Value);
                    CurrentUserFullName = currentUser?.FullName;
                }
            }
            else
            {
                Pics = new List<UserIdentityDto>();
            }
        }
        catch (Exception ex)
        {
            // Log error if needed
            Pics = new List<UserIdentityDto>();
            if (Toast != null)
            {
                await Toast.Show(new ToastOption()
                {
                    Category = ToastCategory.Error,
                    Title = "Lỗi",
                    Content = $"Không thể tải danh sách người dùng: {ex.Message}",
                    Delay = 5000
                });
            }
        }
        SelectedPic = null;
    }
    
    // Check if user name is current user - only current user can remove themselves
    private bool CanRemoveUser(string userName)
    {
        return !string.IsNullOrEmpty(CurrentUserFullName) && userName == CurrentUserFullName;
    }

    #endregion

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
