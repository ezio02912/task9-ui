using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services;

public interface IKeywordMonthlyMetricService
{
    Task<ResponseHttpBaseList<KeywordMonthlyMetricDto>> GetListAsync(long keywordId);
    Task<ResponseHttpBase<KeywordMonthlyMetricDto>> UpsertAsync(CreateUpdateKeywordMonthlyMetricDto input);
    Task<ResponseHttpBaseBool> DeleteAsync(long keywordId, string yearMonth);
}

public class KeywordMonthlyMetricService : IKeywordMonthlyMetricService
{
    public async Task<ResponseHttpBaseList<KeywordMonthlyMetricDto>> GetListAsync(long keywordId)
    {
        return await RequestClient.GetAPIAsync<ResponseHttpBaseList<KeywordMonthlyMetricDto>>($"keyword-monthly-metric/get-list/{keywordId}");
    }

    public async Task<ResponseHttpBase<KeywordMonthlyMetricDto>> UpsertAsync(CreateUpdateKeywordMonthlyMetricDto input)
    {
        return await RequestClient.PostAPIAsync<ResponseHttpBase<KeywordMonthlyMetricDto>>("keyword-monthly-metric/upsert", input);
    }

    public async Task<ResponseHttpBaseBool> DeleteAsync(long keywordId, string yearMonth)
    {
        return await RequestClient.DeleteAPIAsync<ResponseHttpBaseBool>($"keyword-monthly-metric/delete/{keywordId}/{yearMonth}");
    }
}

