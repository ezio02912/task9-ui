using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;

namespace BootstrapBlazor.Server.Components.Task9.KeywordManager;

public partial class MonthlyMetricsManager : ComponentBase
{
    [Parameter]
    public long KeywordId { get; set; }

    [Inject]
    [NotNull]
    private IKeywordMonthlyMetricService? MetricService { get; set; }

    [Inject]
    [NotNull]
    private ToastService? ToastService { get; set; }

    [Inject]
    [NotNull]
    private DialogService? DialogService { get; set; }

    private List<KeywordMonthlyMetricDto> MonthlyMetrics { get; set; } = new();
    private CreateUpdateKeywordMonthlyMetricDto NewMetric { get; set; } = new();
    private bool IsLoading { get; set; } = false;
    private bool IsAddingMetric { get; set; } = false;

    protected override async Task OnParametersSetAsync()
    {
        if (KeywordId > 0)
        {
            await LoadMetrics();
        }
    }

    private async Task LoadMetrics()
    {
        try
        {
            IsLoading = true;
            var result = await MetricService.GetListAsync(KeywordId);
            
            if (result.Status)
            {
                MonthlyMetrics = result.Data ?? new();
            }
            else
            {
                await ToastService.Error("Lỗi", result.Message ?? "Không thể tải dữ liệu search volume");
            }
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải dữ liệu: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OnAddMetric()
    {
        try
        {
            // Validate
            if (string.IsNullOrWhiteSpace(NewMetric.YearMonth))
            {
                await ToastService.Warning("Cảnh báo", "Vui lòng nhập tháng");
                return;
            }

            if (!Regex.IsMatch(NewMetric.YearMonth, @"^\d{4}-\d{2}$"))
            {
                await ToastService.Warning("Cảnh báo", "Định dạng tháng không hợp lệ. Vui lòng nhập theo định dạng YYYY-MM (ví dụ: 2025-01)");
                return;
            }

            if (NewMetric.SearchVol <= 0)
            {
                await ToastService.Warning("Cảnh báo", "Search volume phải lớn hơn 0");
                return;
            }

            IsAddingMetric = true;
            NewMetric.KeywordId = KeywordId;

            var result = await MetricService.UpsertAsync(NewMetric);
            
            if (result.Status)
            {
                await ToastService.Success("Thành công", "Đã thêm/cập nhật search volume");
                
                // Reset form
                NewMetric = new CreateUpdateKeywordMonthlyMetricDto();
                
                // Reload metrics
                await LoadMetrics();
            }
            else
            {
                await ToastService.Error("Lỗi", result.Message ?? "Không thể thêm search volume");
            }
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Có lỗi xảy ra: {ex.Message}");
        }
        finally
        {
            IsAddingMetric = false;
        }
    }

    private async Task OnDeleteMetric(KeywordMonthlyMetricDto metric)
    {
        var confirmed = await DialogService!.ShowModal("Xác nhận xóa", 
            $"Bạn có chắc chắn muốn xóa search volume tháng {metric.YearMonth}?",
            new ResultDialogOption()
            {
                Size = Size.Small
            });

        if (confirmed == DialogResult.Yes)
        {
            try
            {
                var result = await MetricService.DeleteAsync(metric.KeywordId, metric.YearMonth);
                
                if (result.Status && result.Data)
                {
                    await ToastService.Success("Thành công", "Đã xóa search volume");
                    await LoadMetrics();
                }
                else
                {
                    await ToastService.Error("Lỗi", result.Message ?? "Không thể xóa search volume");
                }
            }
            catch (Exception ex)
            {
                await ToastService.Error("Lỗi", $"Không thể xóa: {ex.Message}");
            }
        }
    }
}

