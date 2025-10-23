
namespace BootstrapBlazor.Server.Data;
public class KeywordRankingWithPicDto
{
    public string Domain { get; set; }
    public string PicName { get; set; }
    public string Keyword { get; set; }
    public int Rank { get; set; }
    public int Volume1 { set; get; }
    public int Volume2 { set; get; }
    public DateTime RankingDate { get; set; }
}



public class KeywordRankingReportDto
{
    public string ResultGroup { get; set; } = string.Empty;
    public int TotalOnTop { get; set; }
    public int MaxTotalOnTop { get; set; }
    public int Rank1 { get; set; }
    public int Rank2 { get; set; }
    public int Rank3 { get; set; }
    public int Rank4 { get; set; }
    public int Rank5 { get; set; }
    public int Rank6 { get; set; }
    public int Rank7 { get; set; }
    public int Rank8 { get; set; }
    public int Rank9 { get; set; }
    public int Rank10 { get; set; }
    public int Volume1 { get; set; }
    public int Volume2 { get; set; }
    public DateTime RankingDate { get; set; }

}

public class ReportChartPerformanceDetail
{
    public string ResultGroup { get; set; } = string.Empty;
    public int Rank { get; set; }
    public int Volume { get; set; } = 0;
}

public class KeywordRankingReportResponseDto
{
    private List<KeywordRankingReportDto> _allData = new();
    private List<ReportChartPerformanceDetail> _top1Data = new();
    private List<ReportChartPerformanceDetail> _top2Data = new();
    private List<ReportChartPerformanceDetail> _top3Data = new();
    private List<ReportChartPerformanceDetail> _top4Data = new();
    private List<ReportChartPerformanceDetail> _top5Data = new();
    private List<ReportChartPerformanceDetail> _top6Data = new();
    private List<ReportChartPerformanceDetail> _top7Data = new();
    private List<ReportChartPerformanceDetail> _top8Data = new();
    private List<ReportChartPerformanceDetail> _top9Data = new();
    private List<ReportChartPerformanceDetail> _top10Data = new();

    public List<KeywordRankingReportDto> AllData
    {
        get => _allData;
        set
        {
            _allData = value;
        }
    }

    public List<ReportChartPerformanceDetail> Top1Data
    {
        get => _top1Data;
        set
        {
            _top1Data = value;
        }
    }

    public List<ReportChartPerformanceDetail> Top2Data
    {
        get => _top2Data;
        set
        {
            _top2Data = value;
        }
    }

    public List<ReportChartPerformanceDetail> Top3Data
    {
        get => _top3Data;
        set
        {
            _top3Data = value;
        }
    }

    public List<ReportChartPerformanceDetail> Top4Data
    {
        get => _top4Data;
        set
        {
            _top4Data = value;
        }
    }

    public List<ReportChartPerformanceDetail> Top5Data
    {
        get => _top5Data;
        set
        {
            _top5Data = value;
        }
    }

    public List<ReportChartPerformanceDetail> Top6Data
    {
        get => _top6Data;
        set
        {
            _top6Data = value;
        }
    }

    public List<ReportChartPerformanceDetail> Top7Data
    {
        get => _top7Data;
        set
        {
            _top7Data = value;
        }
    }

    public List<ReportChartPerformanceDetail> Top8Data
    {
        get => _top8Data;
        set
        {
            _top8Data = value;
        }
    }

    public List<ReportChartPerformanceDetail> Top9Data
    {
        get => _top9Data;
        set
        {
            _top9Data = value;
        }
    }

    public List<ReportChartPerformanceDetail> Top10Data
    {
        get => _top10Data;
        set
        {
            _top10Data = value;
        }
    }

    public float MaxAxisValue { get; set; }
    public int Step { get; set; } = 1;
    public int Width { get; set; } = 30;

    public List<VolumeChartData> VolumeData { get; set; } = new();

    private void SortData(List<ReportChartPerformanceDetail> data)
    {
        if (data != null)
        {
            // Sort by Volume2 in descending order
            data.Sort((a, b) => b.Volume.CompareTo(a.Volume));
        }
    }

    private void SortAllData()
    {
        if (_allData != null)
        {
            // Sort by Volume2 in descending order
            _allData.Sort((a, b) => b.Volume2.CompareTo(a.Volume2));
        }
    }

    public void CalculateStep()
    {
        if (MaxAxisValue <= 0)
        {
            Step = 1;
            return;
        }

        // Calculate initial step size
        float initialStep = MaxAxisValue / 5;

        // Round to nearest nice number
        float magnitude = (float)Math.Pow(10, Math.Floor(Math.Log10(initialStep)));
        float normalizedStep = initialStep / magnitude;

        // Find the closest nice number
        float[] niceNumbers = { 1, 2, 5, 10 };
        float closestNiceNumber = niceNumbers[0];
        float minDiff = Math.Abs(normalizedStep - niceNumbers[0]);

        for (int i = 1; i < niceNumbers.Length; i++)
        {
            float diff = Math.Abs(normalizedStep - niceNumbers[i]);
            if (diff < minDiff)
            {
                minDiff = diff;
                closestNiceNumber = niceNumbers[i];
            }
        }

        // Calculate final step size
        Step = (int)(closestNiceNumber * magnitude)  == 0 ? 1 : (int)(closestNiceNumber * magnitude);
    }
}

