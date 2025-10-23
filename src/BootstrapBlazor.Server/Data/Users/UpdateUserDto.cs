using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BootstrapBlazor.Server.Data;

public class UpdateUserDto
{
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Họ là bắt buộc")]
    [MinLength(1, ErrorMessage = "Họ phải có ít nhất1 ký tự")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Tên là bắt buộc")]
    [MinLength(1, ErrorMessage = "Tên phải có ít nhất 1 ký tự")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "Mã nhân viên là bắt buộc")]
    [MinLength(2, ErrorMessage = "Mã nhân viên phải có ít nhất 2 ký tự")]
    public string UserCode { get; set; } = "";

    public int Gender { get; set; } // 0: Female, 1: Male, 2: Unknown
    
    public DateTime DOB { get; set; }
    
    public bool IsActive { get; set; }
    
    public string PhoneNumber { get; set; } = "0000000000";
    
    public bool IsSetPassword { get; set; } = false;
    
    public string? Password { get; set; }
    
    public string? PasswordConfirm { get; set; }
    
    public string? Email { get; set; }
    
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

