using BootstrapBlazor.Components;

namespace BootstrapBlazor.Server.Data;

public class CreateUpdateUserKeywordSelectionDto
{
    public int UserId { get; set; }
    public long KeywordId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DeselectedAt { get; set; } 
    public string? Note { get; set; }
}
