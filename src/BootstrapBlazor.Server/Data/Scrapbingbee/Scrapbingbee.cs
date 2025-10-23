public class ScrapingbeeCheckResult
{
    public bool Success { get; set; }
    public string TitleStatus { get; set; } = string.Empty;
    public string AltStatus { get; set; } = string.Empty;
    public string ShortlinkStatus { get; set; } = string.Empty;
    public string OriginalUrlFromShortLink { get; set; } = string.Empty;
    public string BannerCheckNote { get; set; } = string.Empty;
}
