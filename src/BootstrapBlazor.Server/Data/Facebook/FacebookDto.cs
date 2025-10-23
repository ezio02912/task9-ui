using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data;
 public class FacebookGroupDto
{        
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public string GroupUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<FacebookPostDto> Posts { get; set; } = new List<FacebookPostDto>();
}


public class FacebookPostDto    
{
    public int Id { get; set; }
    public string PostUrl { get; set; } = string.Empty;

    public int FacebookGroupId { get; set; }

    public FacebookGroupDto? FacebookGroup { get; set; }

    public DateTime ScrapedAt { get; set; } = DateTime.UtcNow;

    public ICollection<FacebookCommentDto> Comments { get; set; } = new List<FacebookCommentDto>();
}

public class FacebookCommentDto
{
    public int id { get; set; }
    public string profileUrl { get; set; } = string.Empty;
    public string profilePicture { get; set; } = string.Empty;
    public string profileId { get; set; } = string.Empty;
    public string profileName { get; set; } = string.Empty;
    public string facebookId { get; set; } = string.Empty;
    public string postTitle { get; set; } = string.Empty;
    public string groupTitle { get; set; } = string.Empty;
    public string inputUrl { get; set; } = string.Empty;
    public string? comment { get; set; }
    public DateTime createdAt { get; set; }
}


public class FacebookPostRequest
{
    public string PostUrl { get; set; } = "";
    public string GroupUrl { get; set; } = "";
    public List<FacebookCommentDto> Comments { get; set; } = new();
}
