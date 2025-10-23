using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BootstrapBlazor.Server.Data;

public class CreateUserDto
{
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Họ là bắt buộc")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Tên là bắt buộc")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "Mã nhân viên là bắt buộc")]
    public string UserCode { get; set; } = "";

    public string Password { get; set; } = "Abc@123";

    public string PasswordConfirm { get; set; } = "Abc@123";

    public int Gender { get; set; } = 1; // 0: Female, 1: Male, 2: Unknown

    public DateTime DOB { get; set; } = DateTime.Now.AddYears(-25);

    public string PhoneNumber { get; set; } = "0000000000";

    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;
    
    public bool IsDelete { get; set; } = false;

    [Required(ErrorMessage = "Vui lòng chọn ít nhất một vai trò")]
    public List<string> Roles { get; set; } = new List<string>();

    public int? PositionId { get; set; }

    public string? AvatarURL { get; set; }
    
    public List<int> DepartmentIds { get; set; } = new List<int>();
    
    public int? CreatedBy { get; set; }
    
    public int? ModifiedBy { get; set; }

    public string? CitizenIDNumber { get; set; }
    
    public string? Address { get; set; }
    
    public int? DependsId { get; set; }
    
    public int? Relationship { get; set; }
    
    public int ODX { get; set; }
}

