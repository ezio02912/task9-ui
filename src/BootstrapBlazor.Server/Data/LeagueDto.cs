
namespace BootstrapBlazor.Server.Data;
public class LeagueDto
{
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Country { get; set; }
        public string? Type { get; set; }
        public bool Active { get; set; } = true;
        public int LeagueId { get; set; }
}
