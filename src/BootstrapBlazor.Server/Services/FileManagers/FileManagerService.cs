using Microsoft.AspNetCore.Components.Forms;
using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services
{
    public interface IFileManagerService
    {
        Task<ApiResponseBase<List<string>>> ListFoldersAsync(string folder);
        Task<ApiResponseBase<List<FileInfoDto>>> ListFileInFolderAsync(string folder);
        Task<ApiResponseBase<bool>> UploadFileAsync(string folder, IBrowserFile fileStream);
        Task<ApiResponseBase<bool>> DeleteFileAsync(string fileName);
    }
    public class FileManagerService : IFileManagerService
    {

        public async Task<ApiResponseBase<List<string>>> ListFoldersAsync(string folder)
        {
            return await RequestClient.GetAPIAsync<ApiResponseBase<List<string>>>($"file/list-folders?folder={folder}");
        }
        public async Task<ApiResponseBase<List<FileInfoDto>>> ListFileInFolderAsync(string folder)
        {
            return await RequestClient.GetAPIAsync<ApiResponseBase<List<FileInfoDto>>>($"file/list?folder={folder}");
        }
        public async Task<ApiResponseBase<bool>> UploadFileAsync(string folder, IBrowserFile fileStream)
        {
            return await RequestClient.PostAPIWithFileAsync<ApiResponseBase<bool>>($"file/upload?folder={folder}", fileStream);
        }

        public async Task<ApiResponseBase<bool>> DeleteFileAsync(string fileName)
        {
            return await RequestClient.DeleteAPIAsync<ApiResponseBase<bool>>($"file/delete?fileName={fileName}");
        }
    }
}
