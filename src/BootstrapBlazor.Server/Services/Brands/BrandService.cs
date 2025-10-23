using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services;

public interface IBrandService
{
    Task<ResponseHttpBaseList<BrandDto>> GetListAsync();
}

public class BrandService : IBrandService
{
    public async Task<ResponseHttpBaseList<BrandDto>> GetListAsync()
    {
        return await RequestClient.GetAPIAsync<ResponseHttpBaseList<BrandDto>>("brand/get-list");
    }
}

