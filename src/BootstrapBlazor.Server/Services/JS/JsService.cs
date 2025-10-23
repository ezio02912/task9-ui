using Microsoft.JSInterop;

namespace BootstrapBlazor.Server.Services
{
    public class JsService
    {
        private readonly IJSRuntime _jsInterop;

        public JsService(IJSRuntime jsInterop)
        {
            _jsInterop = jsInterop;
        }
        public async void SetTitle(string title)
        {
            await _jsInterop.InvokeVoidAsync("setTitle",title);
        }

        public async void setFavicon(string path)
        {
            try
            {

            await _jsInterop.InvokeVoidAsync("setFavicon",path);
            }catch(Exception ex)
            {

            }
        }
    }
}
