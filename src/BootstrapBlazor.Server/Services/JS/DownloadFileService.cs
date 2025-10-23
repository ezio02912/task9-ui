using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BootstrapBlazor.Server.Services
{
    public class DownloadFileService
    {
        private readonly IJSRuntime _jsInterop;

        public DownloadFileService(IJSRuntime jsInterop)
        {
            _jsInterop = jsInterop;
        }

        public async Task DownloadFileAsync(Byte[] bytes,string extension,string fileName)
        {
            await _jsInterop.InvokeVoidAsync("saveAsFile", $"{fileName}-{DateTime.Now:yyyyMMddHHmmss}.{extension}", Convert.ToBase64String(bytes));
        }
    }
}
