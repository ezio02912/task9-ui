 public class UserDto
    {
        public int Id { get; set; }
        public int Count { get; set; } = 0;
        public string UserName { get; set; }
        public string UserCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public Gender Gender { get; set; }
        public DateTime DOB { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public string AvatarURL { get; set; }
        public string? SignImgageUrl { get; set; }
        public string? SignKey { get; set; }
        public string? SignSecrect { get; set; }
        public int? BloodTypeId { set; get; }
        public string? CitizenIDNumber { get; set; }
        public int? CountryId { get; set; }
        public string? CountryName { get; set; }
        public int? ProvinceId { get; set; }
        public string? ProvinceName { get; set; }
        public int? DistrictId { get; set; }
        public string? DistrictName { get; set; }
        public int? WardId { get; set; }
        public string? WardName { get; set; }
        public string? Address { get; set; }
        public int? DependsId { get; set; }
        public int? Relationship { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public int? ODX { get; set; }
        public string? DOBString { get; set; }
    }

public class UserBasicInfoDto
{
    public int Id { get; set; }
    public string UserCode { get; set;}
    public string FullName { get; set; }
    public Gender Gender { get; set; } = Gender.Unknown;
    public DateTime DOB { get; set; } = DateTime.Now;
    public DateTime CreatedDate { get; set; }
    public string PhoneNumber { get; set; }
    public List<string> Departments { get; set; }
    public string Position { get; set; }
    public int AverageLevelSatisfaction { get; set; }
    public string AvatarURL { get; set; }
}


public class UserIdentityDto
{
    public int Id { get; set; }
    public int? PositionId { get; set; }
    public string PositionName { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string UserCode { get; set;}
    public string AvatarURL { get; set; }
}
public class TokenDto
{
    public string? UserId { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}
public class UserModel
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? LoginProvider { get; set; }
    public string? DeviceId { get; set; }
    public string? Token { get; set; }
}
public class NewUserPasswordDto
{
    public string UserName { get; set; }

    [Required(ErrorMessage = "NewPassword Is Required")]
    public string? NewPassword { get; set; }
    public string? NewPasswordConfirm { get {return NewPassword; } } 
}
public class TokenModel
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}
public class UserFilterPagingModel : BaseFilterPagingDto
{
    public string? FullName { get; set; }
    public string? UserCode { get; set; }
    public string? RoleName { get; set; }
    public int? CreatedBy { get; set; }
    public List<int>? DepartmentIds { get; set; }
    public List<int>? UserIds { get; set; }
    public List<int>? RoleIds { get; set; }
    public List<string>? Phones { get; set; }
    public Gender? Gender{ get; set; }
    public DateTime? DobFrom { get; set; }
    public DateTime? DobTo { get; set; }
}
