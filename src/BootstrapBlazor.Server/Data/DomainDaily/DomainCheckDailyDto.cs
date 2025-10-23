using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data;
public class DomainDailyUniverSheetDto
{
    [JsonPropertyName("domain")]
    public string Domain { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("backgroundColor")]
    public string? BackgroundColor { get; set; }
    [JsonPropertyName("textColor")]
    public string? TextColor { get; set; }
    [JsonPropertyName("isMainsite")]
    public bool IsMainsite { get; set; }
    [JsonPropertyName("isBold")]
    public bool IsBold { get; set; }
}


public class DomainCheckDailyDto
{
    public int Id { get; set; }
    public string Domain { get; set; }
    public string Type { get; set; }
    public string BackgroundColor { get; set; }
    public string TextColor { get; set; }
    public bool IsMainsite { get; set; }
    public bool IsBold { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DomainCheckDailySearchDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("domain")]
    public string Domain { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("backgroundColor")]
    public string? BackgroundColor { get; set; }
    [JsonPropertyName("textColor")]
    public string? TextColor { get; set; }
    [JsonPropertyName("isMainsite")]
    public bool IsMainsite { get; set; }
    [JsonPropertyName("isBold")]
    public bool IsBold { get; set; }
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}


public class CreateUpdateDomainCheckDailyDto
{
    [Required(ErrorMessage = "Domain is required")]
    [StringLength(255, ErrorMessage = "Domain cannot exceed 255 characters")]
    public string Domain { get; set; }

    [Required(ErrorMessage = "Type is required")]
    [StringLength(255, ErrorMessage = "Type cannot exceed 255 characters")]
    public string Type { get; set; }

    public string BackgroundColor { get; set; }

    public string TextColor { get; set; }

    public bool IsMainsite { get; set; }

    public bool IsBold { get; set; }
}

public class DomainCheckDailyFilterPagingDto : FilterPagingBase
{
    public string? Domain { get; set; }
    public string? Type { get; set; }
    public string? Text { get; set; }
    public bool? IsMainsite { get; set; }
    public bool? IsBold { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}


public class DomainCheckDailyFilterDto: ITableSearchModel
{
    public string? Domain { get; set; }
    public string? Type { get; set; }
    public string? Text { get; set; }
    public bool? IsMainsite { get; set; }
    public bool? IsBold { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public IEnumerable<IFilterAction> GetSearches()
    {
        var ret = new List<IFilterAction>();
        return ret;
    }

    public void Reset()
    {
        Domain = null;
        Type = null;
        Text = null;
        IsMainsite = null;
        IsBold = null;
        FromDate = null;
        ToDate = null;
    }
}