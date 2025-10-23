
using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services
{
    public interface IMainsiteService
    {
        Task<ResponseHttpBaseList<MainsiteDto>> GetListAsync(FilterPagingBase filter);
        Task<ResponseHttpBaseBool> CreateAsync(MainsiteDto mainsite);
        Task<ResponseHttpBaseBool> UpdateAsync(MainsiteDto mainsite);
        Task<ResponseHttpBaseBool> DeleteAsync(int id);
    }

    public class MainsiteService : IMainsiteService
    {
        public MainsiteService()
        {
        }

        public async Task<ResponseHttpBaseList<MainsiteDto>> GetListAsync(FilterPagingBase filter)
        {
            return await RequestClient.PostAPIAsync<ResponseHttpBaseList<MainsiteDto>>("mainsite/get-list", filter);
        }

        public async Task<ResponseHttpBaseBool> CreateAsync(MainsiteDto mainsite)
        {
            return await RequestClient.PostAPIAsync<ResponseHttpBaseBool>("mainsite/create", mainsite);
        }

        public async Task<ResponseHttpBaseBool> UpdateAsync(MainsiteDto mainsite)
        {
            return await RequestClient.PutAPIAsync<ResponseHttpBaseBool>("mainsite/update", mainsite);
        }

        public async Task<ResponseHttpBaseBool> DeleteAsync(int id)
        {
            return await RequestClient.DeleteAPIAsync<ResponseHttpBaseBool>($"mainsite/delete/{id}");
        }
    }
}
