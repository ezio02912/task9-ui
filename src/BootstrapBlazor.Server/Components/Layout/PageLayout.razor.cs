

using System.Globalization;
using Microsoft.AspNetCore.Components.Authorization;
using BootstrapBlazor.Server.Identity;

namespace BootstrapBlazor.Server.Components.Layout;
/// <summary>
///
/// </summary>
public sealed partial class PageLayout
{
    [Inject]
    [NotNull]
    private ToastService? Toast { get; set; }
    [NotNull]
    private Modal? Modal { get; set; }
    [Inject]
    [NotNull]
    private NavigationManager? NavigationManager { get; set; }
    [Inject]
    [NotNull]
    private IUserManagerService? UerManagerService { get; set; }
    
    private string SelectedCulture { get; set; } = CultureInfo.CurrentUICulture.Name;
    private string? Theme { get; set; }

    private string? LayoutClassString => CssBuilder.Default("layout-demo")
        .AddClass(Theme)
        .Build();

    private List<MenuItem>? Menus { get; set; } = new List<MenuItem>();

    /// <summary>
    /// </summary>
    public bool IsFixedHeader { get; set; } = true;

    /// <summary>
    /// </summary>
    public bool IsFixedFooter { get; set; } = true;

    /// <summary>
    /// </summary>
    public bool IsFixedTabHeader { get; set; } = true;

    /// <summary>
    /// </summary>
    public bool IsFullSide { get; set; } = false;

    /// <summary>
    /// </summary>
    public bool ShowFooter { get; set; } = true;
    public string UserName { get; set; } = "Admin";
    
    /// <summary>
    /// Current user role - accessible by child components
    /// </summary>
    public string Role { get; set; } = "";