public class VolumeChartData
{
    public string ResultGroup { get; set; } = string.Empty;
    public int Volume { get; set; }
}


 public class SeoStatCell
    {
        public string Brand { get; set; }
        public TopRange TopRange { get; set; }
        public LevelSearchVolumne VolumeRange { set; get; }
        public int Month { get; set; }
        public List<string> ListKeyWord { get; set; }
        public List<string> ListDomain { get; set; }
        public int TotalKeyWord { get; set; } = 0;
        public int TotalDomain { get; set; } = 0;
        public int KeyWords { get; set; }
        public int Domains { get; set; }
        public int Count { get; set; }
    }

public class TotalCell
{
    public int TotalKeyWord { get; set; } = 0;
    public int TotalDomain { get; set; } = 0;
    public int CountAll { get; set; } = 0;
}


public class SeoStatRow
{
    public Dictionary<string, string> Brands { set; get; } = new();
    public Dictionary<string, Dictionary<int, List<SeoStatCell>>> ReportByBrand { set; get; } =
        new Dictionary<string, Dictionary<int, List<SeoStatCell>>>();


    public Dictionary<string, Dictionary<int, TotalCell>> TotalByBrandTotal { set; get; } =
        new Dictionary<string, Dictionary<int, TotalCell>>();
}



public class KeywordSeoPerformanceReportDto
{
    public List<SeoPerformanceDetailDto> RawData { get; set; } = new();
    public List<SeoPerformanceDetailByResultDto> RawDataByKeywordOrDomain { get; set; } = new();
}

public class SeoPerformanceDetailDto
{
    public string PicName { get; set; } = string.Empty;
    public string VeryEasyTitle  { get; set; } = string.Empty;
    public string EasyTitle  { get; set; } = string.Empty;
    public string MediumTitle  { get; set; } = string.Empty;
    public string HardTitle  { get; set; } = string.Empty;
    public string VeryHardTitle  { get; set; } = string.Empty;
    public string ExtremeHardTitle  { get; set; } = string.Empty;
    public string HardLevel4Title  { get; set; } = string.Empty;
    public string HardLevel5Title  { get; set; } = string.Empty;

    public int VeryEasy { get; set; } = 0;
    public int VeryEasyTop1 { get; set; } = 0;
    public int VeryEasyTop2 { get; set; } = 0;
    public int VeryEasyTop3 { get; set; } = 0;
    public int VeryEasyTop4 { get; set; } = 0;
    public int VeryEasyTop5 { get; set; } = 0;
    public int VeryEasyTop6 { get; set; } = 0;
    public int VeryEasyTop7 { get; set; } = 0;
    public int VeryEasyTop8 { get; set; } = 0;
    public int VeryEasyTop9 { get; set; } = 0;
    public int VeryEasyTop10 { get; set; } = 0;
    public int Easy { get; set; } = 0;
    public int EasyTop1 { get; set; } = 0;
    public int EasyTop2 { get; set; } = 0;
    public int EasyTop3 { get; set; } = 0;
    public int EasyTop4 { get; set; } = 0;
    public int EasyTop5 { get; set; } = 0;
    public int EasyTop6 { get; set; } = 0;
    public int EasyTop7 { get; set; } = 0;
    public int EasyTop8 { get; set; } = 0;
    public int EasyTop9 { get; set; } = 0;
    public int EasyTop10 { get; set; } = 0;

