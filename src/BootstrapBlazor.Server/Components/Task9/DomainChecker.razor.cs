using System.Text.Json;
using Newtonsoft.Json.Linq;
using BootstrapBlazor.Server.Helper;

namespace BootstrapBlazor.Server.Components.Task9;
public partial class DomainChecker : IDisposable
{
    #region Inject
    [Inject, NotNull]
    private MaskService? MaskService { get; set; }
    [Inject, NotNull] private ClipboardService? ClipboardService { get; set; }
    [Inject]
    [NotNull]
    private ToastService? Toast { get; set; }
    #endregion
    private CancellationTokenSource? TokenSource { get; set; }
    private List<DomainCheckerDto> Datas { get; set; } = new();
    private Table<DomainCheckerDto>? TableRef { get; set; }
    [NotNull]
    private Mask? CustomMask1 { get; set; }
    private string? DomainListTextArea { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {  
        base.OnInitialized();
    }

    private async Task OnCheckRedirect()
    {
        Datas.Clear();
        if(string.IsNullOrWhiteSpace(DomainListTextArea))
        {
            await Toast.Error("Thông báo", "Vui lòng nhập danh sách tên miền.");
            return;
        }

        var domainList = DomainListTextArea.Split("\n");
        if(domainList.Length == 0)
        {
            await Toast.Error("Thông báo", "Vui lòng nhập danh sách tên miền.");
            return;
        }

        Datas.AddRange(domainList.Select(domain => new DomainCheckerDto()
        {
            Domain = domain,
            RedirectLogs = string.Empty,
            FinalDomain = string.Empty
        }));
        await MaskService.Show(new MaskOption()
        {
            ChildContent = builder => builder.AddContent(0, new MarkupString("<i class=\"text-white fa-solid fa-3x fa-spinner fa-spin-pulse\"></i><span class=\"ms-3 fs-2 text-white\">Đang kiểm tra dữ liệu vui lòng chờ ....</span>")),
        }, CustomMask1);

        foreach(var domain in Datas)
        {
            try 
            {
                (string finalDomain, string logs)  =await RedirectChecker.CheckRedirectAsync(domain);
                domain.RedirectLogs = logs;
                domain.FinalDomain = finalDomain;

            }catch(Exception ex)
            {
                domain.RedirectLogs = ex.Message;
                continue;
            }
        }

        await MaskService.Close(CustomMask1);
        StateHasChanged();
    }
    
    private async Task CopyData()
    {
        string titles ="Domain\tFinal Domain\tRedirect Logs\t";
        var data = Datas.Select(row => 
            string.Join("\t", row.Domain, row.FinalDomain, row.RedirectLogs)
        ).ToList();
        data.Insert(0, titles);
        await ClipboardService.Copy(string.Join("\n", data));
        await Toast.Success("Success", "Đã copy dữ liệu vào clipboard");
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
