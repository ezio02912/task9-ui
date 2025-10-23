namespace BootstrapBlazor.Server.Data;

public class UserWithNavigationPropertiesDto
{
    public int Index { get; set;}
    public int Count { get; set; } = 0;
    public UserDto User { get; set; } = new UserDto();
    public List<string> RoleNames { get; set; } = new List<string>();
    public List<RoleDto> Roles { get; set; } = new List<RoleDto>();
    public PositionDto? Position { get; set; }
}
