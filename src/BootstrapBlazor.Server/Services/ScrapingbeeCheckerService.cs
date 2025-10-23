using System.Diagnostics;
using System.Collections.Concurrent;

namespace BootstrapBlazor.Server.Services
{
    public interface IScrapingbeeCheckerService
    {
        Task<ScrapingbeeCheckResult> CheckWithScrapingbeeAsync(CpdCheckerToolRow row,
            Dictionary<string, List<string>> folderAndFilesFromMinio, IFileManagerService fileManagerService,
            bool isMobileMode,  bool isLiveMode = false, bool isRedirectMode = false,  bool isRenderJS = true);
        Task<string> GetLinkLiveWithScrapingbeeAsync(CpdCheckerToolRow row, bool isRenderJS);
        Task<List<CpdCheckerToolRow>> UpdateExcelLinkWithScrapingbeeAsync(List<CpdCheckerToolRow> originalRows,
            bool isMobileMode, bool isRenderJS = true);
        void ClearRedirectCache();
        int GetCacheSize();
        int GetSuccessRate();
    }


    public class ScrapingbeeCheckerService : IScrapingbeeCheckerService
    {
        private readonly ScrapingbeeService _scrapingbeeService;
        private readonly ILinkCheckerService _linkCheckerService;

        // Cache for redirect results to avoid duplicate API calls
        private static readonly ConcurrentDictionary<string, string> _redirectCache = new();

        // Rate limiting increased for better performance - was 5, now 8
        private static readonly SemaphoreSlim _rateLimitingSemaphore = new(8);

        // Performance tracking
        private static int _totalAttempts = 0;
        private static int _successfulAttempts = 0;
        private static readonly object _statsLock = new object();

        public ScrapingbeeCheckerService(ScrapingbeeService scrapingbeeService, ILinkCheckerService linkCheckerService)
        {
            _scrapingbeeService = scrapingbeeService;
            _linkCheckerService = linkCheckerService;
        }