    /// <summary>
    /// </summary>
    public bool UseTabSet { get; set; } = false;

    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; }
    [Inject, NotNull]
    private IServiceProvider? ServiceProvider { get; set; }


    public NewUserPasswordDto NewPassword { get; set; } = new NewUserPasswordDto();
    /// <summary>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {

        await base.OnInitializedAsync();

        await Task.Delay(10);

       
        var auth = await AuthState;
        if (!auth!.User!.Identity!.IsAuthenticated!)
        {
            NavigationManager.NavigateTo("/login");
        }
        else
        {
            UserName = auth!.User!.Identity!.Name!;
            NewPassword.UserName = UserName;
        }
        try{
            var provider = ServiceProvider.GetService<AuthenticationStateProvider>();
            Role = await ((ApiAuthenticationStateProvider)provider!).GetUserRolesAsync();
        
            Menus.Add(new() { Text = "Cấu hình", Icon = "fa-fw fa-solid fa-house", Url = "/" });

            if (Role == "ADMIN" || Role.Contains("SEO")||Role == "QC"){

                Menus.Add(new() { Text = "Tools SEO", Icon = "fa-fw fa-solid fa-info", 
                    Items = new List<MenuItem>() {      
                    new() { Text = "Báo cáo SEO tổng quan", Icon = "fa-fw fa-solid fa-chart-simple", Url = "report-seo-performance" },
                    new() { Text = "Báo cáo SEO PIC", Icon = "fa-fw fa-solid fa-users", Url = "report-seo-performance-by-pic" },
                    new() { Text = "Báo cáo SEO chi tiết", Icon = "fa-fw fa-solid fa-chart-line", Url = "report-detail-seo-performance" },
                    new() { Text = "Báo cáo SEO BOD", Icon = "fa-fw fa-solid fa-address-card", Url = "report-detail-seo-performance-bod" },
                    new() { Text = "Performance SEO", Icon = "fa-fw fa-solid fa-percent", Url = "evaluate-seo-performance" },
                    // new() { Text = "Chat với AI SEO", Icon = "fa-fw fa-solid fa-robot", Url = "ai-seo-chat" },
                    new() { Text = "Lọc domain theo PIC", Icon = "fa-fw fa-solid fa-globe", Url = "filter-domain-by-pic" }, 
                    new() { Text = "PIC phụ trách keywords", Icon = "fa-fw fa-solid fa-address-book", Url = "users-keyword-selections" },
                    new() { Text = "Tool check redirect", Icon = "fa-fw fa-solid fa-link", Url = "check-redirect" },
                    new() { Text = "Top domain hàng ngày", Icon = "fa-fw fa-solid fa-chart-line", Url = "seo-daily-data" },
                    
                } });
            }
            
            if (Role == "ADMIN")
            {
                Menus.Add(new() { Text = "Tools Assistant", Icon = "fa-fw fa-solid fa-assistive-listening-systems", 
                    Items = new List<MenuItem>() {      
                    new() { Text = "Xử lý phiếu", Icon = "fa-fw fa-solid fa-toolbox", Url = "support-asst" },
                } });
            }

            if (Role == "ADMIN" || Role == "QC")
            {
                Menus.Add(new() { Text = "Tools QC", Icon = "fa-fw fa-solid fa-check-square", 
                    Items = new List<MenuItem>() {      
                    new() { Text = "Quy trình đổi domain", Icon = "fa-fw fa-solid fa-list-check", Url = "quy-trinh-doi-domain" },
                    new() { Text = "Tool CPD", Icon = "fa-fw fa-solid fa-toolbox", Url = "cpd-checker-tool" },
                    new() { Text = "Dashboard CPD", Icon = "fa-fw fa-solid fa-dashboard", Url = "cpd" },
                    new() { Text = "Tool Telegram", Icon = "fa-fw fa-brands fa-telegram", Url = "telegram-bot" },
                    new() { Text = "Báo cáo brand keyword", Icon = "fa-fw fa-solid fa-list-check", Url = "report-brand-keyword" },
                    new() { Text = "Quản lý ảnh", Icon = "fa-fw fa-solid fa-images", Url = "cpd-banners" },
                    new() { Text = "Quản lý top domain", Icon = "fa-fw fa-solid fa-images", Url = "upload-top-domain-daily" },
                } });
            }

            if (Role == "ADMIN" || Role == "HR"){
                Menus.Add(new() { Text = "Tools HRTA", Icon = "fa-fw fa-solid fa-commenting",
                    Items = new List<MenuItem>() {
                    new() { Text = "Nhóm facebook", Icon = "fa-fw fa-brands fa-facebook", Url = "facebook-group-manager" },
                    new() { Text = "Thông tin ứng viên", Icon = "fa-fw fa-solid fa-user", Url = "profile-crawler-manager" },
                } });
            }

            if (Role == "ADMIN"){
                Menus.Add(new() { Text = "Quản trị hệ thống", Icon = "fa-fw fa-solid fa-gear",
                    Items = new List<MenuItem>() {
                    new() { Text = "Quản lý người dùng", Icon = "fa-fw fa-solid fa-user", Url = "user-manager" },
                    new() { Text = "Quản lý Keyword", Icon = "fa-fw fa-solid fa-key", Url = "keyword-manager" },
                } });
            }
        }
        catch (Exception ex)
        {       
            NavigationManager.NavigateTo("/login");
        }

        await SetLang("vi-VN");
    }
    private Task SetLang(string cultureName)
    {
        if (SelectedCulture != cultureName)
        {
            var uri = new Uri(NavigationManager.Uri).GetComponents(UriComponents.PathAndQuery, UriFormat.SafeUnescaped);
            var query = $"?culture={Uri.EscapeDataString(cultureName)}&redirectUri={Uri.EscapeDataString(uri)}";
            NavigationManager.NavigateTo("/Culture/SetCulture" + query, forceLoad: true);
        }

        return Task.CompletedTask;
    }
    
    private async Task ShowModal()
    {
        await Modal.Show();
    }

    private async Task<bool> OnChangePasswordAsync()
    {
        var res = await UerManagerService!.SetNewPasswordAsync(NewPassword);
        if (res){
            await Toast.Success("Success", "Đổi mật khẩu thành công");
            return true;
        }
        else{
            await Toast.Error("Error", "Đổi mật khẩu thất bại");
            return false;
        }

    }
    private Task Logout()
    {
        UerManagerService!.Logout();
        NavigationManager.NavigateTo("/login");
        return Task.CompletedTask;
    }
}
