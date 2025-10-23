using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services
{
    public interface ILeagueService
    {
        Task<ResponseHttpBaseList<LeagueDto>> GetListAsync(LeagueFilter filter);
        Task<ResponseHttpBaseBool> CreateAsync(LeagueDto league);
        Task<ResponseHttpBaseBool> UpdateAsync(LeagueDto league);
        Task<ResponseHttpBaseBool> DeleteAsync(int id);
    }

    public class LeagueService : ILeagueService
    {
        public LeagueService()
        {
        }

        public async Task<ResponseHttpBaseList<LeagueDto>> GetListAsync(LeagueFilter filter)
        {
            return await RequestClient.PostAPIAsync<ResponseHttpBaseList<LeagueDto>>("league/get-list", filter);
        }

        public async Task<ResponseHttpBaseBool> CreateAsync(LeagueDto league)
        {
            return await RequestClient.PostAPIAsync<ResponseHttpBaseBool>("league/create", league);
        }

        public async Task<ResponseHttpBaseBool> UpdateAsync(LeagueDto league)
        {
            return await RequestClient.PutAPIAsync<ResponseHttpBaseBool>($"league/update/{league.Id}", league);
        }

        public async Task<ResponseHttpBaseBool> DeleteAsync(int id)
        {
            return await RequestClient.DeleteAPIAsync<ResponseHttpBaseBool>($"league/delete/{id}");
        }
    }
}
