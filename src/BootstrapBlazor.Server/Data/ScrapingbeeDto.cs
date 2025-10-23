
namespace BootstrapBlazor.Server.Data;
using Newtonsoft.Json;

public class ScrapingbeeAIResponse
{
    [JsonProperty("ai_response")]
    public string? AiResponse { get; set; }
}


public class ScrapingbeeAIDetailResponse
{
    [JsonProperty("href")]
    public string? Href { get; set; }
    [JsonProperty("img_src")]
    public string? ImgSrc { get; set; }
    [JsonProperty("img_alt")]
    public string? ImgAlt { get; set; }
    [JsonProperty("img_title")]
    public string? ImgTitle { get; set; }
}


public class ScrapingbeeResponse
{
    [JsonProperty("link")]
    public ScrapingbeeLink Link { get; set; } = new();

    [JsonProperty("images")]
    public List<ScrapingbeeImage> Images { get; set; } = new();
}



public class ScrapingbeeRowResponse
{
    [JsonProperty("rows")]
    public List<ScrapingbeeResponse> Rows { get; set; } = new();
}


public class ScrapingbeeRowHrefResponse
{
    [JsonProperty("rows")]
    public List<ScrapingbeeHrefResponse> Rows { get; set; } = new();
}

public class ScrapingbeeHrefResponse
{
    [JsonProperty("href")]
    public string Href { get; set; } = string.Empty;
}


public class ScrapingbeeLink
{
    [JsonProperty("href")]
    public string Href { get; set; } = string.Empty;
}

public class ScrapingbeeImage
{
    [JsonProperty("dataSrc")]
    public string DataSrc { get; set; } = string.Empty;

    [JsonProperty("src")]
    public string Src { get; set; } = string.Empty;

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("alt")]
    public string Alt { get; set; } = string.Empty;
}
