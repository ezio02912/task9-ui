
public class CreateAIChatDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public string Question { get; set; } = string.Empty;
    [Required]
    public string Answer { get; set; } = string.Empty;
}
public class AIChatFilterDto : FilterPagingBase
{
    /// <summary>
    /// Filter by specific user ID
    /// </summary>
    public int? UserId { get; set; }
    
    /// <summary>
    /// Search text in question or answer
    /// </summary>
    public string? SearchText { get; set; }
    
    /// <summary>
    /// Filter by date from
    /// </summary>
    public DateTime? FromDate { get; set; }
    
    /// <summary>
    /// Filter by date to
    /// </summary>
    public DateTime? ToDate { get; set; }
}

public class AIChatResponseDto
{
    public string id { get; set; } = string.Empty;
    public string userId { get; set; } = string.Empty;
    public string question { get; set; } = string.Empty;
    public string answer { get; set; } = string.Empty;
    public DateTime createdAt { get; set; } = DateTime.Now;
}