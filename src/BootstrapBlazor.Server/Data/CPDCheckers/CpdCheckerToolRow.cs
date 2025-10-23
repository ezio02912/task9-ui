namespace BootstrapBlazor.Server.Data;
public class CpdCheckerToolRow
    {
        public string No { get; set; } = string.Empty;
        public string Pic { get; set; } = string.Empty;
        public string Publink { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string OriginalLink { get; set; } = string.Empty;
        public string ShortLink { get; set; } = string.Empty;
        public string RedirectShortLink { get; set; } = string.Empty;
        public string FolderBanner { get; set; } = string.Empty;
        public string FileNameBanner { get; set; } = string.Empty;
        public string DeviceCheck{ get; set; } = string.Empty;
        public string Dimension { get; set; } = string.Empty;
        public string FirstElementLive { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Alt { get; set; } = string.Empty;
        public string BannerPosition { get; set; } = string.Empty;
        public string ResultShortlink { get; set; } = string.Empty;
        public string ResultShortlinkStatus { get; set; } = string.Empty;
        public string ResultAlt { get; set; } = string.Empty;
        public string ResultAltStatus { get; set; } = string.Empty;
        public string ResultTitle { get; set; } = string.Empty;
        public string ResultTitleStatus { get; set; } = string.Empty;
        public string BannerCheckNote { get; set; } = string.Empty;


        public string CheckLink { get; set; } = string.Empty;


        public override string ToString()
        {
            return $"{No}\t{Pic}\t{Publink}\t{Brand}\t{Position}\t{OriginalLink}\t{ShortLink}\t{FileNameBanner}\t{Dimension}\t{Title}\t{Alt}\t{BannerPosition}\t{ResultShortlink}\t{ResultShortlinkStatus}\t{ResultAltStatus}\t{ResultTitleStatus}\t{BannerCheckNote}";
        }
    }
