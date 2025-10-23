using System.Text;
using System.Linq;
using BootstrapBlazor.Components;
using Microsoft.JSInterop;

namespace BootstrapBlazor.Server.Components.Task9;
public partial class SupportAsstCheckTop : IDisposable
{
    #region Inject
    [Inject, NotNull] private ClipboardService? ClipboardService { get; set; }
    [Inject, NotNull]
    private MaskService? MaskService { get; set; }
    [Inject]
    [NotNull]
    private ToastService? Toast { get; set; }
    #endregion
    [Inject, NotNull]
    private IJSRuntime? JSRuntime { get; set; }
    private CancellationTokenSource? TokenSource { get; set; }
    [NotNull]
    private Mask? CustomMask1 { get; set; }
    private DropUpload DropUpload1 { get; set; }
    private async Task OnUpload()
    {
        await MaskService.Show(new MaskOption()
        {
            ChildContent = builder => builder.AddContent(0, new MarkupString("<i class=\"text-white fa-solid fa-3x fa-spinner fa-spin-pulse\"></i><span class=\"ms-3 fs-2 text-white\">Đang xử lý ....</span>")),
            ContainerId = "div-mask-loading"
        }, CustomMask1);

        try 
        {
            List<UploadFile> files = ((IUpload)DropUpload1).UploadFiles;
            if(files == null || files.Count == 0 )
            {
                await Toast.Error("Lỗi", "Không có file để xử lý");
                return;
            }
            var file = files[0].File;
            //var csvBytes = await Helper.FileHelper.FilterByColumnToCsvBytes(file,  "K", "MKT02");
            
            // Copy clipboard ở dạng text nếu cần
           //var csvText = Encoding.UTF8.GetString(csvBytes);
           // await ClipboardService.Copy(csvText);
           // await Toast.Success("Success", $"Đã copy vào clipboard");
            
           // var b64 = Convert.ToBase64String(csvBytes);
           // await JSRuntime.InvokeVoidAsync("__downloadCsv", $"checktop_{DateTime.Now:yyyyMMdd_HHmmss}.csv", b64);
        }
        catch (Exception ex)
        {
            await Toast.Error("Lỗi", $"Lỗi khi upload file: {ex.Message}");
        }
        finally{
            await MaskService.Close(CustomMask1);
        }
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

