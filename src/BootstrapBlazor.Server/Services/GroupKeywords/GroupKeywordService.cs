using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services;

public interface IGroupKeywordService
{
    Task<ResponseHttpBaseList<GroupKeywordDto>> GetListAsync(int groupId = 0);
     Task<ResponseHttpBaseBool> ImportExcelAsync(ImportRequestBase req);
}
public class GroupKeywordService : IGroupKeywordService
{

    public async Task<ResponseHttpBaseList<GroupKeywordDto>> GetListAsync(int groupId = 0)
    {
        return await RequestClient.GetAPIAsync<ResponseHttpBaseList<GroupKeywordDto>>($"group-keyword/get-list/{groupId}");
    }

    public async Task<ResponseHttpBaseBool> ImportExcelAsync(ImportRequestBase req)
    {
        return await RequestClient.PostAPIAsync<ResponseHttpBaseBool>("group-keyword/import-excel", req);
    }
}
