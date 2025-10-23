using System.Globalization;
using System.Text.RegularExpressions;

namespace BootstrapBlazor.Server.Helper
{
    public static class RedirectChecker
    {
        public static async Task<(string finalDomain, string logs)> CheckRedirectAsync(DomainCheckerDto domain)
        {
            string finalUrl = string.Empty, logs= string.Empty;
            (finalUrl, logs) = await GetRedirectAndCompareUrlWithRetryAsync(domain.Domain);
            return (finalUrl, logs);
        }

        
        private static void SetUserAgent(HttpClient client)
        {
            // Updated User-Agents to latest versions
            var desktopUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
            var mobileUserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1";

            client.DefaultRequestHeaders.Add("User-Agent",  desktopUserAgent);

            // Add additional headers to appear more like a real browser
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            // Remove Accept-Encoding to avoid compression issues
            // HttpClient will handle compression automatically when using HttpClientHandler with automatic decompression
            client.DefaultRequestHeaders.Add("DNT", "1");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
        }
        private static async Task<(string finalUrl, string logs)> GetRedirectAndCompareUrlWithRetryAsync(string url, int maxRedirects = 10)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                AllowAutoRedirect = false, // Disable auto redirect to handle manually
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            using var client = new HttpClient(handler);
            SetUserAgent(client);
            client.Timeout = TimeSpan.FromSeconds(35); // Slightly increased timeout

            var currentUrl = url;
            var redirectCount = 0;
            var logs = new List<string>();
            
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

                    var previousUrl = currentUrl;
                    currentUrl = location.IsAbsoluteUri ? location.ToString() : new Uri(new Uri(currentUrl), location).ToString();
                    redirectCount++;
                    
                    logs.Add($"Redirect {redirectCount}: {statusCode} {response.StatusCode} -> {currentUrl}");
                }
                // Check for successful response (200) but might have JavaScript/Meta redirects
                else if (statusCode == 200)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    // If content is too large (>5000 chars), treat as final destination and stop looking for redirects
                    if (content.Length > 5000)
                    {
                        logs.Add($"Final destination reached (large content): {currentUrl}");
                        break;
                    }
                    // Check for meta refresh redirect
                    var metaRefreshMatch = System.Text.RegularExpressions.Regex.Match(
                        content, @"<meta[^>]*http-equiv\s*=\s*[""']refresh[""'][^>]*content\s*=\s*[""'][^""']*URL\s*=\s*([^""']+)[""'][^>]*>",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    if (metaRefreshMatch.Success && metaRefreshMatch.Groups.Count > 2)
                    {
                        var redirectUrl = metaRefreshMatch.Groups[2].Value;
                        if (!string.IsNullOrEmpty(redirectUrl))
                        {
                            var previousUrl = currentUrl;
                            currentUrl = redirectUrl.StartsWith("http") ? redirectUrl : new Uri(new Uri(currentUrl), redirectUrl).ToString();
                            redirectCount++;
                            
                            // Log the meta refresh redirect
                            logs.Add($"Meta refresh redirect {redirectCount}: {statusCode} {response.StatusCode} -> {currentUrl}");
                            continue;
                        }
                    }

                    if (content.Length <= 5000) // Keep this smaller for performance in compare function
                    {
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
                        bool next = false;
                        foreach (var pattern in jsRedirectPatterns)
                        {
                            var jsMatch = System.Text.RegularExpressions.Regex.Match(content, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            if (jsMatch.Success && jsMatch.Groups.Count > 1)
                            {
                                var redirectUrl = jsMatch.Groups[1].Value;
                                if (!string.IsNullOrEmpty(redirectUrl))
                                {
                                    var previousUrl = currentUrl;
                                    currentUrl = redirectUrl.StartsWith("http") ? redirectUrl : new Uri(new Uri(currentUrl), redirectUrl).ToString();
                                    redirectCount++;
                                    next = true;
                                    // Log the JavaScript redirect
                                    logs.Add($"Redirect {redirectCount}: {statusCode} {response.StatusCode} -> {currentUrl}");
                                    break;
                                }
                            }
                        }

                        if (next) continue;
                    }

                    // If no redirects found in content, break the loop
                    if (redirectCount == 0|| !metaRefreshMatch.Success)
                    {
                        break;
                    }
                }
                else
                {
                    // Other status codes, stop checking
                    logs.Add($"Stopped at status code: {statusCode} {response.StatusCode} - {currentUrl}");
                    break;
                }
            }

            // Join all logs into a single string
            var logsString = string.Join("|| ", logs);
            
            return (currentUrl, logsString);
        }
    }
}
