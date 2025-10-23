namespace BootstrapBlazor.Server.Data;
public class LeagueFilter : FilterPagingBase
{
    public string? Type { get; set; }
    public string? Name { get; set; }
    public string? Country { get; set; }
    public int? LeagueId { get; set; }
}
