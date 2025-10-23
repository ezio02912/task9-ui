using Newtonsoft.Json;

namespace BootstrapBlazor.Server.Services;

public class ScrapingbeeService
{
    private static readonly string _apiKey = "TJYU6VRRJD4BB1Z7NTN8BERZJHLJJHP21ZW3GJT66XQGEGCQTS7P6HIG6FHPMRUO5UVAJSJGT85I3W06";
    private static readonly string _baseUrl = "https://app.scrapingbee.com/api/v1/";
    private static readonly HttpClient _httpClient;

    static ScrapingbeeService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<ScrapingbeeResponse?> ScrapeWithoutAi(string url, string buildQuery, bool isRenderJS, string device = "desktop")
    {
        ScrapingbeeResponse result = null;
        try
        {
            buildQuery = buildQuery.Replace("\r", "");
            buildQuery = buildQuery.Replace("\n", "");
            buildQuery = buildQuery.Replace("&", "%26"); // URL encode ampersand
            var queryParams = new Dictionary<string, string>
            {
                { "api_key", _apiKey },
                { "url", url },
                { "render_js",isRenderJS.ToString() },
                { "wait", "3000" },
                { "extract_rules", buildQuery },
                { "json_response", "false" },
                { "premium_proxy", "true" },
                { "country_code", "vn" },
                { "block_ads", "false" },
                { "block_resources", "false" },
                { "device", device }
            };
            
            var queryString = string.Join("&", queryParams.Select(x => $"{x.Key}={x.Value}"));
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var response = await _httpClient.GetAsync($"{_baseUrl}?{queryString}", cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ScrapingbeeResponse>(res);
            }
            else
            {
                throw new HttpRequestException($"Scrapingbee API request failed with status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error calling Scrapingbee API: {ex.Message}", ex);
        }

        return null;
    }
    public async Task<ScrapingbeeResponse?> ScrapeWithXpath(string url, string buildQuery,
        bool isRenderJS , string device = "desktop", string shortLink = "")
    {
        ScrapingbeeResponse result = null;
        try
        {
            buildQuery = buildQuery.Replace("\r", "");
            buildQuery = buildQuery.Replace("\n", "");
            buildQuery = buildQuery.Replace("&", "%26"); // URL encode ampersand
            var queryParams = new Dictionary<string, string>
            {
                { "api_key", _apiKey },
                { "url", url },
                { "render_js", isRenderJS.ToString() },
                { "wait", "3000" },
                { "extract_rules", buildQuery },
                { "json_response", "false" },
                { "premium_proxy", "true" },
                { "country_code", "vn" },
                { "block_ads", "false" },
                { "block_resources", "false" },
                { "device", device }
            };
            var queryString = string.Join("&", queryParams.Select(x => $"{x.Key}={x.Value}"));
            
            // Create cancellation token with 10 seconds timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var response = await _httpClient.GetAsync($"{_baseUrl}?{queryString}", cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadAsStringAsync();
                var results=  JsonConvert.DeserializeObject<ScrapingbeeRowResponse>(res);
                if(results != null && results.Rows.Count > 0)
                {
                    var row = results.Rows.FirstOrDefault(x => x.Link.Href.Contains(shortLink));
                    if(row != null)
                    {
                        return row;
                    }else {
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }
        }
        catch (OperationCanceledException)
        {
            // Request was cancelled due to timeout
            return null;
        }
        catch (Exception ex)
        {
            return null;
        }

        return null;
    }
    public async Task<ScrapingbeeRowHrefResponse?> ScrapeWithAllLinks(string url, string buildQuery,
        bool isRenderJS , 
        string device = "desktop")
    {
        ScrapingbeeResponse result = null;
        try
        {
            buildQuery = buildQuery.Replace("\r", "");
            buildQuery = buildQuery.Replace("\n", "");
            buildQuery = buildQuery.Replace("&", "%26"); // URL encode ampersand
            var queryParams = new Dictionary<string, string>
            {
                { "api_key", _apiKey },
                { "url", url },
                { "render_js", isRenderJS.ToString() },
                { "wait", "3000" },
                { "extract_rules", buildQuery },
                { "json_response", "false" },
                { "premium_proxy", "true" },
                { "country_code", "vn" },
                { "block_ads", "false" },
                { "block_resources", "false" },
                { "device", device }
            };
            var queryString = string.Join("&", queryParams.Select(x => $"{x.Key}={x.Value}"));
            var response = await _httpClient.GetAsync($"{_baseUrl}?{queryString}");

            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadAsStringAsync();
                var results=  JsonConvert.DeserializeObject<ScrapingbeeRowHrefResponse>(res);
                return results;
            }
            else
            {
                return null;
            }
        }
        catch (Exception ex)
        {
            return null;
        }

        return null;
    }

}
