using BootstrapBlazor.Server.Data;
using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services.DomainCheckDaily
{
    public interface ISeoDailyDataService
    {
        Task<ResponseHttpBase<UploadSeoDailyDataResponse>> UploadAsync(UploadSeoDailyDataRequest request);
        Task<ResponseHttpBase<List<SeoDailyDataWithManagedDomainsDto>>> GetDataAsync(GetSeoDailyDataRequest request);
        Task<ResponseHttpBase<List<DomainSearchDto>>> GetManagedDomainsAsync(string userName);
        Task<byte[]> ExportExcelAsync(GetSeoDailyDataRequest request);
        List<string> GetTopDomains(SeoDailyDataWithManagedDomainsDto item);
    }

    public class SeoDailyDataService : ISeoDailyDataService
    {
        public async Task<ResponseHttpBase<UploadSeoDailyDataResponse>> UploadAsync(UploadSeoDailyDataRequest request)
        {
            return await RequestClient.PostAPIAsync<ResponseHttpBase<UploadSeoDailyDataResponse>>(
                "task9/seo-daily-data/upload", 
                request
            );
        }

        public async Task<ResponseHttpBase<List<SeoDailyDataWithManagedDomainsDto>>> GetDataAsync(GetSeoDailyDataRequest request)
        {
            return await RequestClient.PostAPIAsync<ResponseHttpBase<List<SeoDailyDataWithManagedDomainsDto>>>(
                "task9/seo-daily-data/get-data", 
                request
            );
        }

        public async Task<ResponseHttpBase<List<DomainSearchDto>>> GetManagedDomainsAsync(string userName)
        {
            return await RequestClient.GetAPIAsync<ResponseHttpBase<List<DomainSearchDto>>>(
                $"task9/seo-daily-data/managed-domains?userName={Uri.EscapeDataString(userName)}"
            );
        }

        public async Task<byte[]> ExportExcelAsync(GetSeoDailyDataRequest request)
        {
            return await RequestClient.PostAPIBytesAsync(
                "task9/seo-daily-data/export-excel", 
                request
            );
        }

        public List<string> GetTopDomains(SeoDailyDataWithManagedDomainsDto item)
        {
            var domains = new List<string>();
            var topDomains = new[] { item.Top1, item.Top2, item.Top3, item.Top4, item.Top5,
                                   item.Top6, item.Top7, item.Top8, item.Top9, item.Top10 };
            
            foreach (var domain in topDomains)
            {
                if (!string.IsNullOrEmpty(domain))
                {
                    domains.Add(domain);
                }
            }
            
            return domains;
        }
    }
}