        public async Task<ScrapingbeeCheckResult> CheckWithScrapingbeeAsync(CpdCheckerToolRow row, Dictionary<string, List<string>> folderAndFilesFromMinio,
            IFileManagerService fileManagerService, bool isMobileMode, bool isLiveMode = false, bool isRedirectMode = false, bool isRenderJS = true)
        {
            var result = new ScrapingbeeCheckResult();
            string shortLinkNew = !string.IsNullOrEmpty(row.RedirectShortLink) ? row.RedirectShortLink :  row.ShortLink;

            // Track attempt
            IncrementTotalAttempts();

            try
            {
                string rule = BuildScrapingbeeRule(shortLinkNew ,row.DeviceCheck, isLiveMode);
                var scrapingbeeResponse = !isLiveMode && !string.IsNullOrEmpty(row.DeviceCheck)
                 ? await _scrapingbeeService.ScrapeWithXpath(url: row.Publink, rule,isRenderJS,  device: isMobileMode ? "mobile" : "desktop", shortLink: shortLinkNew)
                 : await _scrapingbeeService.ScrapeWithoutAi(url: isLiveMode ? row.CheckLink : row.Publink, rule,isRenderJS, device: isMobileMode ? "mobile" : "desktop");

                if (scrapingbeeResponse != null)
                {
                    result.Success = true;

                    // Check title
                    if (!string.IsNullOrEmpty(row.Title))
                    {
                        result.TitleStatus = scrapingbeeResponse.Images.FirstOrDefault(x => x.Title == row.Title)?.Title == row.Title ? "OK" : $"FAILED title check: {scrapingbeeResponse.Images.FirstOrDefault(x => x.Title == row.Title)?.Title}";
                    }

                    // Check alt
                    if (!string.IsNullOrEmpty(row.Alt))
                    {
                        result.AltStatus = scrapingbeeResponse.Images.FirstOrDefault(x => x.Alt == row.Alt)?.Alt == row.Alt ? "OK" : $"FAILED alt check: {scrapingbeeResponse.Images.FirstOrDefault(x => x.Alt == row.Alt)?.Alt}";
                    }

                    // Check shortlink with enhanced error handling
                    if (!string.IsNullOrEmpty(scrapingbeeResponse.Link.Href) && string.IsNullOrEmpty(row.ResultShortlinkStatus))
                    {
                        try
                        {
                            string newShortLink = scrapingbeeResponse.Link.Href;
                            if(!newShortLink.Contains("http") || newShortLink.StartsWith("/"))
                            {
                                newShortLink = Helper.StringHelpers.GetDomain(row.Publink) + newShortLink;
                            }
                            var originalUrlFromShortLink = await _linkCheckerService.GetOriginalUrlAsync(newShortLink, isMobileMode);
                            originalUrlFromShortLink = originalUrlFromShortLink.Replace("/?", "?");
                            result.OriginalUrlFromShortLink = originalUrlFromShortLink;
                            result.ShortlinkStatus = originalUrlFromShortLink == row.OriginalLink ? "OK" : "FAILED";

                            // Track successful shortlink resolution
                            if (result.ShortlinkStatus == "OK")
                            {
                                IncrementSuccessfulAttempts();
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error resolving shortlink {scrapingbeeResponse.Link.Href}: {ex.Message}");
                            result.ShortlinkStatus = "FAILED - URL Resolution Error";
                        }
                    }
                    else
                    {
                        result.ShortlinkStatus = string.IsNullOrEmpty(row.ResultShortlinkStatus) ? "FAILED - No Href Found" : row.ResultShortlinkStatus;
                        result.OriginalUrlFromShortLink = row.ResultShortlink;
                    }

                    // Check banner
                    result.BannerCheckNote = await CheckBannerAsync(row, scrapingbeeResponse, isMobileMode, folderAndFilesFromMinio, fileManagerService);
                }
                else
                {
                    result.ShortlinkStatus = "FAILED - No response after scrape";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking with Scrapingbee: {ex.Message}");
                if(ex.Message.Contains("The operation was canceled"))
                {
                    result.ShortlinkStatus = "FAILED - Timeout after 10 seconds";
                }
                else
                {
                    result.ShortlinkStatus = $"FAILED - Exception: {ex.Message}";
                }
            }

            return result;
        }


        public async Task<string> GetLinkLiveWithScrapingbeeAsync(CpdCheckerToolRow row, bool isRenderJS)
        {
            try
            {
                string rule = BuildScrapingbeeRuleGetLinkLive(row.DeviceCheck);
                var scrapingbeeResponse = await _scrapingbeeService.ScrapeWithoutAi(row.Publink, rule,isRenderJS);

                if (scrapingbeeResponse != null && !string.IsNullOrEmpty(scrapingbeeResponse.Link.Href))
                {
                    if(scrapingbeeResponse.Link.Href.Contains("http"))
                    {
                        return scrapingbeeResponse.Link.Href;
                    }
                    else
                    {
                        return Helper.StringHelpers.GetDomain(row.Publink) + scrapingbeeResponse.Link.Href;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking with Scrapingbee: {ex.Message}");
            }

            return string.Empty;
        }

        private string BuildScrapingbeeRule(string shortLink, string rowXpath = "", bool isLiveMode = false)
        {
            if(shortLink.Contains("tinyurl"))
            {
                //https://tinyurl.com/r8nw76yw => lấy r8nw76yw
                shortLink = shortLink.Split("/").Last();
            }
            if(!string.IsNullOrEmpty(rowXpath) && !isLiveMode)
            {
                var imageXpathArray = rowXpath.Split("|");
                return $@"{{
                    ""rows"": {{
                        ""selector"": ""{imageXpathArray[0]}"",
                        ""type"": ""list"",
                        ""output"": {{
                            ""link"": {{
                                ""selector"": ""a[href*='{shortLink}']"",
                                ""output"": {{
                                    ""href"": {{
                                        ""selector"": ""a"",
                                        ""output"": ""@href""
                                    }}
                                }}
                            }},
                            ""images"": {{
                                ""selector"": ""{imageXpathArray[1]}"",
                                ""type"": ""list"",
                                ""output"": {{
                                    ""dataSrc"": {{
                                        ""selector"": ""img"",
                                        ""output"": ""@data-src""
                                    }},
                                    ""src"": {{
                                        ""selector"": ""img"",
                                        ""output"": ""@src""
                                    }},
                                    ""title"": {{
                                        ""selector"": ""img"",
                                        ""output"": ""@title""
                                    }},
                                    ""alt"": {{
                                        ""selector"": ""img"",
                                        ""output"": ""@alt""
                                    }}
                                }}
                            }}
                        }}
                    }}
                }}";
            }else
                return $@"{{
                    ""link"": {{
                        ""selector"": ""a[href*='{shortLink}']"",
                        ""type"": ""item"",
                        ""output"": {{
                            ""href"": {{
                                ""selector"": ""a"",
                                ""output"": ""@href""
                            }}
                        }}
                    }},
                    ""images"": {{
                        ""selector"":  ""a[href*='{shortLink}'] img"",
                        ""type"": ""list"",
                        ""output"": {{
                            ""dataSrc"": {{
                                ""selector"": ""img"",
                                ""output"": ""@data-src""
                            }},
                            ""src"": {{
                                ""selector"": ""img"",
                                ""output"": ""@src""
                            }},
                            ""title"": {{
                                ""selector"": ""img"",
                                ""output"": ""@title""
                            }},
                            ""alt"": {{
                                ""selector"": ""img"",
                                ""output"": ""@alt""
                            }}
                        }}
                    }}
                }}";
        }

