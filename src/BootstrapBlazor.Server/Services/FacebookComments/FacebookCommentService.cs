
using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services
{
    public interface IFacebookCommentService
    {
        Task<ApiResponseBase<List<FacebookCommentDto>>> GetListAsync(BaseFilterPagingDto filter);   
        Task<ApiResponseBase<FacebookCommentDto>> GetByIdAsync(int id);
        Task<ApiResponseBase<bool>> UpdateAsync(int id, FacebookCommentDto dto);
        Task<ApiResponseBase<bool>> DeleteAsync(int id);
        Task<byte[]> ExportExcelAsync(BaseFilterPagingDto filter);
    }

    public class FacebookCommentService : IFacebookCommentService
    {
        public async Task<ApiResponseBase<List<FacebookCommentDto>>> GetListAsync(BaseFilterPagingDto filter)
        {
            return await RequestClient.PostAPIAsync<ApiResponseBase<List<FacebookCommentDto>>>("facebook-comment/get-list", filter);
        }

        public async Task<ApiResponseBase<FacebookCommentDto>> GetByIdAsync(int id)
        {
            return await RequestClient.GetAPIAsync<ApiResponseBase<FacebookCommentDto>>($"facebook-comment/get-by-id/{id}");
        }


        public async Task<ApiResponseBase<bool>> UpdateAsync(int id, FacebookCommentDto dto)
        {
            return await RequestClient.PutAPIAsync<ApiResponseBase<bool>>($"facebook-comment/update/{id}", dto);
        }

        public async Task<ApiResponseBase<bool>> DeleteAsync(int id)
        {
            return await RequestClient.DeleteAPIAsync<ApiResponseBase<bool>>($"facebook-comment/delete/{id}");
        }
        public async Task<byte[]> ExportExcelAsync(BaseFilterPagingDto filter)
        {
            return await RequestClient.PostAPIBytesAsync("facebook-comment/export-excel", filter);
        }
    }
}
