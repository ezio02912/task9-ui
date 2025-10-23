
namespace BootstrapBlazor.Server.Data;
public class PicDomainAssignmentDto
{
        public int Id { get; set; }
        public string Domain { get; set; } = string.Empty;
        public string PicName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
}
