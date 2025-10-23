using Microsoft.AspNetCore.Components.Authorization;
namespace BootstrapBlazor.Server.Components.Layout;

public partial class TutorialsLayout : IDisposable
{
    [Inject]
    [NotNull]
    private IDispatchService<MessageItem>? DispatchService { get; set; }

    [Inject]
    [NotNull]
    private ToastService? Toast { get; set; }

    [Inject]
    [NotNull]
    private NavigationManager? NavigationManager { get; set; }
    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            try
            {
                var auth = await AuthState;
                if (!auth.User.Identity.IsAuthenticated)
                {
                    NavigationManager.NavigateTo("/login");
                }
            }
            catch
            {
            }
        }
    }

    private async Task Dispatch(DispatchEntry<MessageItem> entry)
    {
        if (entry.Entry != null)
        {
            await Toast.Show(new ToastOption()
            {
                Title = "Dispatch",
                Content = entry.Entry.Message,
                Category = ToastCategory.Information,
                Delay = 30 * 1000,
                ForceDelay = true
            });
        }
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            DispatchService.UnSubscribe(Dispatch);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
