using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services
{
    public interface IPicDomainAssignmentService
    {
        Task<ResponseHttpBaseList<PicDomainAssignmentDto>> GetListAsync(PicDomainAssignmentFilter filter);

        Task<ResponseHttpBaseBool> ImportExcelAsync(PicDomainAssignmentImportRequest req);

        Task<ResponseHttpBaseList<string>> GetPicListAsync();
    }

    public class PicDomainAssignmentService : IPicDomainAssignmentService
    {

        public async Task<ResponseHttpBaseList<PicDomainAssignmentDto>> GetListAsync(PicDomainAssignmentFilter filter)
        {
            return await RequestClient.PostAPIAsync<ResponseHttpBaseList<PicDomainAssignmentDto>>("pic-domain-assignment/get-list", filter);
        }

        public async Task<ResponseHttpBaseBool> ImportExcelAsync(PicDomainAssignmentImportRequest req)
        {
            return await RequestClient.PostAPIAsync<ResponseHttpBaseBool>("pic-domain-assignment/import-excel", req);
        }

        public async Task<ResponseHttpBaseList<string>> GetPicListAsync()
        {
            return await RequestClient.GetAPIAsync<ResponseHttpBaseList<string>>("pic-domain-assignment/get-pic-list");
        }
    }
}
