using BootstrapBlazor.Components;
using BootstrapBlazor.Server.Components.Task9.CPD;

namespace BootstrapBlazor.Server.Components.Task9.CPD;

/// <summary>
/// Component for viewing CPD data with filtering and pagination
/// </summary>
public partial class CpdDataView : ComponentBase, IDisposable
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
    private List<CpdDaily> Datas = new();
    private List<CpdDaily> DataFilters = new();
    private Table<CpdDaily>? TableRef { get; set; }
    private CpdFilter Filter { get; set; } = new();
    private ITableSearchModel CustomerSearchModel { get; set; } = new CpdSearchFilterDto();
    private int TotalItems { get; set; } = 0;
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
            var result = await CpdService!.GetItemsAsync(new CpdFilter
            {
                FromDate = DateOnly.FromDateTime(DateTime.Today),
                ToDate = DateOnly.FromDateTime(DateTime.Today),
                Skip = 0,
                Take = 20
            });
            
            if (result.IsSuccess && result.Data != null)
            {
                Datas = result.Data.ToList();
                TotalItems = result.TotalItems ?? 0;
            }
        }
        catch (Exception ex)
        {
            await Toast!.Error("Lỗi", $"Không thể tải dữ liệu: {ex.Message}");
        }
    }

    private async Task<QueryData<CpdDaily>> OnQueryFilterAsync(QueryPageOptions options)
    {
        await Task.Delay(200);
        
        try
        {
            var filterModel = CustomerSearchModel as CpdSearchFilterDto;
            
            // Determine date range
            DateOnly? fromDate = null;
            DateOnly? toDate = null;
            
            if (filterModel?.DateRange?.Start != null)
            {
                fromDate = DateOnly.FromDateTime(filterModel.DateRange.Start);
                
                // If End date is provided and different from Start, use it as toDate
                if (filterModel.DateRange.End != null)
                {
                    toDate = DateOnly.FromDateTime(filterModel.DateRange.End);
                }
                else
                {
                    // Only start date provided, use it as both from and to
                    toDate = fromDate;
                }
            }
            
            var filter = new CpdFilter
            {
                Skip = (options.PageIndex - 1) * options.PageItems,
                Take = options.PageItems,
                FromDate = fromDate,
                ToDate = toDate,
                Pic = filterModel?.Pic,
                Brand = filterModel?.Brand,
                Position = filterModel?.Position
            };

            var result = await CpdService!.GetItemsAsync(filter);

            if (result != null && result.IsSuccess)
            {
                DataFilters = result.Data?.ToList() ?? new List<CpdDaily>();
                TotalItems = result.Total ?? 0;
            }
            else
            {
                DataFilters = new List<CpdDaily>();
                TotalItems = 0;
            }
        }
        catch (Exception ex)
        {
            DataFilters = new List<CpdDaily>();
            TotalItems = 0;
            await Toast!.Error("Lỗi", $"Không thể tải dữ liệu: {ex.Message}");
        }

        return new QueryData<CpdDaily>()
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
        CustomerSearchModel = new CpdSearchFilterDto();
        await TableRef!.QueryAsync();
        StateHasChanged();
    }

    private Color GetStatusColor(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "ok" or "success" or "active" => Color.Success,
            "error" or "failed" or "inactive" => Color.Danger,
            "warning" or "pending" => Color.Warning,
            "info" or "processing" => Color.Info,
            _ => Color.Secondary
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
