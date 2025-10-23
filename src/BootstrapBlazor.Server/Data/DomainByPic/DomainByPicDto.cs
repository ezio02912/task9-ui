using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data;
public class DomainUniverSheetDto
{
    [JsonPropertyName("domain")]
    public string Domain { get; set; } = "Paste vào ô này để thêm miền";
    [JsonPropertyName("team")]
    public string Team { get; set; }
    [JsonPropertyName("pic")]
    public string Pic { get; set; }
}


public class DomainCheckerDto
{
    public string Domain { get; set; } = string.Empty;
    public string RedirectLogs { get; set; } = string.Empty;
    public string FinalDomain { get; set; } = string.Empty;
}

public class DomainSearchDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("domain")]
    public string Domain { get; set; }
    [JsonPropertyName("team")]
    public string Team { get; set; }
    [JsonPropertyName("pic")]
    public string Pic { get; set; }
    [JsonPropertyName("backgroundColorTeam")]
    public string? BackgroundColorTeam { get; set; }
    [JsonPropertyName("textColorTeam")]
    public string? TextColorTeam { get; set; }
    [JsonPropertyName("backgroundColorPic")]
    public string? BackgroundColorPic { get; set; }
    [JsonPropertyName("textColorPic")]
    public string? TextColorPic { get; set; }
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }
    [NotMapped]
    [JsonPropertyName("isMainSite")]
    public bool IsMainSite { get; set; }
}


public class CreateUpdateDomainSearchDto
{
    [Required(ErrorMessage = "Domain is required")]
    [StringLength(255, ErrorMessage = "Domain cannot exceed 255 characters")]
    public string Domain { get; set; }

    [Required(ErrorMessage = "Team is required")]
    [StringLength(255, ErrorMessage = "Team cannot exceed 255 characters")]
    public string Team { get; set; }

    [Required(ErrorMessage = "Pic is required")]
    [StringLength(255, ErrorMessage = "Pic cannot exceed 255 characters")]
    public string Pic { get; set; }

    public string BackgroundColorTeam { get; set; }

    public string TextColorTeam { get; set; }

    public string BackgroundColorPic { get; set; }

    public string TextColorPic { get; set; }
}

public class DomainSearchFilterPagingDto : FilterPagingBase
{
    public string? Domain { get; set; }
    public string? Team { get; set; }
    public string? Pic { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}


public class DomainSearchFilterDto: ITableSearchModel
{
    public string? Domain { get; set; }
    public string? Team { get; set; }
    public string? Pic { get; set; }
    public IEnumerable<IFilterAction> GetSearches()
    {
        var ret = new List<IFilterAction>();
        return ret;
    }

    public void Reset()
    {
        Domain = null;
        Team = null;
        Pic = null;
    }
}