    public int Medium { get; set; } = 0;
    public int MediumTop1 { get; set; } = 0;
    public int MediumTop2 { get; set; } = 0;
    public int MediumTop3 { get; set; } = 0;
    public int MediumTop4 { get; set; } = 0;
    public int MediumTop5 { get; set; } = 0;
    public int MediumTop6 { get; set; } = 0;
    public int MediumTop7 { get; set; } = 0;
    public int MediumTop8 { get; set; } = 0;
    public int MediumTop9 { get; set; } = 0;
    public int MediumTop10 { get; set; } = 0;
    public int Hard { get; set; } = 0;
    public int HardTop1 { get; set; } = 0;
    public int HardTop2 { get; set; } = 0;
    public int HardTop3 { get; set; } = 0;
    public int HardTop4 { get; set; } = 0;
    public int HardTop5 { get; set; } = 0;
    public int HardTop6 { get; set; } = 0;
    public int HardTop7 { get; set; } = 0;
    public int HardTop8 { get; set; } = 0;
    public int HardTop9 { get; set; } = 0;
    public int HardTop10 { get; set; } = 0;
    public int VeryHard { get; set; } = 0;
    public int VeryHardTop1 { get; set; } = 0;
    public int VeryHardTop2 { get; set; } = 0;
    public int VeryHardTop3 { get; set; } = 0;
    public int VeryHardTop4 { get; set; } = 0;
    public int VeryHardTop5 { get; set; } = 0;
    public int VeryHardTop6 { get; set; } = 0;
    public int VeryHardTop7 { get; set; } = 0;
    public int VeryHardTop8 { get; set; } = 0;
    public int VeryHardTop9 { get; set; } = 0;
    public int VeryHardTop10 { get; set; } = 0;
    public int ExtremeHard { get; set; } = 0;
    public int ExtremeHardTop1 { get; set; } = 0;
    public int ExtremeHardTop2 { get; set; } = 0;
    public int ExtremeHardTop3 { get; set; } = 0;
    public int ExtremeHardTop4 { get; set; } = 0;
    public int ExtremeHardTop5 { get; set; } = 0;
    public int ExtremeHardTop6 { get; set; } = 0;
    public int ExtremeHardTop7 { get; set; } = 0;
    public int ExtremeHardTop8 { get; set; } = 0;
    public int ExtremeHardTop9 { get; set; } = 0;
    public int ExtremeHardTop10 { get; set; } = 0;
    public int HardLevel4 { get; set; } = 0;
    public int HardLevel4Top1 { get; set; } = 0;
    public int HardLevel4Top2 { get; set; } = 0;
    public int HardLevel4Top3 { get; set; } = 0;
    public int HardLevel4Top4 { get; set; } = 0;
    public int HardLevel4Top5 { get; set; } = 0;
    public int HardLevel4Top6 { get; set; } = 0;
    public int HardLevel4Top7 { get; set; } = 0;
    public int HardLevel4Top8 { get; set; } = 0;
    public int HardLevel4Top9 { get; set; } = 0;
    public int HardLevel4Top10 { get; set; } = 0;


    public int HardLevel5 { get; set; } = 0;
    public int HardLevel5Top1 { get; set; } = 0;
    public int HardLevel5Top2 { get; set; } = 0;
    public int HardLevel5Top3 { get; set; } = 0;
    public int HardLevel5Top4 { get; set; } = 0;
    public int HardLevel5Top5 { get; set; } = 0;
    public int HardLevel5Top6 { get; set; } = 0;
    public int HardLevel5Top7 { get; set; } = 0;
    public int HardLevel5Top8 { get; set; } = 0;
    public int HardLevel5Top9 { get; set; } = 0;
    public int HardLevel5Top10 { get; set; } = 0;
}

    public class SeoPerformanceDetailByResultDto
{
    public string PicName { get; set; } = string.Empty;
    public string VeryEasyTitle  { get; set; } = string.Empty;
    public string EasyTitle  { get; set; } = string.Empty;
    public string MediumTitle  { get; set; } = string.Empty;
    public string HardTitle  { get; set; } = string.Empty;
    public string VeryHardTitle  { get; set; } = string.Empty;
    public string ExtremeHardTitle  { get; set; } = string.Empty;
    public string HardLevel4Title  { get; set; } = string.Empty;
    public string HardLevel5Title  { get; set; } = string.Empty;
    public Dictionary<string, Dictionary<int, int>> VeryEasyResults { get; set; } = new(); 
    public Dictionary<string, Dictionary<int, int>> EasyResults { get; set; } = new(); 
    public Dictionary<string, Dictionary<int, int>> MediumResults { get; set; } = new(); 
    public Dictionary<string, Dictionary<int, int>> HardResults { get; set; } = new(); 
    public Dictionary<string, Dictionary<int, int>> VeryHardResults { get; set; } = new(); 
    public Dictionary<string, Dictionary<int, int>> ExtremeHardResults { get; set; } = new(); 
    public Dictionary<string, Dictionary<int, int>> HardLevel4Results { get; set; } = new(); 
    public Dictionary<string, Dictionary<int, int>> HardLevel5Results { get; set; } = new(); 
}



public class KeywordSumarySeoReportDto
{
    public List<ByKeywordsSummaryDto> RawData { get; set; } = new();
}

public class RawDataSummaryDto
{
    public string MonthYear { get; set; } = string.Empty;
    public int Volume { get; set; } = 0;
    public string Keyword { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public int DaysOnTop { get; set; } = 0;
}

public class ByKeywordsSummaryDto
{
    public string PicName { get; set; } = string.Empty;
    public string Keyword { get; set; } = string.Empty;
    public int Volume { get; set; } = 0;
    public List<string> Domains { get; set; } =new();
    public int TotalOnTop { get; set; } = 0;
    public Dictionary<string, int> DomainDays { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, Dictionary<string, int>> DomainTops { get; set; } = new Dictionary<string, Dictionary<string, int>>();
}
