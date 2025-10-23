namespace BootstrapBlazor.Server.Data;

public class KeywordDto
{
    public long Id { get; set; }
    public int BrandId { get; set; }
    public string KeywordText { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class KeywordWithUserCountDto 
{
    public long Id { get; set; }
    public int BrandId { get; set; }
    public string KeywordText { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int UserCount { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public List<string> UserNames { get; set; } = new List<string>();
    public List<MonthlyVolumeDto> MonthlyVolumes { get; set; } = new List<MonthlyVolumeDto>();
}

public class CreateUpdateKeywordDto
{
    public int BrandId { get; set; }
    public string KeywordText { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class MonthlyVolumeDto
{
    public string YearMonth { get; set; } = string.Empty; // "2025-01"
    public int SearchVolume { get; set; }
}
