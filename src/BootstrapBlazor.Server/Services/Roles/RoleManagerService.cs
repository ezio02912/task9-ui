using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services
{
    public interface IRoleManagerService
    {
        Task<List<RoleDto>> GetListAsync();
        public Task<List<RoleClaimDto>> GetClaimListAsync(int roleId);
    }
    public class RoleManagerService  : IRoleManagerService
    {
        public async Task<List<RoleDto>> GetListAsync()
        {
           return await RequestClient.GetAPIAsync<List<RoleDto>>("role");
        }
        public Task<List<RoleClaimDto>> GetClaimListAsync(int roleId)
        {
            throw new NotImplementedException();
        }
    }
}
