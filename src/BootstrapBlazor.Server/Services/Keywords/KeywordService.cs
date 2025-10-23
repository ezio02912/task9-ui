using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services;

public interface IKeywordService
{
    Task<ResponseHttpBaseList<KeywordWithUserCountDto>> GetListAsync(KeywordFilter filter);
    Task<ResponseHttpBaseList<KeywordDto>> GetListSimpleAsync(KeywordFilter filter);
    Task<ResponseHttpBase<KeywordDto>> GetByIdAsync(long id);
    Task<ResponseHttpBase<KeywordDto>> CreateAsync(CreateUpdateKeywordDto input);
    Task<ResponseHttpBase<KeywordDto>> UpdateAsync(long id, CreateUpdateKeywordDto input);
    Task<ResponseHttpBaseBool> DeleteAsync(long id);
}

public class KeywordService : IKeywordService
{
    public async Task<ResponseHttpBaseList<KeywordWithUserCountDto>> GetListAsync(KeywordFilter filter)
    {
        return await RequestClient.PostAPIAsync<ResponseHttpBaseList<KeywordWithUserCountDto>>($"keyword/get-list-with-user-count", filter);
    }

    public async Task<ResponseHttpBaseList<KeywordDto>> GetListSimpleAsync(KeywordFilter filter)
    {
        return await RequestClient.PostAPIAsync<ResponseHttpBaseList<KeywordDto>>($"keyword/get-list", filter);
    }

    public async Task<ResponseHttpBase<KeywordDto>> GetByIdAsync(long id)
    {
        return await RequestClient.GetAPIAsync<ResponseHttpBase<KeywordDto>>($"keyword/get-by-id/{id}");
    }

    public async Task<ResponseHttpBase<KeywordDto>> CreateAsync(CreateUpdateKeywordDto input)
    {
        return await RequestClient.PostAPIAsync<ResponseHttpBase<KeywordDto>>($"keyword/create", input);
    }

    public async Task<ResponseHttpBase<KeywordDto>> UpdateAsync(long id, CreateUpdateKeywordDto input)
    {
        return await RequestClient.PostAPIAsync<ResponseHttpBase<KeywordDto>>($"keyword/update/{id}", input);
    }

    public async Task<ResponseHttpBaseBool> DeleteAsync(long id)
    {
        return await RequestClient.DeleteAPIAsync<ResponseHttpBaseBool>($"keyword/delete/{id}");
    }
}
