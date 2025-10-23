using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services
{

    public interface IKeywordRankingService
    {
        Task<ResponseHttpBaseList<KeywordRankingWithPicDto>> GetListAsync(KeywordRankingFilter filter);

        Task<ResponseHttpBaseBool> ImportExcelAsync(KeywordRankingImportRequest req);

        Task<ResponseHttpBase<KeywordRankingReportResponseDto>> GetReportKeywordRankingAsync(KeywordRankingFilter filter);

        Task<ResponseHttpBase<SeoStatRow>> GetReportKeywordRankingDetailAsync(KeywordRankingFilter filter);
        Task<ResponseHttpBase<SeoStatRow>> GetReportKeywordRankingDetailBODAsync(KeywordRankingFilter filter);

        Task<ResponseHttpBase<KeywordSumarySeoReportDto>> GetSummaryReportKeywordRankingAsync(KeywordRankingFilter filter);

        Task<ResponseHttpBaseList<KeywordRankingReportDto>> GetReportRankingByPicAsync(KeywordRankingFilter filter);

        Task<ResponseHttpBaseBool> DeleteDataOfCurrentMonthAsync();

        Task<ResponseHttpBase<KeywordSeoPerformanceReportDto>> GetSeoPerformanceReportAsync(KeywordRankingFilter filter);
        Task<ResponseHttpBase<KeywordSeoPerformanceReportDto>> GetSeoPerformanceReportWithOutDetailAsync(KeywordRankingFilter filter);

        Task<byte[]> ExportExcelReportRankingDetailAsync(KeywordRankingFilter filter);
    }
    public class KeywordRankingService : IKeywordRankingService
    {
        public async Task<ResponseHttpBaseList<KeywordRankingWithPicDto>> GetListAsync(KeywordRankingFilter filter)
        {
            return await RequestClient.PostAPIAsync<ResponseHttpBaseList<KeywordRankingWithPicDto>>("keyword-ranking/get-list", filter);
        }

        public async Task<ResponseHttpBaseBool> ImportExcelAsync(KeywordRankingImportRequest req)
        {
            return await RequestClient.PostAPIAsync<ResponseHttpBaseBool>("keyword-ranking/import-excel", req);
        }

        public async Task<ResponseHttpBase<KeywordRankingReportResponseDto>> GetReportKeywordRankingAsync(KeywordRankingFilter filter)
        {
            return await RequestClient.PostAPIAsync<ResponseHttpBase<KeywordRankingReportResponseDto>>("keyword-ranking/get-report-keyword-ranking", filter);
        }

        public async Task<ResponseHttpBase<SeoStatRow>> GetReportKeywordRankingDetailAsync(KeywordRankingFilter filter)
        {
            return await RequestClient.PostAPIAsync<ResponseHttpBase<SeoStatRow>>("keyword-ranking/get-report-keyword-ranking-detail", filter);
        }

        public async Task<ResponseHttpBase<SeoStatRow>> GetReportKeywordRankingDetailBODAsync(KeywordRankingFilter filter)
        {
            return await RequestClient.PostAPIAsync<ResponseHttpBase<SeoStatRow>>("keyword-ranking/get-report-keyword-ranking-detail-bod", filter);
        }

        public async Task<ResponseHttpBase<KeywordSumarySeoReportDto>> GetSummaryReportKeywordRankingAsync(KeywordRankingFilter filter)
        {
            return await RequestClient.PostAPIAsync<ResponseHttpBase<KeywordSumarySeoReportDto>>("keyword-ranking/get-summary-report-keyword-ranking", filter);
        }

        public async Task<ResponseHttpBaseList<KeywordRankingReportDto>> GetReportRankingByPicAsync(KeywordRankingFilter filter)
        {
            return await RequestClient.PostAPIAsync<ResponseHttpBaseList<KeywordRankingReportDto>>("keyword-ranking/get-report-keyword-ranking-by-pic", filter);
        }

        public async Task<ResponseHttpBaseBool> DeleteDataOfCurrentMonthAsync()
        {
            return await RequestClient.PostAPIAsync<ResponseHttpBaseBool>("keyword-ranking/delete-data-of-current-month", null);
        }

        public async Task<ResponseHttpBase<KeywordSeoPerformanceReportDto>> GetSeoPerformanceReportAsync(KeywordRankingFilter filter)
        {
            return await RequestClient.PostAPIAsync<ResponseHttpBase<KeywordSeoPerformanceReportDto>>("keyword-ranking/get-seo-performance-report", filter);
        }
        public async Task<ResponseHttpBase<KeywordSeoPerformanceReportDto>> GetSeoPerformanceReportWithOutDetailAsync(KeywordRankingFilter filter)
        {
            return await RequestClient.PostAPIAsync<ResponseHttpBase<KeywordSeoPerformanceReportDto>>("keyword-ranking/get-seo-performance-report-without-detail", filter);
        }

        public async Task<byte[]> ExportExcelReportRankingDetailAsync(KeywordRankingFilter filter)
        {
            return await RequestClient.PostAPIBytesAsync("keyword-ranking/export-excel-report-ranking-detail", filter);
        }
    }
}
