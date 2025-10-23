namespace BootstrapBlazor.Server.Data;

public class FilterBaseData
{
    public string? Text { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public DateTime? StartDay { get; set;}
    public DateTime? EndDay { get; set; }
}
public class FilterPagingBase : FilterBaseData
{
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 10;
    public string? Sort { set; get; }
}



public class BaseFilterPagingDto
{
    public string? FilterText { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? No { get; set; }
    public string? Code { get; set; }
    public string? UserFullName { get; set; }
    public string? UserCode { get; set; }
    public int? IntId { get; set; }
    public bool? IsActive { get; set; }
    public int? GuidId { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 10;
    public string? Sort { set; get; }
    public DateTime FromDate { get; set; } = DateTime.Now.AddDays(-30);
    public DateTime ToDate { get; set; } = DateTime.Now;
}
