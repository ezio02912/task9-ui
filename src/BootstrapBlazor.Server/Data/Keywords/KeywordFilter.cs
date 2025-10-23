namespace BootstrapBlazor.Server.Data;
public class KeywordFilter : FilterPagingBase
{
    public List<int>? BrandIds { get; set; }
    public string? KeywordText { get; set; }
    public bool OnlyWithUsers { get; set; } = false; // true: chỉ show keyword có người phụ trách, false/null: show tất cả
    public int? UserId { get; set; } // Filter theo user cụ thể đang phụ trách keyword
}
