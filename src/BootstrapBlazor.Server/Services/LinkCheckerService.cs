using System.Diagnostics;
namespace BootstrapBlazor.Server.Services
{
    public interface ILinkCheckerService
    {
        Task<LinkCheckResult> CheckLinkAsync(CpdCheckerToolRow row, bool isMobileMode, Dictionary<string, List<string>> folderAndFilesFromMinio, IFileManagerService fileManagerService);
        Task<string> GetOriginalUrlAsync(string shortUrl, bool isMobileMode, int maxRedirects = 5);
        Task<bool> CheckImageLinkExistsAsync(string link, bool isMobileMode);
        Task<string> GetRedirectAndCompareUrlAsync(string url, bool isMobileMode, int maxRedirects, List<string> shortLinks);
        Task<RedirectTraceResult> TraceRedirectChainAsync(string url, bool isMobileMode, int maxRedirects = 10);
    }

    public class LinkCheckResult
    {

        public bool ShortLinkFound { get; set; }
        public bool TitleFound { get; set; }
        public bool AltFound { get; set; }
        public string BannerCheckResult { get; set; } = string.Empty;
        public string OriginalUrlFromShortLink { get; set; } = string.Empty;
    }

    public class RedirectStep
    {
        public int StepNumber { get; set; }
        public string Url { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string RedirectType { get; set; } = string.Empty; // "HTTP", "JavaScript", "Meta Refresh"
        public string NextUrl { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    public class RedirectTraceResult
    {
        public string OriginalUrl { get; set; } = string.Empty;
        public string FinalUrl { get; set; } = string.Empty;
        public List<RedirectStep> Steps { get; set; } = new List<RedirectStep>();
        public int TotalRedirects { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class LinkCheckerService : ILinkCheckerService
    {
        private readonly HttpClient _httpClient;
        private static readonly Random _random = new Random();

        public LinkCheckerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LinkCheckResult> CheckLinkAsync(CpdCheckerToolRow row, bool isMobileMode, Dictionary<string, List<string>> folderAndFilesFromMinio, IFileManagerService fileManagerService)
        {
            var result = new LinkCheckResult();

            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
                };

                using var client = new HttpClient(handler);
                SetUserAgent(client, isMobileMode);
                client.Timeout = TimeSpan.FromSeconds(30);

                var response = await client.GetAsync(row.Publink);
                if (!response.IsSuccessStatusCode)
                    return result;

                var content = await response.Content.ReadAsStringAsync();

                // Check if content contains shortlink
                result.ShortLinkFound = content.Contains(row.ShortLink);

                if (result.ShortLinkFound)
                {
                    // Extract image URL from anchor tag
                    var shortLinkImage = Helper.StringHelpers.ExtractImageFromShortLink(
                        Helper.StringHelpers.GetAnchorTagContent(content, row.ShortLink),
                        row.ShortLink);

                    if (!shortLinkImage.Contains("http"))
                    {
                        shortLinkImage = Helper.StringHelpers.GetDomain(row.Publink) + "/"+shortLinkImage;
                    }

                    if (!string.IsNullOrEmpty(shortLinkImage))
                    {
                        result.BannerCheckResult = BootstrapBlazor.Server.Helper.CompareGifHelper.GetComparisonConclusion(
                            await BootstrapBlazor.Server.Helper.CompareGifHelper.CompareGifs(row.FileNameBanner, shortLinkImage));
                    }
                    else
                    {
                        result.BannerCheckResult = "Không tìm thấy ảnh banner ở publink";
                    }
                }
                else
                {
                    result.BannerCheckResult = "Không tìm thấy banner vì không có thẻ a chứa shortlink ở publink";
                }

                // Check title and alt
                result.TitleFound = string.IsNullOrEmpty(row.Title) ? true : content.Contains(row.Title);
                result.AltFound = string.IsNullOrEmpty(row.Alt) ? true : content.Contains(row.Alt);

                // Get original URL from shortlink
                if (result.ShortLinkFound)
                {
                    result.OriginalUrlFromShortLink = await GetOriginalUrlAsync(row.ShortLink, isMobileMode);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking link: {ex.Message}");
            }

            return result;
        }

        public async Task<string> GetOriginalUrlAsync(string shortUrl, bool isMobileMode, int maxRedirects = 5)
        {
            // Add retry logic for better reliability
            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    var result = await GetOriginalUrlWithRetryAsync(shortUrl, isMobileMode, maxRedirects);
                    if (!string.IsNullOrEmpty(result))
                        return result;

                    // Wait before retry with exponential backoff
                    if (attempt < 2)
                        await Task.Delay(1000 * (attempt + 1) + _random.Next(500));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Attempt {attempt + 1} failed: {ex.Message}");
                    if (attempt == 2) // Last attempt
                    {
                        Debug.WriteLine($"Error getting original URL after all retries: {ex.Message}");
                        return string.Empty;
                    }
                }
            }
            return string.Empty;
        }

        private async Task<string> GetOriginalUrlWithRetryAsync(string shortUrl, bool isMobileMode, int maxRedirects)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                AllowAutoRedirect = false, // Disable auto redirect to handle manually
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            using var client = new HttpClient(handler);
            SetUserAgent(client, isMobileMode);
            // Increase timeout for better success rate
            client.Timeout = TimeSpan.FromSeconds(45);

            var currentUrl = shortUrl;
            var redirectCount = 0;

            while (redirectCount < maxRedirects)
            {
                var response = await client.GetAsync(currentUrl);
                var statusCode = (int)response.StatusCode;

                // Handle HTTP redirects (301, 302, 307, 308)
                if (statusCode == 302 || statusCode == 301 || statusCode == 307 || statusCode == 308)
                {
                    var location = response.Headers.Location;
                    if (location == null)
                        break;

                    currentUrl = location.IsAbsoluteUri ? location.ToString() : new Uri(new Uri(currentUrl), location).ToString();
                    redirectCount++;
                    Debug.WriteLine($"HTTP Redirect #{redirectCount}: {statusCode} -> {currentUrl}");
                }
                // Handle 200 status but check for JavaScript/Meta redirects
                else if (statusCode == 200)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var foundRedirect = false;

                    // If content is too large (>5000 chars), treat as final destination
                    if (content.Length > 5000)
                    {
                        Debug.WriteLine($"Large content detected ({content.Length} chars) - treating as final URL: {currentUrl}");
                        return response.RequestMessage?.RequestUri?.ToString() ?? currentUrl;
                    }else 
                    {
                        // Check for meta refresh redirect first
                        var metaRefreshPattern = @"<meta[^>]*http-equiv\s*=\s*[""']refresh[""'][^>]*content\s*=\s*[""'][^""']*URL\s*=\s*([^""']+)[""'][^>]*>";
                        var metaMatch = System.Text.RegularExpressions.Regex.Match(content, metaRefreshPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (metaMatch.Success && metaMatch.Groups.Count > 1)
                        {
                            var redirectUrl = metaMatch.Groups[1].Value;
                            if (!string.IsNullOrEmpty(redirectUrl))
                            {
                                currentUrl = redirectUrl.StartsWith("http") ? redirectUrl : new Uri(new Uri(currentUrl), redirectUrl).ToString();
                                redirectCount++;
                                foundRedirect = true;
                                Debug.WriteLine($"Meta Refresh Redirect #{redirectCount}: {currentUrl}");
                                continue; // Continue to next iteration to check for more redirects
                            }
                        }

                        var jsRedirectPatterns = new[]
                        {
                            @"window\.location\.href\s*=\s*[""']([^""']+)[""']",
                            @"window\.location\s*=\s*[""']([^""']+)[""']",
                            @"location\.href\s*=\s*[""']([^""']+)[""']",
                            @"location\s*=\s*[""']([^""']+)[""']",
                            @"document\.location\s*=\s*[""']([^""']+)[""']",
                            @"document\.location\.replace\([""']([^""']+)[""']\)",
                            @"window\.location\.replace\([""']([^""']+)[""']\)",
                            @"location\.replace\([""']([^""']+)[""']\)",
                            @"top\.location\s*=\s*[""']([^""']+)[""']"
                        };

                        foreach (var pattern in jsRedirectPatterns)
                        {
                            var jsMatch = System.Text.RegularExpressions.Regex.Match(content, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            if (jsMatch.Success && jsMatch.Groups.Count > 1)
                            {
                                var redirectUrl = jsMatch.Groups[1].Value;
                                if (!string.IsNullOrEmpty(redirectUrl))
                                {
                                    currentUrl = redirectUrl.StartsWith("http") ? redirectUrl : new Uri(new Uri(currentUrl), redirectUrl).ToString();
                                    redirectCount++;
                                    foundRedirect = true;
                                    Debug.WriteLine($"JS Redirect #{redirectCount}: {currentUrl}");
                                    break;
                                }
                            }
                        }
                    }
                    if (!foundRedirect)
                    {
                        return response.RequestMessage?.RequestUri?.ToString() ?? currentUrl;
                    }
                }
                else
                {
                    // Other status codes, return current URL
                    return response.RequestMessage?.RequestUri?.ToString() ?? currentUrl;
                }
            }

            return currentUrl;
        }

        public async Task<bool> CheckImageLinkExistsAsync(string link, bool isMobileMode)
        {
            try
            {
                if (!link.Contains("http"))
                {
                    link = "https://" + link;
                }

                using var client = new HttpClient();
                SetUserAgent(client, isMobileMode);
                client.Timeout = TimeSpan.FromSeconds(30);

                var response = await client.GetAsync(link);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking image link: {ex.Message}");
                return false;
            }
        }

        private void SetUserAgent(HttpClient client, bool isMobileMode)
        {
            // Updated User-Agents to latest versions
            var desktopUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
            var mobileUserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1";

            client.DefaultRequestHeaders.Add("User-Agent", isMobileMode ? mobileUserAgent : desktopUserAgent);

            // Add additional headers to appear more like a real browser
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            // Remove Accept-Encoding to avoid compression issues
            // HttpClient will handle compression automatically when using HttpClientHandler with automatic decompression
            client.DefaultRequestHeaders.Add("DNT", "1");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
        }

        public async Task<string> GetRedirectAndCompareUrlAsync(string url, bool isMobileMode, int maxRedirects, List<string> shortLinks)
        {
            // Add retry logic for better reliability
            for (int attempt = 0; attempt < 2; attempt++) // Reduced retries for performance
            {
                try
                {
                    var result = await GetRedirectAndCompareUrlWithRetryAsync(url, isMobileMode, maxRedirects, shortLinks);
                    if (!string.IsNullOrEmpty(result))
                        return result;

                    // Short wait before retry
                    if (attempt < 1)
                        await Task.Delay(500 + _random.Next(300));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"GetRedirectAndCompareUrl attempt {attempt + 1} failed: {ex.Message}");
                    if (attempt == 1) // Last attempt
                    {
                        Debug.WriteLine($"Error in GetRedirectAndCompareUrlAsync after retries: {ex.Message}");
                        return string.Empty;
                    }
                }
            }
            return string.Empty;
        }

        private async Task<string> GetRedirectAndCompareUrlWithRetryAsync(string url, bool isMobileMode, int maxRedirects, List<string> shortLinks)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                AllowAutoRedirect = false, // Disable auto redirect to handle manually
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            using var client = new HttpClient(handler);
            SetUserAgent(client, isMobileMode);
            client.Timeout = TimeSpan.FromSeconds(35); // Slightly increased timeout

            var currentUrl = url;
            var redirectCount = 0;

            while (redirectCount < maxRedirects)
            {
                var response = await client.GetAsync(currentUrl);
                int statusCode = (int)response.StatusCode;

                // Check for HTTP redirects
                if (statusCode == 302 || statusCode == 301 || statusCode == 307 || statusCode == 308)
                {
                    var location = response.Headers.Location;
                    if (location == null)
                        break;

                    currentUrl = location.IsAbsoluteUri ? location.ToString() : new Uri(new Uri(currentUrl), location).ToString();

                    // Check if this URL matches any shortlinks
                    if (shortLinks != null && shortLinks.Any(sl => currentUrl.Contains(sl)))
                    {
                        return shortLinks.FirstOrDefault(sl => currentUrl.Contains(sl));
                    }

                    redirectCount++;
                }
                // Check for successful response (200) but might have JavaScript/Meta redirects
                else if (statusCode == 200)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    // If content is too large (>5000 chars), treat as final destination and stop looking for redirects
                    if (content.Length > 5000)
                    {
                        Debug.WriteLine($"Large content detected ({content.Length} chars) - stopping redirect search");
                        break;
                    }

                    // Check for meta refresh redirect
                    var metaRefreshMatch = System.Text.RegularExpressions.Regex.Match(
                        content, @"<meta[^>]*http-equiv=[""']refresh[""'][^>]*content=[""'](\d+;\s*url=)?([^""']+)[""']",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    if (metaRefreshMatch.Success && metaRefreshMatch.Groups.Count > 2)
                    {
                        var redirectUrl = metaRefreshMatch.Groups[2].Value;
                        if (!string.IsNullOrEmpty(redirectUrl))
                        {
                            currentUrl = redirectUrl.StartsWith("http") ? redirectUrl : new Uri(new Uri(currentUrl), redirectUrl).ToString();

                            // Check if this URL matches any shortlinks
                            if (shortLinks != null && shortLinks.Any(sl => currentUrl.Contains(sl)))
                            {
                                return shortLinks.FirstOrDefault(sl => currentUrl.Contains(sl));
                            }

                            redirectCount++;
                            continue;
                        }
                    }

                    // Enhanced JavaScript redirect detection for smaller content only
                    if (content.Length <= 3000) // Keep this smaller for performance in compare function
                    {
                        var jsRedirectPatterns = new[]
                        {
                            @"window\.location\.href\s*=\s*[""']([^""']+)[""']",
                            @"window\.location\s*=\s*[""']([^""']+)[""']",
                            @"location\.href\s*=\s*[""']([^""']+)[""']",
                            @"location\s*=\s*[""']([^""']+)[""']",
                            @"document\.location\s*=\s*[""']([^""']+)[""']",
                            // Enhanced patterns
                            @"window\.location\.replace\([""']([^""']+)[""']\)",
                            @"location\.replace\([""']([^""']+)[""']\)"
                        };

                        foreach (var pattern in jsRedirectPatterns)
                        {
                            var jsMatch = System.Text.RegularExpressions.Regex.Match(content, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            if (jsMatch.Success && jsMatch.Groups.Count > 1)
                            {
                                var redirectUrl = jsMatch.Groups[1].Value;
                                if (!string.IsNullOrEmpty(redirectUrl))
                                {
                                    currentUrl = redirectUrl.StartsWith("http") ? redirectUrl : new Uri(new Uri(currentUrl), redirectUrl).ToString();

                                    // Check if this URL matches any shortlinks
                                    if (shortLinks != null && shortLinks.Any(sl => currentUrl.Contains(sl)))
                                    {
                                        return shortLinks.FirstOrDefault(sl => currentUrl.Contains(sl));
                                    }

                                    redirectCount++;
                                    break;
                                }
                            }
                        }
                    }

                    // If no redirects found in content, break the loop
                    if (redirectCount == 0 || !metaRefreshMatch.Success)
                    {
                        break;
                    }
                }
                else
                {
                    // Other status codes, stop checking
                    break;
                }
            }

            return string.Empty;
        }

        public async Task<RedirectTraceResult> TraceRedirectChainAsync(string url, bool isMobileMode, int maxRedirects = 10)
        {
            var result = new RedirectTraceResult
            {
                OriginalUrl = url,
                Success = true
            };

            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                    AllowAutoRedirect = false, // Disable auto redirect to handle manually
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
                };

                using var client = new HttpClient(handler);
                SetUserAgent(client, isMobileMode);
                client.Timeout = TimeSpan.FromSeconds(30);

                var currentUrl = url;
                var redirectCount = 0;

                while (redirectCount < maxRedirects)
                {
                    var response = await client.GetAsync(currentUrl);
                    var statusCode = (int)response.StatusCode;
                    var step = new RedirectStep
                    {
                        StepNumber = redirectCount + 1,
                        Url = currentUrl,
                        StatusCode = statusCode
                    };

                    // Handle HTTP redirects (301, 302, 307, 308)
                    if (statusCode == 302 || statusCode == 301 || statusCode == 307 || statusCode == 308)
                    {
                        var location = response.Headers.Location;
                        if (location == null)
                        {
                            step.RedirectType = "HTTP Redirect";
                            step.Details = $"Status {statusCode} but no Location header found";
                            result.Steps.Add(step);
                            break;
                        }

                        var nextUrl = location.IsAbsoluteUri ? location.ToString() : new Uri(new Uri(currentUrl), location).ToString();
                        step.RedirectType = "HTTP Redirect";
                        step.NextUrl = nextUrl;
                        step.Details = $"Status {statusCode} -> {nextUrl}";

                        result.Steps.Add(step);
                        currentUrl = nextUrl;
                        redirectCount++;
                    }
                    // Handle 200 status but check for JavaScript/Meta redirects
                    else if (statusCode == 200)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var foundRedirect = false;

                        // Check for meta refresh redirect
                        var metaRefreshMatch = System.Text.RegularExpressions.Regex.Match(
                            content, @"<meta[^>]*http-equiv=[""']refresh[""'][^>]*content=[""'](\d+;\s*url=)?([^""']+)[""']",
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                        if (metaRefreshMatch.Success && metaRefreshMatch.Groups.Count > 2)
                        {
                            var redirectUrl = metaRefreshMatch.Groups[2].Value;
                            if (!string.IsNullOrEmpty(redirectUrl))
                            {
                                var nextUrl = redirectUrl.StartsWith("http") ? redirectUrl : new Uri(new Uri(currentUrl), redirectUrl).ToString();
                                step.RedirectType = "Meta Refresh";
                                step.NextUrl = nextUrl;
                                step.Details = $"Meta refresh redirect to {nextUrl}";

                                result.Steps.Add(step);
                                currentUrl = nextUrl;
                                redirectCount++;
                                foundRedirect = true;
                            }
                        }

                        // Check for JavaScript redirects if no meta redirect found
                        // Skip JS redirect detection for large HTML content (likely full websites)
                        if (!foundRedirect && content.Length <= 1000)
                        {
                            var jsRedirectPatterns = new[]
                            {
                                @"window\.location\.href\s*=\s*[""']([^""']+)[""']",
                                @"window\.location\s*=\s*[""']([^""']+)[""']",
                                @"location\.href\s*=\s*[""']([^""']+)[""']",
                                @"location\s*=\s*[""']([^""']+)[""']",
                                @"document\.location\s*=\s*[""']([^""']+)[""']"
                            };

                            foreach (var pattern in jsRedirectPatterns)
                            {
                                var jsMatch = System.Text.RegularExpressions.Regex.Match(content, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                if (jsMatch.Success && jsMatch.Groups.Count > 1)
                                {
                                    var redirectUrl = jsMatch.Groups[1].Value;
                                    if (!string.IsNullOrEmpty(redirectUrl))
                                    {
                                        var nextUrl = redirectUrl.StartsWith("http") ? redirectUrl : new Uri(new Uri(currentUrl), redirectUrl).ToString();
                                        step.RedirectType = "JavaScript Redirect";
                                        step.NextUrl = nextUrl;
                                        step.Details = $"JavaScript redirect: {pattern} -> {nextUrl}";

                                        result.Steps.Add(step);
                                        currentUrl = nextUrl;
                                        redirectCount++;
                                        foundRedirect = true;
                                        break;
                                    }
                                }
                            }
                        }

                        // If no redirect found in 200 response, this is the final URL
                        if (!foundRedirect)
                        {
                            step.RedirectType = "Final Destination";
                            step.Details = "No more redirects found - Final URL";
                            result.Steps.Add(step);
                            break;
                        }
                    }
                    else
                    {
                        // Other status codes
                        step.RedirectType = "Final Status";
                        step.Details = $"Final response with status {statusCode}";
                        result.Steps.Add(step);
                        break;
                    }
                }

                result.FinalUrl = currentUrl;
                result.TotalRedirects = redirectCount;

                if (redirectCount >= maxRedirects)
                {
                    result.ErrorMessage = $"Max redirects ({maxRedirects}) exceeded";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                Debug.WriteLine($"Error tracing redirect chain: {ex.Message}");
            }

            return result;
        }
    }
}
