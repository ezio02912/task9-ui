using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services
{
    public interface IAppConfigService
    {
        Task<AppConfigDto> GetAppliedConfigAsync();
        Task<AppConfigDto> UpdateAsync(AppConfigDto input, int id);
    }
    public class AppConfigService : IAppConfigService
    {

        public async Task<AppConfigDto> GetAppliedConfigAsync()
        {
            return await RequestClient.GetAPIAsync<AppConfigDto>("app-config/get-applied-config");
        }

        public async  Task<AppConfigDto> UpdateAsync(AppConfigDto input, int id)
        {
            return await RequestClient.PutAPIAsync<AppConfigDto>($"app-config/{id}",input);
        }
    }
}
