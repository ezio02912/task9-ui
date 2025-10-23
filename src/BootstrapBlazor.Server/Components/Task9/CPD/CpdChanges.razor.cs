using BootstrapBlazor.Components;
using BootstrapBlazor.Server.Components.Task9.CPD;

namespace BootstrapBlazor.Server.Components.Task9.CPD;

/// <summary>
/// Component for viewing CPD changes with filtering and pagination
/// </summary>
public partial class CpdChanges : ComponentBase, IDisposable
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
    private List<ChangeRow> Datas = new();
    private List<ChangeRow> DataFilters = new();
    private Table<ChangeRow>? TableRef { get; set; }
    private ChangeRow? SelectedChangeItem { get; set; }
    private Modal? ChangeDetailsModal { get; set; }
    private ITableSearchModel CustomerSearchModel { get; set; } = new CpdChangesSearchFilterDto();
    private int TotalItems { get; set; } = 0;
    
    // Filter options
    private List<SelectedItem> ChangeTypeItems { get; set; } = new()
    {
        new("NEW", "Mới"),
        new("CHANGED", "Thay đổi"),
        new("REMOVED", "Bị xóa")
    };
    
    private List<SelectedItem> ImportanceItems { get; set; } = new()
    {
        new("1", "Thông thường"),
        new("2", "Quan trọng"),
        new("3", "Rất quan trọng")
    };
    #endregion

    /// <summary>
    /// Initialize component
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        base.OnInitialized();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var result = await CpdService!.GetChangesAsync(today, today);
            
            if (result.IsSuccess && result.Data != null)
            {
                Datas = result.Data.Items.ToList();
                TotalItems = result.Data.Items.Count();
            }
        }
        catch (Exception ex)
        {
            await Toast!.Error("Lỗi", $"Không thể tải dữ liệu: {ex.Message}");
        }
    }

    private async Task<QueryData<ChangeRow>> OnQueryFilterAsync(QueryPageOptions options)
    {
        await Task.Delay(200);
        
        try
        {
            var filterModel = CustomerSearchModel as CpdChangesSearchFilterDto;
            var fromDate = filterModel?.DateRange?.Start ?? DateTime.Today;
            var toDate = filterModel?.DateRange?.End ?? fromDate;
            
            var result = await CpdService!.GetChangesAsync(
                DateOnly.FromDateTime(fromDate), 
                DateOnly.FromDateTime(toDate),
                filterModel?.ChangeType,
                filterModel?.Importance,
                filterModel?.BusinessKeyHash);

            if (result != null && result.IsSuccess && result.Data != null)
            {
                var filteredData = result.Data.Items.AsQueryable();
                
                // Apply filters
                if (!string.IsNullOrEmpty(filterModel?.ChangeType))
                {
                    filteredData = filteredData.Where(x => x.ChangeType == filterModel.ChangeType);
                }
                
                if (!string.IsNullOrEmpty(filterModel?.Importance))
                {
                    var importance = int.Parse(filterModel.Importance);
                    filteredData = filteredData.Where(x => x.Importance == importance);
                }
                
                if (!string.IsNullOrEmpty(filterModel?.BusinessKeyHash))
                {
                    filteredData = filteredData.Where(x => x.Publink.Contains(filterModel.BusinessKeyHash));
                }
                
                // Apply pagination
                var pagedData = filteredData
                    .Skip((options.PageIndex - 1) * options.PageItems)
                    .Take(options.PageItems)
                    .ToList();
                
                DataFilters = pagedData;
                TotalItems = filteredData.Count();
            }
            else
            {
                DataFilters = new List<ChangeRow>();
                TotalItems = 0;
            }
        }
        catch (Exception ex)
        {
            DataFilters = new List<ChangeRow>();
            TotalItems = 0;
            await Toast!.Error("Lỗi", $"Không thể tải dữ liệu: {ex.Message}");
        }

        return new QueryData<ChangeRow>()
        {
            Items = DataFilters,
            TotalCount = TotalItems
        };
    }

    private async Task OnTriggerFilterQueryAsync()
    {
        await Task.Delay(100);
        await TableRef!.QueryAsync();
        StateHasChanged();
    }

    private async Task OnResetFilter()
    {
        CustomerSearchModel = new CpdChangesSearchFilterDto();
        await TableRef!.QueryAsync();
        StateHasChanged();
    }

    private async Task ShowChangeDetails(ChangeRow item)
    {
        SelectedChangeItem = item;
        await ChangeDetailsModal!.Show();
    }

    private async Task CloseChangeDetailsModal()
    {
        await ChangeDetailsModal!.Close();
        SelectedChangeItem = null;
    }

    private Color GetChangeTypeColor(string changeType)
    {
        return changeType.ToUpperInvariant() switch
        {
            "NEW" => Color.Success,
            "CHANGED" => Color.Warning,
            "REMOVED" => Color.Danger,
            _ => Color.Secondary
        };
    }

    private string GetChangeTypeText(string changeType)
    {
        return changeType.ToUpperInvariant() switch
        {
            "NEW" => "Mới",
            "CHANGED" => "Thay đổi",
            "REMOVED" => "Bị xóa",
            _ => changeType
        };
    }

    private Color GetImportanceColor(int importance)
    {
        return importance switch
        {
            3 => Color.Danger,
            2 => Color.Warning,
            1 => Color.Info,
            _ => Color.Secondary
        };
    }

    private string GetImportanceText(int importance)
    {
        return importance switch
        {
            3 => "Rất quan trọng",
            2 => "Quan trọng",
            1 => "Thông thường",
            _ => "Không xác định"
        };
    }

    private string GetChangeFlagText(string flag)
    {
        return flag switch
        {
            "ORIGINAL_LINK" => "Original Link",
            "SHORT_LINK" => "Short Link",
            "FILE_NAME_BANNER" => "File Banner",
            "DIMENSION" => "Kích thước",
            "XPATH" => "XPath",
            "STATUS" => "Trạng thái",
            "ALT" => "Alt",
            "TITLE" => "Title",
            "BANNER_NOTE" => "Ghi chú Banner",
            _ => flag
        };
    }

    /// <summary>
    /// Dispose component resources
    /// </summary>
    public void Dispose()
    {
        // Cleanup if needed
    }
}
