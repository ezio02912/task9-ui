using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services;

public interface IGroupService
{
    Task<ResponseHttpBaseList<GroupDto>> GetListAsync(int brandId=0);
    Task<ResponseHttpBaseBool> ImportExcelAsync(ImportRequestBase req);
}

public class GroupService : IGroupService
{
    public async Task<ResponseHttpBaseList<GroupDto>> GetListAsync(int brandId=0)
    {
        return await RequestClient.GetAPIAsync<ResponseHttpBaseList<GroupDto>>($"group/get-list/{brandId}");
    }

    public async Task<ResponseHttpBaseBool> ImportExcelAsync(ImportRequestBase req)
    {
        return await RequestClient.PostAPIAsync<ResponseHttpBaseBool>("group/import-excel", req);
    }
}
