using BootstrapBlazor.Server.Components.Task9.CPD;
using BootstrapBlazor.Server.Data;

namespace BootstrapBlazor.Server.Services.CPD;

/// <summary>
/// CPD Service Interface - Content Performance Dashboard
/// </summary>
public interface ICpdService
{
    /// <summary>
    /// Import CPD data from Excel file
    /// </summary>
    /// <param name="request">Import request with Excel data</param>
    /// <returns>Import result with statistics</returns>
    Task<ApiResponseBase<CpdImportResponse>> ImportAsync(ImportCpdRequest request);

    /// <summary>
    /// Get CPD summary data for date range
    /// </summary>
    /// <param name="from">Start date</param>
    /// <param name="to">End date</param>
    /// <returns>Summary data with statistics</returns>
    Task<ApiResponseBase<List<CpdSummaryItem>>> GetSummaryAsync(DateOnly from, DateOnly to);

    /// <summary>
    /// Get comprehensive CPD report for dashboard
    /// </summary>
    /// <param name="from">Start date</param>
    /// <param name="to">End date</param>
    /// <returns>Complete report data</returns>
    Task<ApiResponseBase<CpdReportResponse>> GetReportAsync(DateOnly from, DateOnly to);

    /// <summary>
    /// Get CPD changes for specific date range
    /// </summary>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    /// <param name="changeType">Optional change type filter</param>
    /// <param name="importance">Optional importance filter</param>
    /// <param name="businessKeyHash">Optional business key hash filter</param>
    /// <returns>List of changes</returns>
    Task<ApiResponseBase<ChangesResponse>> GetChangesAsync(DateOnly fromDate, DateOnly toDate, string? changeType = null, string? importance = null, string? businessKeyHash = null);

    /// <summary>
    /// Get CPD items with filtering and pagination
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <returns>Paginated list of CPD items</returns>
    Task<ApiResponseBase<List<CpdDaily>>> GetItemsAsync(CpdFilter filter);

    /// <summary>
    /// Get CPD change details by ID
    /// </summary>
    /// <param name="changeId">Change ID</param>
    /// <returns>Change details</returns>
    Task<ApiResponseBase<ChangesResponse>> GetChangeDetailAsync(long changeId);

    /// <summary>
    /// Get available dates for CPD data
    /// </summary>
    /// <returns>List of available dates</returns>
}
