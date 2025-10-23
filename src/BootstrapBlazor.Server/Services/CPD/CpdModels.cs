using BootstrapBlazor.Components;
using BootstrapBlazor.Server.Data;

namespace BootstrapBlazor.Server.Components.Task9.CPD;

// CPD Contract Models
public class CpdImportResponse
{
    public int Inserted { get; set; }
    public int Updated { get; set; }
    public int TotalRows { get; set; }
    public DateOnly DateKey { get; set; }
    public ChangesResponse? Diff { get; set; }
}

public class ChangesResponse
{
    public DateOnly DateKey { get; set; }
    public DateOnly PreviousDate { get; set; }
    public int NewCount { get; set; }
    public int RemovedCount { get; set; }
    public int ChangedCount { get; set; }
    public IEnumerable<ChangeRow> Items { get; set; } = Enumerable.Empty<ChangeRow>();
}

public class ChangeRow
{
    public string ChangeType { get; set; } = string.Empty; // CHANGED|NEW|REMOVED
    public string Publink { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? ChangeFlags { get; set; } // nullable
    public object? ChangedColumns { get; set; } // anonymous object/json
    public int Importance { get; set; }
}

public class CpdSummaryItem
{
    public DateOnly DateKey { get; set; }
    public int New { get; set; }
    public int Removed { get; set; }
    public int Changed { get; set; }
}

public class CpdDaily
{
    public long Id { get; set; }
    public DateOnly DateKey { get; set; }
    public string? Pic { get; set; }
    public string? Publink { get; set; }
    public string? Brand { get; set; }
    public string? Position { get; set; }
    public string? OriginalLink { get; set; }
    public string? ShortLink { get; set; }
    public string? FolderBanner { get; set; }
    public string? FileNameBanner { get; set; }
    public string? XpathLive { get; set; }
    public string? Dimension { get; set; }
    public string? ShortlinkCheck { get; set; }
    public string? Status { get; set; }
    public string? AltStatus { get; set; }
    public string? TitleStatus { get; set; }
    public string? BannerCheckNote { get; set; }
    public string BusinessKeyHash { get; set; } = string.Empty;
    public string ContentHash { get; set; } = string.Empty;
    public Guid ImportBatchId { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Request Models
public class ImportCpdRequest
{
    public DateOnly DateKey { get; set; }
    public byte[] ExcelBytes { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
}

public class CpdFilter
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public string? Pic { get; set; }
    public string? Brand { get; set; }
    public string? Position { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
    
    // Backward compatibility property
    public DateOnly? DateKey 
    { 
        get => FromDate; 
        set => FromDate = ToDate = value; 
    }
}

// Response Models

public class ImportCpdResult
{
    public DateOnly DateKey { get; set; }
    public int TotalRows { get; set; }
    public int Inserted { get; set; }
    public int Updated { get; set; }
    public CpdDiffResult? Diff { get; set; }
}

public class CpdDiffResult
{
    public DateOnly DateKey { get; set; }
    public DateOnly PreviousDate { get; set; }
    public int NewCount { get; set; }
    public int RemovedCount { get; set; }
    public int ChangedCount { get; set; }
    public List<object>? Items { get; set; }
}

public class CpdSummaryResponse
{
    public bool Status { get; set; }
    public string? Message { get; set; }
    public CpdSummaryData? Data { get; set; }
}

public class CpdSummaryData
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public int TotalItems { get; set; }
    public int TotalChanges { get; set; }
    public Dictionary<string, int> ChangesByType { get; set; } = new();
    public Dictionary<string, int> ChangesByPic { get; set; } = new();
}

public class CpdChangesResponse
{
    public bool Status { get; set; }
    public string? Message { get; set; }
    public List<CpdChangeItem>? Data { get; set; }
}

public class CpdChangeItem
{
    public long Id { get; set; }
    public DateOnly DateKey { get; set; }
    public DateOnly PreviousDate { get; set; }
    public string BusinessKeyHash { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public List<string> ChangeFlags { get; set; } = new();
    public int Importance { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object>? ChangedColumns { get; set; }
}

public class CpdChangeDetailResponse
{
    public bool Status { get; set; }
    public string? Message { get; set; }
    public CpdChangeDetail? Data { get; set; }
}

public class CpdChangeDetail
{
    public long Id { get; set; }
    public DateOnly DateKey { get; set; }
    public DateOnly PreviousDate { get; set; }
    public string BusinessKeyHash { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public List<string> ChangeFlags { get; set; } = new();
    public int Importance { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, CpdFieldChange>? ChangedColumns { get; set; }
}

public class CpdFieldChange
{
    public string FieldName { get; set; } = string.Empty;
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
    public string ChangeType { get; set; } = string.Empty;
}

public class CpdItemsResponse
{
    public bool Status { get; set; }
    public string? Message { get; set; }
    public List<CpdItem>? Data { get; set; }
    public int Total { get; set; }
}

public class CpdItem
{
    public long Id { get; set; }
    public DateOnly DateKey { get; set; }
    public string? Pic { get; set; }
    public string? Publink { get; set; }
    public string? Brand { get; set; }
    public string? Position { get; set; }
    public string? OriginalLink { get; set; }
    public string? ShortLink { get; set; }
    public string? FolderBanner { get; set; }
    public string? FileNameBanner { get; set; }
    public string? XpathLive { get; set; }
    public string? Dimension { get; set; }
    public string? ShortlinkCheck { get; set; }
    public string? Status { get; set; }
    public string? AltStatus { get; set; }
    public string? TitleStatus { get; set; }
    public string? BannerCheckNote { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CpdDatesResponse
{
    public bool Status { get; set; }
    public string? Message { get; set; }
    public List<DateOnly>? Data { get; set; }
}

// Report Models
public class CpdReportResponse
{
    public CpdSummaryStats SummaryStats { get; set; } = new();
    public List<CpdChangeTypeItem> ChangesByType { get; set; } = new();
    public List<CpdPicItem> ChangesByPic { get; set; } = new();
    public List<CpdChangeTypeDetail> ChangeTypeDetails { get; set; } = new();
    public List<CpdPicDetail> PicDetails { get; set; } = new();
}

public class CpdSummaryStats
{
    public int TotalItems { get; set; }
    public int TotalChanges { get; set; }
    public int NewItems { get; set; }
    public int DeletedItems { get; set; }
}

public class CpdChangeTypeItem
{
    public string ChangeType { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class CpdPicItem
{
    public string Pic { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class CpdChangeTypeDetail
{
    public string ChangeType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double Percentage { get; set; }
}

public class CpdPicDetail
{
    public string Pic { get; set; } = string.Empty;
    public int ChangeCount { get; set; }
    public double Percentage { get; set; }
}

// Search Filter DTOs
public class CpdSearchFilterDto : ITableSearchModel
{
    public DateTimeRangeValue? DateRange { get; set; }
    public string? Pic { get; set; }
    public string? Brand { get; set; }
    public string? Position { get; set; }

    public IEnumerable<IFilterAction> GetSearches()
    {
        var ret = new List<IFilterAction>();
        return ret;
    }

    public void Reset()
    {
        DateRange = null;
        Pic = null;
        Brand = null;
        Position = null;
    }
}

public class CpdChangesSearchFilterDto : ITableSearchModel
{
    public DateTimeRangeValue? DateRange { get; set; }
    public string? ChangeType { get; set; }
    public string? Importance { get; set; }
    public string? BusinessKeyHash { get; set; }

    public IEnumerable<IFilterAction> GetSearches()
    {
        var ret = new List<IFilterAction>();
        return ret;
    }

    public void Reset()
    {
        DateRange = null;
        ChangeType = null;
        Importance = null;
        BusinessKeyHash = null;
    }
}

