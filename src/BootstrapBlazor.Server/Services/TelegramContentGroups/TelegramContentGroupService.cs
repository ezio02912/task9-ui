
using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services
{
    public interface ITelegramContentGroupService
    {
        Task<ApiResponseBase<List<TelegramContentGroupDto>>> GetListAsync(BaseFilterPagingDto filter);
        Task<ApiResponseBase<TelegramContentGroupDto>> GetByIdAsync(int id);
    }

    public class TelegramContentGroupService : ITelegramContentGroupService
    {
        public async Task<ApiResponseBase<List<TelegramContentGroupDto>>> GetListAsync(BaseFilterPagingDto filter)
        {
            return await RequestClient.PostAPIAsync<ApiResponseBase<List<TelegramContentGroupDto>>>("telegram-content-group/get-list", filter);
        }

        public async Task<ApiResponseBase<TelegramContentGroupDto>> GetByIdAsync(int id)
        {
            return await RequestClient.GetAPIAsync<ApiResponseBase<TelegramContentGroupDto>>($"telegram-content-group/{id}");
        }
    }
}
