
namespace BootstrapBlazor.Server.Data;

public class AppHistoryDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { set; get; }
    public string IpAddress { get; set; }
    public string Functions { get; set; }
    public string Operation { get; set; }
    public string FullName { get; set; }
}
public class AppHistorySearchResponseDto
{
    public int TotalItem { set; get; } = 0;
    public List<AppHistoryDto> Result { set; get; } = new List<AppHistoryDto>();
}

 public class AppHistoryFilterPagingDto : BaseFilterPagingDto
{
    public DateTime? Date { set; get; }
    public string? IpAddress { get; set; }
    public string? Functions { get; set; }
    public string? Operation { get; set; }
    public string? FullName { get; set; }
}
