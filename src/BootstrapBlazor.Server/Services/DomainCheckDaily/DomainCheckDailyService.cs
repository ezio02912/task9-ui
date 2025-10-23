
using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services
{
    public interface IDomainCheckDailyService
    {
        Task<ApiResponseBase<List<DomainCheckDailyDto>>> GetListAsync(DomainCheckDailyFilterPagingDto filter);
        Task<ApiResponseBase<DomainCheckDailyDto>> GetByIdAsync(int id);
        Task<ApiResponseBase<DomainCheckDailyDto>> CreateAsync(DomainCheckDailyDto dto);
        Task<ApiResponseBase<bool>> CreateListAsync(List<DomainCheckDailyDto> dto);
        Task<ApiResponseBase<bool>> UpdateAsync(int id, DomainCheckDailyDto dto);
        Task<ApiResponseBase<bool>> DeleteAsync(int id);
    }

    public class DomainCheckDailyService : IDomainCheckDailyService
    {
        public async Task<ApiResponseBase<List<DomainCheckDailyDto>>> GetListAsync(DomainCheckDailyFilterPagingDto filter)
        {
            return await RequestClient.PostAPIAsync<ApiResponseBase<List<DomainCheckDailyDto>>>("domain-check-daily/get-list", filter);
        }

        public async Task<ApiResponseBase<DomainCheckDailyDto>> GetByIdAsync(int id)
        {
            return await RequestClient.GetAPIAsync<ApiResponseBase<DomainCheckDailyDto>>($"domain-check-daily/get-by-id/{id}");
        }

        public async Task<ApiResponseBase<DomainCheckDailyDto>> CreateAsync(DomainCheckDailyDto dto)
        {
            return await RequestClient.PostAPIAsync<ApiResponseBase<DomainCheckDailyDto>>($"domain-check-daily/create", dto);
        }
        public async Task<ApiResponseBase<bool>> CreateListAsync(List<DomainCheckDailyDto> dto)
        {
            return await RequestClient.PostAPIAsync<ApiResponseBase<bool>>($"domain-check-daily/create-many", dto);
        }


        public async Task<ApiResponseBase<bool>> UpdateAsync(int id, DomainCheckDailyDto dto)
        {
            return await RequestClient.PutAPIAsync<ApiResponseBase<bool>>($"domain-check-daily/update/{id}", dto);
        }

        public async Task<ApiResponseBase<bool>> DeleteAsync(int id)
        {
            return await RequestClient.DeleteAPIAsync<ApiResponseBase<bool>>($"domain-check-daily/delete/{id}");
        }
    }
}
