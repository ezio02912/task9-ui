namespace BootstrapBlazor.Server.Data;

public class KeywordMonthlyMetricDto
{
    public long KeywordId { get; set; }
    public string YearMonth { get; set; } = string.Empty;
    public int SearchVol { get; set; }
}

public class CreateUpdateKeywordMonthlyMetricDto
{
    public long KeywordId { get; set; }
    public string YearMonth { get; set; } = string.Empty;
    public int SearchVol { get; set; }
}

