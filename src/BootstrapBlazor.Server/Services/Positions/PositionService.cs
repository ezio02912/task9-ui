using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services
{
    public interface IPositionService
    {
        Task<List<PositionDto>> GetListAsync();
    }

    public class PositionService : IPositionService
    {
        public async Task<List<PositionDto>> GetListAsync()
        {
            return await RequestClient.GetAPIAsync<List<PositionDto>>("position");
        }
    }
}

