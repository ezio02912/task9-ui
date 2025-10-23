using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services;

public interface IUserKeywordSelectionService
{
    Task<ResponseHttpBase<bool>> Create(CreateUpdateUserKeywordSelectionDto filter);
    Task<ResponseHttpBase<bool>> Deactivate(long id);
    Task<ResponseHttpBase<UserKeywordSelectionDto>> GetByKeywordAndUser(long keywordId, int userId);
}
public class UserKeywordSelectionService : IUserKeywordSelectionService
{

    public async Task<ResponseHttpBase<bool>> Create(CreateUpdateUserKeywordSelectionDto input)
    {
        return await RequestClient.PostAPIAsync<ResponseHttpBase<bool>>($"user-keyword-selection/create", input);
    }

    public async Task<ResponseHttpBase<bool>> Deactivate(long id)
    {
        return await RequestClient.PostAPIAsync<ResponseHttpBase<bool>>($"user-keyword-selection/deactivate/{id}", null);
    }

    public async Task<ResponseHttpBase<UserKeywordSelectionDto>> GetByKeywordAndUser(long keywordId, int userId)
    {
        return await RequestClient.GetAPIAsync<ResponseHttpBase<UserKeywordSelectionDto>>($"user-keyword-selection/get-by-user-and-keyword/{userId}/{keywordId}");
    }
}
