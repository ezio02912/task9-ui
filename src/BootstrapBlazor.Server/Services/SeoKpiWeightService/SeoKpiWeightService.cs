using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services
{
    public interface ISeoKpiWeightService
    {
        Task<ApiResponseBase<List<SeoKpiWeightDto>>> GetListAsync();
    }

    public class SeoKpiWeightService : ISeoKpiWeightService
    {
        public async Task<ApiResponseBase<List<SeoKpiWeightDto>>> GetListAsync()
        {
            return await RequestClient.PostAPIAsync<ApiResponseBase<List<SeoKpiWeightDto>>>("seo-kpi-weight/get-list", null);
        }
    }
}
