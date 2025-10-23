using BootstrapBlazor.Components;
using BootstrapBlazor.Server.Data;
using BootstrapBlazor.Server.Components.Task9.CPD;

namespace BootstrapBlazor.Server.Components.Task9.CPD;

/// <summary>
/// Component for uploading CPD Excel files
/// </summary>
public partial class CpdUpload : ComponentBase
{
    #region Inject
    [Inject]
    [NotNull]
    private ToastService? Toast { get; set; }
    
    [Inject]
    [NotNull]
    private BootstrapBlazor.Server.Services.CPD.ICpdService? CpdService { get; set; }
    #endregion

    #region Properties
    private DateTime DateKey { get; set; } = DateTime.Today;
    private string FileName { get; set; } = string.Empty;
    private bool IsUploading { get; set; } = false;
    private CpdImportResponse? LastImportResult { get; set; }
    private List<string> ImportErrors { get; set; } = new();
    #endregion

    /// <summary>
    /// Initialize component
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    private async Task OnDropCpdUpload(UploadFile file)
    {
        if (file?.File == null)
        {
            await Toast!.Error("Lỗi", "Vui lòng chọn file để tải lên");
            return;
        }

        // Validate file size (max 20MB)
        if (file.File.Size > 20 * 1024 * 1024)
        {
            file.Code = 1004;
            await Toast!.Information("Cảnh báo", "File không được vượt quá 20MB");
            return;
        }

        // Validate file extension
        var extension = Path.GetExtension(file.File.Name).ToLowerInvariant();
        if (extension != ".xlsx")
        {
            await Toast!.Error("Lỗi", "Chỉ chấp nhận file Excel (.xlsx)");
            return;
        }

        IsUploading = true;
        ImportErrors.Clear();
        LastImportResult = null;
        StateHasChanged();

        try
        {
            using var stream = new MemoryStream();
            await file.File.OpenReadStream(maxAllowedSize: 20 * 1024 * 1024).CopyToAsync(stream);
            var excelBytes = stream.ToArray();

            if (excelBytes.Length == 0)
            {
                await Toast!.Error("Lỗi", "File rỗng hoặc không đọc được");
                return;
            }

            // Create import request
            var importRequest = new ImportCpdRequest
            {
                DateKey = DateOnly.FromDateTime(DateKey),
                ExcelBytes = excelBytes,
                FileName = string.IsNullOrEmpty(FileName) ? file.File.Name : FileName
            };

            // Call CPD service to import
            var result = await CpdService!.ImportAsync(importRequest);
            
            if (result.IsSuccess)
            {
                LastImportResult = result.Data;
                await Toast!.Success("Thành công", $"Import thành công! Tổng {result.Data?.TotalRows} dòng dữ liệu");
                
                // Reset form
                FileName = string.Empty;
            }
            else
            {
                ImportErrors = new List<string> { result.Message ?? "Có lỗi xảy ra khi import" };
                await Toast!.Error("Lỗi", $"Import thất bại: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            ImportErrors = new List<string> { ex.Message };
            await Toast!.Error("Lỗi", $"Có lỗi xảy ra: {ex.Message}");
        }
        finally
        {
            IsUploading = false;
            StateHasChanged();
        }
    }
}
