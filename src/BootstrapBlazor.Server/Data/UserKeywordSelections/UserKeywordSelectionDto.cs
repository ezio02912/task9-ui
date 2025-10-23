namespace BootstrapBlazor.Server.Data;

public class UserKeywordSelectionDto 
{
    public long Id { get; set; }
        public int UserId { get; set; }
        public long KeywordId { get; set; }
        public bool IsActive { get; set; }
        public DateTime SelectedAt { get; set; }
        public DateTime? DeselectedAt { get; set; }
        public string? Note { get; set; }
}

