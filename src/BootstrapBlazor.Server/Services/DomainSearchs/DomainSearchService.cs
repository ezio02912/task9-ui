
using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services
{
    public interface IDomainSearchService
    {
        Task<ApiResponseBase<List<DomainSearchDto>>> GetListAsync(DomainSearchFilterPagingDto filter);
        Task<ApiResponseBase<DomainSearchDto>> GetByIdAsync(int id);
        Task<ApiResponseBase<DomainSearchDto>> CreateAsync(DomainSearchDto dto);
        Task<ApiResponseBase<bool>> CreateListAsync(List<DomainSearchDto> dto);
        Task<ApiResponseBase<bool>> UpdateAsync(int id, DomainSearchDto dto);
        Task<ApiResponseBase<bool>> DeleteAsync(int id);
    }

    public class DomainSearchService : IDomainSearchService
    {
        public async Task<ApiResponseBase<List<DomainSearchDto>>> GetListAsync(DomainSearchFilterPagingDto filter)
        {
            return await RequestClient.PostAPIAsync<ApiResponseBase<List<DomainSearchDto>>>("domain-search/get-list", filter);
        }

        public async Task<ApiResponseBase<DomainSearchDto>> GetByIdAsync(int id)
        {
            return await RequestClient.GetAPIAsync<ApiResponseBase<DomainSearchDto>>($"domain-search/get-by-id/{id}");
        }

        public async Task<ApiResponseBase<DomainSearchDto>> CreateAsync(DomainSearchDto dto)
        {
            return await RequestClient.PostAPIAsync<ApiResponseBase<DomainSearchDto>>($"domain-search/create", dto);
        }
        public async Task<ApiResponseBase<bool>> CreateListAsync(List<DomainSearchDto> dto)
        {
            return await RequestClient.PostAPIAsync<ApiResponseBase<bool>>($"domain-search/create-many", dto);
        }


        public async Task<ApiResponseBase<bool>> UpdateAsync(int id, DomainSearchDto dto)
        {
            return await RequestClient.PutAPIAsync<ApiResponseBase<bool>>($"domain-search/update/{id}", dto);
        }

        public async Task<ApiResponseBase<bool>> DeleteAsync(int id)
        {
            return await RequestClient.DeleteAPIAsync<ApiResponseBase<bool>>($"domain-search/delete/{id}");
        }
    }
}
