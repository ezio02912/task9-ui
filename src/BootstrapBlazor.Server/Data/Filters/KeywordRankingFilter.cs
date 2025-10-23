namespace BootstrapBlazor.Server.Data;
public class KeywordRankingFilter : FilterPagingBase
{
    public string Domain { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int BrandId { get; set; } = 0;
    public string Group { get; set; } = string.Empty;
    public string Keyword { get; set; } = string.Empty;
    public string PicName { get; set; } = string.Empty;
    public ReportDateType ReportDateType { set; get; } = ReportDateType.Month;
    public ReportType ReportType{ set; get; } = ReportType.Keyword;
    public LevelSearchVolumne LevelSearchVolumne { set; get; } = LevelSearchVolumne.All;
    public List<int> GroupIds { set; get; } = new  List<int>();
    public List<int> BrandIds { set; get; } = new  List<int>();
    public List<string> GroupNames { set; get; } = new  List<string>();
    public List<string> Brands { set; get; } = new  List<string>();
    public List<string> PicNames { set; get; } = new  List<string>();
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
