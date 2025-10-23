using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services
{
    public interface IAIChatService
    {
        Task<ApiResponseBase<List<AIChatResponseDto>>> GetListAsync(AIChatFilterDto filter);
        Task<ApiResponseBase<AIChatResponseDto>> CreateAsync(CreateAIChatDto dto);
    }

    public class AIChatService : IAIChatService
    {
        public async Task<ApiResponseBase<List<AIChatResponseDto>>> GetListAsync(AIChatFilterDto filter)
        {
            return await RequestClient.PostAPIAsync<ApiResponseBase<List<AIChatResponseDto>>>("ai-chat/get-list", filter);
        }

        public async Task<ApiResponseBase<AIChatResponseDto>> CreateAsync(CreateAIChatDto dto)
        {
            return await RequestClient.PostAPIAsync<ApiResponseBase<AIChatResponseDto>>("ai-chat/chat", dto);
        }
    }
}
