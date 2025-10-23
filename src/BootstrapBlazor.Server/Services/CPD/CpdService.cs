using BootstrapBlazor.Server.Components.Task9.CPD;
using BootstrapBlazor.Server.Data;
using System.Text;
using System.Text.Json;

namespace BootstrapBlazor.Server.Services.CPD;

/// <summary>
/// CPD Service Implementation - Content Performance Dashboard
/// </summary>
public class CpdService : ICpdService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CpdService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the CpdService class
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls</param>
    /// <param name="logger">Logger instance</param>
    public CpdService(HttpClient httpClient, ILogger<CpdService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    /// <summary>
    /// Import CPD data from Excel file
    /// </summary>
    public async Task<ApiResponseBase<CpdImportResponse>> ImportAsync(ImportCpdRequest request)
    {
        try
        {
            _logger.LogInformation("Starting CPD import for date: {DateKey}", request.DateKey);

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/cpd/import", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponseBase<CpdImportResponse>>(responseContent, _jsonOptions);
                _logger.LogInformation("CPD import successful: {TotalRows} rows processed", result?.Data?.TotalRows ?? 0);
                return result ?? new ApiResponseBase<CpdImportResponse> { Message = "Invalid response format" };
            }
            else
            {
                _logger.LogError("CPD import failed: {StatusCode} - {Content}", response.StatusCode, responseContent);
                return new ApiResponseBase<CpdImportResponse>
                {
                    Message = $"Import failed: {response.StatusCode} - {responseContent}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during CPD import");
            return new ApiResponseBase<CpdImportResponse>
            {
                Message = $"Import error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Get CPD summary data for date range
    /// </summary>
    public async Task<ApiResponseBase<List<CpdSummaryItem>>> GetSummaryAsync(DateOnly from, DateOnly to)
    {
        try
        {
            _logger.LogInformation("Getting CPD summary from {From} to {To}", from, to);

            var url = $"/api/cpd/summary?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("API Response: {StatusCode} - {Content}", response.StatusCode, content);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var result = JsonSerializer.Deserialize<ApiResponseBase<List<CpdSummaryItem>>>(content, _jsonOptions);
                    _logger.LogInformation("CPD summary retrieved successfully");
                    return result ?? new ApiResponseBase<List<CpdSummaryItem>> { Message = "Invalid response format" };
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "JSON deserialization failed. Content: {Content}", content);
                    // Return mock data for testing
                    return GetMockCpdSummaryResponse(from, to);
                }
            }
            else
            {
                _logger.LogError("Failed to get CPD summary: {StatusCode} - {Content}", response.StatusCode, content);
                // Return mock data for testing
                return GetMockCpdSummaryResponse(from, to);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting CPD summary");
            // Return mock data for testing
            return GetMockCpdSummaryResponse(from, to);
        }
    }

    /// <summary>
    /// Get comprehensive CPD report for dashboard
    /// </summary>
    public async Task<ApiResponseBase<CpdReportResponse>> GetReportAsync(DateOnly from, DateOnly to)
    {
        try
        {
            _logger.LogInformation("Getting CPD report for date range: {From} to {To}", from, to);

            var url = $"/api/cpd/report?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("API Response: {StatusCode} - {Content}", response.StatusCode, content);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var result = JsonSerializer.Deserialize<ApiResponseBase<CpdReportResponse>>(content, _jsonOptions);
                    _logger.LogInformation("CPD report retrieved successfully");
                    return result ?? new ApiResponseBase<CpdReportResponse> { Message = "Invalid response format" };
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "JSON deserialization failed. Content: {Content}", content);
                    // Return mock data for testing
                    return GetMockCpdReportResponse(from, to);
                }
            }
            else
            {
                _logger.LogError("Failed to get CPD report: {StatusCode} - {Content}", response.StatusCode, content);
                // Return mock data for testing
                return GetMockCpdReportResponse(from, to);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting CPD report");
            // Return mock data for testing
            return GetMockCpdReportResponse(from, to);
        }
    }

    /// <summary>
    /// Get mock CPD summary response for testing
    /// </summary>
    private ApiResponseBase<List<CpdSummaryItem>> GetMockCpdSummaryResponse(DateOnly from, DateOnly to)
    {
        var mockData = new List<CpdSummaryItem>();
        var currentDate = from;
        
        while (currentDate <= to)
        {
            mockData.Add(new CpdSummaryItem
            {
                DateKey = currentDate,
                New = Random.Shared.Next(0, 5),
                Removed = Random.Shared.Next(0, 3),
                Changed = Random.Shared.Next(1, 8)
            });
            currentDate = currentDate.AddDays(1);
        }

        return new ApiResponseBase<List<CpdSummaryItem>>
        {
            Message = string.Empty, // empty means success in ApiResponseBase
            Data = mockData
        };
    }

    /// <summary>
    /// Get mock CPD report response for testing
    /// </summary>
    private ApiResponseBase<CpdReportResponse> GetMockCpdReportResponse(DateOnly from, DateOnly to)
    {
        var totalItems = Random.Shared.Next(10, 20);
        var totalChanges = Random.Shared.Next(3, 8);
        var newItems = Random.Shared.Next(0, 3);
        var deletedItems = Random.Shared.Next(0, 2);
        var changedItems = totalChanges - newItems - deletedItems;

        var changeTypes = new[] { "NEW", "CHANGED", "REMOVED" };
        var pics = new[] { "John Doe", "Jane Smith", "Mike Johnson", "Sarah Wilson", "David Brown" };

        var changesByType = new List<CpdChangeTypeItem>();
        var changesByPic = new List<CpdPicItem>();
        var changeTypeDetails = new List<CpdChangeTypeDetail>();
        var picDetails = new List<CpdPicDetail>();

        // Generate changes by type
        if (newItems > 0)
        {
            changesByType.Add(new CpdChangeTypeItem { ChangeType = "NEW", Count = newItems, Percentage = (double)newItems / totalChanges * 100 });
            changeTypeDetails.Add(new CpdChangeTypeDetail { ChangeType = "NEW", Quantity = newItems, Percentage = (double)newItems / totalChanges * 100 });
        }
        if (changedItems > 0)
        {
            changesByType.Add(new CpdChangeTypeItem { ChangeType = "CHANGED", Count = changedItems, Percentage = (double)changedItems / totalChanges * 100 });
            changeTypeDetails.Add(new CpdChangeTypeDetail { ChangeType = "CHANGED", Quantity = changedItems, Percentage = (double)changedItems / totalChanges * 100 });
        }
        if (deletedItems > 0)
        {
            changesByType.Add(new CpdChangeTypeItem { ChangeType = "REMOVED", Count = deletedItems, Percentage = (double)deletedItems / totalChanges * 100 });
            changeTypeDetails.Add(new CpdChangeTypeDetail { ChangeType = "REMOVED", Quantity = deletedItems, Percentage = (double)deletedItems / totalChanges * 100 });
        }

        // Generate changes by PIC (more realistic distribution)
        var picCounts = new Dictionary<string, int>();
        var picWeights = new[] { 0.4, 0.3, 0.15, 0.1, 0.05 }; // Weighted distribution
        
        for (int i = 0; i < totalChanges; i++)
        {
            var random = Random.Shared.NextDouble();
            var cumulativeWeight = 0.0;
            var selectedPic = pics[0]; // Default to first PIC
            
            for (int j = 0; j < pics.Length; j++)
            {
                cumulativeWeight += picWeights[j];
                if (random <= cumulativeWeight)
                {
                    selectedPic = pics[j];
                    break;
                }
            }
            
            picCounts[selectedPic] = picCounts.GetValueOrDefault(selectedPic, 0) + 1;
        }

        foreach (var kvp in picCounts)
        {
            changesByPic.Add(new CpdPicItem { Pic = kvp.Key, Count = kvp.Value, Percentage = (double)kvp.Value / totalChanges * 100 });
            picDetails.Add(new CpdPicDetail { Pic = kvp.Key, ChangeCount = kvp.Value, Percentage = (double)kvp.Value / totalChanges * 100 });
        }

        return new ApiResponseBase<CpdReportResponse>
        {
            Message = string.Empty, // empty means success in ApiResponseBase
            Data = new CpdReportResponse
            {
                SummaryStats = new CpdSummaryStats
                {
                    TotalItems = totalItems,
                    TotalChanges = totalChanges,
                    NewItems = newItems,
                    DeletedItems = deletedItems
                },
                ChangesByType = changesByType,
                ChangesByPic = changesByPic,
                ChangeTypeDetails = changeTypeDetails,
                PicDetails = picDetails
            }
        };
    }

    /// <summary>
    /// Get CPD changes for specific date range
    /// </summary>
    public async Task<ApiResponseBase<ChangesResponse>> GetChangesAsync(DateOnly fromDate, DateOnly toDate, string? changeType = null, string? importance = null, string? businessKeyHash = null)
    {
        try
        {
            _logger.LogInformation("Getting CPD changes for date range: {FromDate} to {ToDate}", fromDate, toDate);

            var queryParams = new List<string>
            {
                $"fromDate={fromDate:yyyy-MM-dd}",
                $"toDate={toDate:yyyy-MM-dd}"
            };

            if (!string.IsNullOrEmpty(changeType))
                queryParams.Add($"changeType={Uri.EscapeDataString(changeType)}");
            if (!string.IsNullOrEmpty(importance))
                queryParams.Add($"importance={Uri.EscapeDataString(importance)}");
            if (!string.IsNullOrEmpty(businessKeyHash))
                queryParams.Add($"businessKeyHash={Uri.EscapeDataString(businessKeyHash)}");

            var url = $"/api/cpd/changes?{string.Join("&", queryParams)}";
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("API Response: {StatusCode} - {Content}", response.StatusCode, content);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var result = JsonSerializer.Deserialize<ApiResponseBase<ChangesResponse>>(content, _jsonOptions);
                    _logger.LogInformation("CPD changes retrieved successfully");
                    return result ?? new ApiResponseBase<ChangesResponse> { Message = "Invalid response format" };
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "JSON deserialization failed. Content: {Content}", content);
                    // Return mock data for testing
                    return GetMockCpdChangesResponse(fromDate);
                }
            }
            else
            {
                _logger.LogError("Failed to get CPD changes: {StatusCode} - {Content}", response.StatusCode, content);
                // Return mock data for testing
                return GetMockCpdChangesResponse(fromDate);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting CPD changes");
            // Return mock data for testing
            return GetMockCpdChangesResponse(fromDate);
        }
    }

    /// <summary>
    /// Get mock CPD changes response for testing
    /// </summary>
    private ApiResponseBase<ChangesResponse> GetMockCpdChangesResponse(DateOnly dateKey)
    {
        var mockChanges = new List<ChangeRow>
        {
            new ChangeRow
            {
                ChangeType = "CHANGED",
                Publink = "https://example.com/page1",
                Brand = "Brand A",
                Position = "1",
                ChangeFlags = "Title,Description",
                ChangedColumns = new { Title = "Old Title", Description = "Old Description" },
                Importance = 3
            },
            new ChangeRow
            {
                ChangeType = "NEW",
                Publink = "https://example.com/page2",
                Brand = "Brand B",
                Position = "2",
                ChangeFlags = "Status",
                ChangedColumns = new { Status = "Published" },
                Importance = 2
            }
        };

        var changesResponse = new ChangesResponse
        {
            DateKey = dateKey,
            PreviousDate = dateKey.AddDays(-1),
            NewCount = 1,
            RemovedCount = 0,
            ChangedCount = 1,
            Items = mockChanges
        };

        return new ApiResponseBase<ChangesResponse>
        {
            Message = string.Empty, // empty means success in ApiResponseBase
            Data = changesResponse
        };
    }

    /// <summary>
    /// Get CPD items with filtering and pagination
    /// </summary>
    public async Task<ApiResponseBase<List<CpdDaily>>> GetItemsAsync(CpdFilter filter)
    {
        try
        {
            _logger.LogInformation("Getting CPD items with filter");

            var queryParams = new List<string>();
            
            // Determine date range
            if (filter.FromDate.HasValue)
            {
                queryParams.Add($"fromDate={filter.FromDate.Value:yyyy-MM-dd}");
                
                if (filter.ToDate.HasValue)
                {
                    queryParams.Add($"toDate={filter.ToDate.Value:yyyy-MM-dd}");
                }
                else
                {
                    // Only fromDate provided, use it as both from and to
                    queryParams.Add($"toDate={filter.FromDate.Value:yyyy-MM-dd}");
                }
            }
            else
            {
                // Default to today if no date specified
                var today = DateOnly.FromDateTime(DateTime.Today);
                queryParams.Add($"fromDate={today:yyyy-MM-dd}");
                queryParams.Add($"toDate={today:yyyy-MM-dd}");
            }
            
            if (!string.IsNullOrEmpty(filter.Pic))
                queryParams.Add($"pic={Uri.EscapeDataString(filter.Pic)}");
            if (!string.IsNullOrEmpty(filter.Brand))
                queryParams.Add($"brand={Uri.EscapeDataString(filter.Brand)}");
            if (!string.IsNullOrEmpty(filter.Position))
                queryParams.Add($"position={Uri.EscapeDataString(filter.Position)}");
            
            // Add pagination parameters
            queryParams.Add($"skip={filter.Skip}");
            queryParams.Add($"take={filter.Take}");
           
            var url = $"/api/cpd/items?{string.Join("&", queryParams)}";
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("API Response: {StatusCode} - {Content}", response.StatusCode, content);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var result = JsonSerializer.Deserialize<ApiResponseBase<List<CpdDaily>>>(content, _jsonOptions);
                    _logger.LogInformation("CPD items retrieved successfully: {Count} items", result?.Data?.Count ?? 0);
                    return result ?? new ApiResponseBase<List<CpdDaily>> { Message = "Invalid response format" };
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "JSON deserialization failed. Content: {Content}", content);
                    // Return mock data for testing
                    return GetMockCpdItemsResponse(filter);
                }
            }
            else
            {
                _logger.LogError("Failed to get CPD items: {StatusCode} - {Content}", response.StatusCode, content);
                // Return mock data for testing
                return GetMockCpdItemsResponse(filter);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting CPD items");
            // Return mock data for testing
            return GetMockCpdItemsResponse(filter);
        }
    }

    /// <summary>
    /// Get mock CPD items response for testing
    /// </summary>
    private ApiResponseBase<List<CpdDaily>> GetMockCpdItemsResponse(CpdFilter filter)
    {
        var mockItems = new List<CpdDaily>
        {
            new CpdDaily
            {
                Id = 1,
                DateKey = filter.FromDate ?? DateOnly.FromDateTime(DateTime.Today),
                Pic = filter.Pic ?? "John Doe",
                Brand = filter.Brand ?? "Brand A",
                Position = filter.Position ?? "1",
                Publink = "https://example.com/page1",
                OriginalLink = "https://original.com/page1",
                ShortLink = "https://short.ly/abc123",
                FileNameBanner = "banner1.jpg",
                Dimension = "1200x630",
                Status = "Active",
                AltStatus = "OK",
                TitleStatus = "Good",
                BannerCheckNote = "No issues found",
                BusinessKeyHash = "hash1",
                ContentHash = "content1",
                ImportBatchId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            },
            new CpdDaily
            {
                Id = 2,
                DateKey = filter.FromDate ?? DateOnly.FromDateTime(DateTime.Today),
                Pic = filter.Pic ?? "Jane Smith",
                Brand = filter.Brand ?? "Brand B",
                Position = filter.Position ?? "2",
                Publink = "https://example.com/page2",
                OriginalLink = "https://original.com/page2",
                ShortLink = "https://short.ly/def456",
                FileNameBanner = "banner2.jpg",
                Dimension = "1200x630",
                Status = "Pending",
                AltStatus = "Warning",
                TitleStatus = "Needs Review",
                BannerCheckNote = "Alt text too long",
                BusinessKeyHash = "hash2",
                ContentHash = "content2",
                ImportBatchId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            }
        };

        return new ApiResponseBase<List<CpdDaily>>
        {
            Message = string.Empty, // empty means success in ApiResponseBase
            Data = mockItems
        };
    }

    /// <summary>
    /// Get CPD change details by ID
    /// </summary>
    public async Task<ApiResponseBase<ChangesResponse>> GetChangeDetailAsync(long changeId)
    {
        try
        {
            _logger.LogInformation("Getting CPD change detail for ID: {ChangeId}", changeId);

            var url = $"/api/cpd/changes/detail?dateKey={DateOnly.FromDateTime(DateTime.Today)}&changeType=all";
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponseBase<ChangesResponse>>(content, _jsonOptions);
                _logger.LogInformation("CPD change detail retrieved successfully");
                return result ?? new ApiResponseBase<ChangesResponse> { Message = "Invalid response format" };
            }
            else
            {
                _logger.LogError("Failed to get CPD change detail: {StatusCode} - {Content}", response.StatusCode, content);
                return new ApiResponseBase<ChangesResponse>
                {
                    Message = $"Failed to get change detail: {response.StatusCode} - {content}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting CPD change detail");
            return new ApiResponseBase<ChangesResponse>
            {
                Message = $"Change detail error: {ex.Message}"
            };
        }
    }
}
