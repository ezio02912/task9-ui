namespace BootstrapBlazor.Server.Data;
public class PicDomainAssignmentFilter : FilterPagingBase
{
    public string Domain { get; set; } = string.Empty;
    public string PicName { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