        private string BuildScrapingbeeRuleGetLinkLive(string xpath)
        {
            return $@"{{
                ""link"": {{
                    ""selector"": ""{xpath}"",
                    ""type"": ""item"",
                    ""output"": {{
                        ""href"": {{
                            ""selector"": ""a"",
                            ""output"": ""@href""
                        }}
                    }}
                }},
                ""images"": {{
                    ""selector"": ""{xpath} img"",
                    ""type"": ""list"",
                    ""output"": {{
                        ""dataSrc"": {{
                            ""selector"": ""img"",
                            ""output"": ""@data-src""
                        }},
                        ""src"": {{
                            ""selector"": ""img"",
                            ""output"": ""@src""
                        }},
                        ""title"": {{
                            ""selector"": ""img"",
                            ""output"": ""@title""
                        }},
                        ""alt"": {{
                            ""selector"": ""img"",
                            ""output"": ""@alt""
                        }}
                    }}
                }}
            }}";
        }

        private async Task<string> CheckBannerAsync(CpdCheckerToolRow row, ScrapingbeeResponse scrapingbeeResponse,
            bool isMobileMode, Dictionary<string, List<string>> folderAndFilesFromMinio, IFileManagerService fileManagerService, bool isLiveMode =false)
        {
            if (string.IsNullOrEmpty(row.FileNameBanner) || scrapingbeeResponse.Images == null || scrapingbeeResponse.Images.Count == 0)
                return string.Empty;

            string result = string.Empty;
            foreach (var image in scrapingbeeResponse.Images)
            {
                string imageSrc = string.IsNullOrEmpty(image.DataSrc) ? image.Src : image.DataSrc;

                if (string.IsNullOrEmpty(imageSrc))
                {
                    result = "Không tìm thấy ảnh banner ở publink";
                    continue;
                }

                // Process image source
                if (imageSrc.StartsWith("//"))
                {
                    imageSrc = imageSrc.Substring(2);
                }

                if (!await _linkCheckerService.CheckImageLinkExistsAsync(imageSrc, isMobileMode))
                {

                    if (!imageSrc.Contains("http"))
                    {
                        imageSrc = Helper.StringHelpers.GetDomain(isLiveMode ? row.CheckLink : row.Publink)+ imageSrc;
                    }
                }
                else
                {
                    imageSrc = imageSrc.Contains("http") ? imageSrc : "https://" + imageSrc;
                }

                // Load folder files if not cached
                if (!folderAndFilesFromMinio.ContainsKey(row.FolderBanner))
                {
                    await LoadFolderAndFilesFromMinio(row.FolderBanner, folderAndFilesFromMinio, fileManagerService);
                }

                var bannerFile = folderAndFilesFromMinio[row.FolderBanner].FirstOrDefault(x => x.Contains(row.FileNameBanner));
                if (bannerFile != null)
                {
                    result = BootstrapBlazor.Server.Helper.CompareGifHelper.GetComparisonConclusion(
                        await BootstrapBlazor.Server.Helper.CompareGifHelper.CompareGifs(bannerFile, imageSrc, row.Dimension));
                    if(result == "OK")
                    {
                        return result;
                    }
                }
                else
                {
                    result = "Không tìm thấy ảnh banner ở hệ thống task9.pro";
                }
            }

            return result;
        }

        private async Task LoadFolderAndFilesFromMinio(string folderName, Dictionary<string, List<string>> folderAndFilesFromMinio, IFileManagerService fileManagerService)
        {
            var response = await fileManagerService.ListFileInFolderAsync(folderName);
            if (response.IsSuccess)
            {
                folderAndFilesFromMinio[folderName] = response.Data.Select(x => x.Url).ToList();
            }
        }

        public async Task<List<CpdCheckerToolRow>> UpdateExcelLinkWithScrapingbeeAsync(List<CpdCheckerToolRow> originalRows,
            bool isMobileMode, bool isRenderJS = true)
        {
            // Early return if no data to process
            if (!originalRows?.Any() == true)
                return originalRows ?? new List<CpdCheckerToolRow>();

            // Create cache for processed publinks and their links to avoid duplicate API calls
            var publinkLinksCache = new Dictionary<string, List<string>>();
            var shortLinksToSearch = originalRows
                .Where(x => !string.IsNullOrEmpty(x.ShortLink))
                .Select(x => x.ShortLink)
                .Distinct()
                .ToList();

            // Early return if no shortlinks to search for
            if (!shortLinksToSearch.Any())
                return originalRows;

            // Group rows by publink to minimize API calls
            var publinkGroups = originalRows
                .Where(x => !string.IsNullOrEmpty(x.Publink))
                .GroupBy(x => x.Publink)
                .ToList();

            string rule = $@"{{
                ""rows"": {{
                    ""selector"": ""a"",
                    ""type"": ""list"",
                    ""output"": {{
                        ""href"": {{
                            ""selector"": ""a"",
                            ""output"": ""@href""
                        }}
                    }}
                }}
            }}";

            // Process each unique publink
            foreach (var publinkGroup in publinkGroups)
            {
                var publink = publinkGroup.Key;
                var rowsForPublink = publinkGroup.ToList();

                // Check cache first
                if (!publinkLinksCache.ContainsKey(publink))
                {
                    try
                    {
                        var scrapingbeeResponse = await _scrapingbeeService.ScrapeWithAllLinks(publink, rule,isRenderJS, isMobileMode ? "mobile" : "desktop");

                        if (scrapingbeeResponse?.Rows?.Count > 0)
                        {
                            // Filter and cache valid links
                            publinkLinksCache[publink] = scrapingbeeResponse.Rows
                                .Where(x => x != null && !string.IsNullOrEmpty(x.Href))
                                .Select(x => x.Href)
                                .Where(href => IsValidHref(href))
                                .Distinct()
                                .ToList();
                        }
                        else
                        {
                            publinkLinksCache[publink] = new List<string>();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error scraping {publink}: {ex.Message}");
                        publinkLinksCache[publink] = new List<string>();
                        continue; // Skip to next publink if scraping fails
                    }
                }

                var cachedLinks = publinkLinksCache[publink];
                if (!cachedLinks.Any())
                    continue;

                // Process links with parallel execution for better performance
                var processingTasks = cachedLinks.Select(async href =>
                {
                    try
                    {
                        return await ProcessHrefForRedirect(href, publink, shortLinksToSearch, rowsForPublink);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing href {href}: {ex.Message}");
                        return (string.Empty, string.Empty, (CpdCheckerToolRow)null);
                    }
                });

                var results = await Task.WhenAll(processingTasks);

                // Update rows based on results - with parallel redirect processing
                var redirectTasks = results
                    .Where(r => !string.IsNullOrEmpty(r.Item1) && !string.IsNullOrEmpty(r.Item2) && r.Item3 != null)
                    .Select(async result =>
                    {
                        var (originalLink, matchingHref, matchedRow) = result;
                        var hrefToCheck = matchingHref.Contains("http") ? matchingHref : Helper.StringHelpers.GetDomain(publink) + matchingHref;
                        matchedRow.RedirectShortLink = matchingHref;
                        try
                        {
                            // Get original URL from redirect chain to avoid duplicate redirect calls later
                            var finalUrl = await _linkCheckerService.GetOriginalUrlAsync(hrefToCheck, isMobileMode);
                            var originalUrlFromShortLink = finalUrl.Replace("/?", "?");
                            matchedRow.ResultShortlink = originalUrlFromShortLink;
                            matchedRow.ResultShortlinkStatus = originalUrlFromShortLink == matchedRow.OriginalLink ? "OK" : "FAILED";
                            Debug.WriteLine($"Redirect processed: {matchingHref} -> {matchedRow.ResultShortlink}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error getting redirect for {matchingHref}: {ex.Message}");
                        }
                    });

                await Task.WhenAll(redirectTasks);
            }

            return originalRows;
        }

        /// <summary>
        /// Validates if an href is worth processing (filters out invalid/unwanted links including social media)
        /// </summary>
        private static bool IsValidHref(string href)
        {
            if (string.IsNullOrWhiteSpace(href))
            {
                Debug.WriteLine($"FILTERED: Empty href");
                return false;
            }

            // Convert to lowercase for case-insensitive matching
            var lowerHref = href.ToLower();

            // Filter out unwanted href patterns including social media
            var invalidPatterns = new[]
            {
                "javascript:", "mailto:", "tel:", "#",
                "javascript:void(0)", "javascript:void(0);", "javascript:void(0)#",
                // Social Media platforms
                "facebook.com", "fb.com", "m.facebook.com",
                "twitter.com", "x.com", "t.co",
                "instagram.com", "instagr.am",
                "youtube.com", "youtu.be",
                "tiktok.com", "vm.tiktok.com",
                "linkedin.com", "lnkd.in",
                "pinterest.com", "pin.it",
                "snapchat.com",
                "telegram.me", "t.me",
                "discord.gg", "discord.com",
                "whatsapp.com", "wa.me",
                "reddit.com", "redd.it",
                "tumblr.com",
                "vk.com",
                "weibo.com",
                "line.me",
                // Short patterns that might be social
                "/fb/", "/tw/", "/ig/", "/yt/", "/tiktok/", "/linkedin/"
            };

            var isValid = !invalidPatterns.Any(pattern =>
                lowerHref.Equals(pattern) ||
                lowerHref.Contains(pattern) ||
                lowerHref.StartsWith(pattern) ||
                lowerHref.Contains($"/{pattern}") ||
                lowerHref.Contains($".{pattern}")
            );

            // var isValid =lowerHref.Contains($"ad/201");

            // Only log filtered URLs to reduce noise
            if (!isValid)
            {
                var matchedPattern = invalidPatterns.FirstOrDefault(pattern =>
                    lowerHref.Equals(pattern) ||
                    lowerHref.Contains(pattern) ||
                    lowerHref.StartsWith(pattern) ||
                    lowerHref.Contains($"/{pattern}") ||
                    lowerHref.Contains($".{pattern}")
                );
                Debug.WriteLine($"FILTERED: {href} (matched pattern: {matchedPattern})");
            }

            return isValid;
        }

        /// <summary>
        /// Processes a single href to check for redirects to target shortlinks with enhanced caching and rate limiting
        /// </summary>
        private async Task<(string originalLink, string matchingHref, CpdCheckerToolRow matchedRow)> ProcessHrefForRedirect(
            string href, string publink, List<string> shortLinksToSearch, List<CpdCheckerToolRow> rowsForPublink)
        {
            // Normalize the href URL
            var hrefToCheck = href.Contains("http") ? href : Helper.StringHelpers.GetDomain(publink) + href;

            // Use original cache key format for reliability - avoid hash collision issues
            var cacheKey = $"{hrefToCheck}:{string.Join(",", shortLinksToSearch)}";
            if (_redirectCache.TryGetValue(cacheKey, out var cachedResult))
            {
                if (!string.IsNullOrEmpty(cachedResult))
                {
                    Debug.WriteLine($"Cache HIT for {hrefToCheck} -> {cachedResult}");
                    var matchedRow = rowsForPublink.FirstOrDefault(x => x.ShortLink == cachedResult);
                    return (cachedResult, href, matchedRow);
                }
                // Don't return immediately for empty cache - give it another chance
                Debug.WriteLine($"Cache had empty result for {hrefToCheck}, trying again");
            }

            // Use rate limiting to prevent overwhelming the API
            await _rateLimitingSemaphore.WaitAsync();
            try
            {
                // Double-check cache after acquiring semaphore (another thread might have processed it)
                if (_redirectCache.TryGetValue(cacheKey, out var cachedResult2))
                {
                    if (!string.IsNullOrEmpty(cachedResult2))
                    {
                        Debug.WriteLine($"Cache HIT after semaphore for {hrefToCheck} -> {cachedResult2}");
                        var matchedRow = rowsForPublink.FirstOrDefault(x => x.ShortLink == cachedResult2);
                        return (cachedResult2, href, matchedRow);
                    }
                }

                Debug.WriteLine($"Checking redirect for {hrefToCheck} with {shortLinksToSearch.Count} target shortlinks");

                // Check for redirect - simplified error handling for better reliability
                var originalLink = await _linkCheckerService.GetRedirectAndCompareUrlAsync(
                    hrefToCheck, false, 5, shortLinksToSearch); // Keep improved redirect count

                // Only cache successful results to avoid false negatives
                if (!string.IsNullOrEmpty(originalLink))
                {
                    Debug.WriteLine($"SUCCESS: {hrefToCheck} -> {originalLink}");

                    // Prevent memory bloat but don't be too aggressive
                    if (_redirectCache.Count > 10000)
                    {
                        ClearOldestCacheEntries();
                    }

                    _redirectCache.TryAdd(cacheKey, originalLink);

                    // Find matching row for this publink and shortlink combination
                    var matchedRow = rowsForPublink.FirstOrDefault(x => x.ShortLink == originalLink);
                    return (originalLink, href, matchedRow);
                }
                else
                {
                    Debug.WriteLine($"NO MATCH: {hrefToCheck} did not redirect to any target shortlinks");
                }

                return (string.Empty, string.Empty, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR processing href {href}: {ex.Message}");
                return (string.Empty, string.Empty, null);
            }
            finally
            {
                _rateLimitingSemaphore.Release();
            }
        }

        /// <summary>
        /// Clear oldest cache entries to prevent memory bloat
        /// </summary>
        private void ClearOldestCacheEntries()
        {
            try
            {
                var keysToRemove = _redirectCache.Keys.Take(_redirectCache.Count / 4).ToList(); // Remove 25% of entries
                foreach (var key in keysToRemove)
                {
                    _redirectCache.TryRemove(key, out _);
                }
                Debug.WriteLine($"Cleared {keysToRemove.Count} cache entries to prevent memory bloat");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing cache entries: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears the redirect cache to free memory or when starting fresh processing
        /// </summary>
        public void ClearRedirectCache()
        {
            _redirectCache.Clear();
            Debug.WriteLine("Redirect cache cleared");
        }

        /// <summary>
        /// Get current cache size for monitoring
        /// </summary>
        public int GetCacheSize()
        {
            return _redirectCache.Count;
        }

        /// <summary>
        /// Get success rate for monitoring
        /// </summary>
        public int GetSuccessRate()
        {
            lock (_statsLock)
            {
                if (_totalAttempts == 0) return 0;
                return (int)((_successfulAttempts * 100.0) / _totalAttempts);
            }
        }

        private void IncrementTotalAttempts()
        {
            lock (_statsLock)
            {
                _totalAttempts++;
            }
        }

        private void IncrementSuccessfulAttempts()
        {
            lock (_statsLock)
            {
                _successfulAttempts++;
            }
        }
    }
}
