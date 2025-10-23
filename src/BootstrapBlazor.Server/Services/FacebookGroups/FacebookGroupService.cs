
using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services
{
    public interface IFacebookGroupService
    {
        Task<ApiResponseBase<List<FacebookGroupDto>>> GetListAsync(BaseFilterPagingDto filter);
        Task<ApiResponseBase<FacebookGroupDto>> GetByIdAsync(int id);
        Task<ApiResponseBase<FacebookPostDto>> CreateAsync(FacebookGroupDto dto);
        Task<ApiResponseBase<bool>> UpdateAsync(int id, FacebookGroupDto dto);
        Task<ApiResponseBase<bool>> DeleteAsync(int id);
    }

    public class FacebookGroupService : IFacebookGroupService
    {
        public async Task<ApiResponseBase<List<FacebookGroupDto>>> GetListAsync(BaseFilterPagingDto filter)
        {
            return await RequestClient.PostAPIAsync<ApiResponseBase<List<FacebookGroupDto>>>("facebook-group/get-list", filter);
        }

        public async Task<ApiResponseBase<FacebookGroupDto>> GetByIdAsync(int id)
        {
            return await RequestClient.GetAPIAsync<ApiResponseBase<FacebookGroupDto>>($"facebook-group/get-by-id/{id}");
        }

        public async Task<ApiResponseBase<FacebookPostDto>> CreateAsync(FacebookGroupDto dto)
        {
            return await RequestClient.PostAPIAsync<ApiResponseBase<FacebookPostDto>>($"facebook-group/create", dto);
        }

        public async Task<ApiResponseBase<bool>> UpdateAsync(int id, FacebookGroupDto dto)
        {
            return await RequestClient.PutAPIAsync<ApiResponseBase<bool>>($"facebook-group/update/{id}", dto);
        }

        public async Task<ApiResponseBase<bool>> DeleteAsync(int id)
        {
            return await RequestClient.DeleteAPIAsync<ApiResponseBase<bool>>($"facebook-group/delete/{id}");
        }
    }
}
