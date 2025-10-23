# Quản lý Người dùng (User Manager)

## Tổng quan

Module quản lý người dùng cung cấp các chức năng để tạo mới, xem danh sách, và quản lý người dùng trong hệ thống.

## Các tính năng

### 1. Tạo Người dùng Mới (Create User)

**URL:** `/user-manager/create`

#### Các trường thông tin:

- **Tên đăng nhập** (*): Tên dùng để đăng nhập vào hệ thống
- **Mã nhân viên** (*): Mã định danh duy nhất của nhân viên
- **Họ** (*): Họ của người dùng
- **Tên** (*): Tên của người dùng
- **Email**: Địa chỉ email
- **Số điện thoại**: Số điện thoại liên lạc
- **Giới tính**: Nam/Nữ/Không xác định (mặc định: Nam)
- **Ngày sinh**: Ngày tháng năm sinh (mặc định: 25 tuổi)
- **Vai trò** (*): Chọn một hoặc nhiều vai trò từ danh sách
- **Chức vụ**: Chọn chức vụ từ danh sách
- **Địa chỉ**: Địa chỉ liên lạc

(*) Trường bắt buộc

#### Giá trị mặc định:

- **Mật khẩu**: `Abc@123` (cố định cho tất cả user mới)
- **Giới tính**: Nam (Gender = 1)
- **Ngày sinh**: Ngày hiện tại trừ 25 năm
- **IsActive**: `true`
- **IsDelete**: `false`

#### Cách sử dụng:

1. Truy cập `/user-manager/create`
2. Điền đầy đủ thông tin vào form
3. Chọn ít nhất một vai trò từ danh sách
4. Chọn chức vụ (nếu có)
5. Nhấn nút "Tạo mới"
6. Hệ thống sẽ tạo user với mật khẩu mặc định `Abc@123`

### 3. Chỉnh sửa Người dùng (Edit User)

**URL:** `/user-manager/edit/{userId}`

#### Các trường có thể chỉnh sửa:

- **Tên đăng nhập** (*): Tên dùng để đăng nhập
- **Mã nhân viên** (*): Mã định danh duy nhất
- **Họ** (*): Họ của người dùng
- **Tên** (*): Tên của người dùng
- **Email**: Địa chỉ email
- **Số điện thoại**: Số điện thoại liên lạc
- **Giới tính**: Nam/Nữ/Không xác định
- **Ngày sinh**: Ngày tháng năm sinh
- **Trạng thái hoạt động**: Bật/Tắt (Switch)
- **Vai trò** (*): Chọn một hoặc nhiều vai trò
- **Chức vụ**: Chọn chức vụ từ danh sách
- **Địa chỉ**: Địa chỉ liên lạc
- **Đổi mật khẩu**: Checkbox để kích hoạt chức năng đổi mật khẩu

(*) Trường bắt buộc

#### Tính năng đặc biệt:

- **Đổi mật khẩu tùy chọn**: Chỉ cần tick vào checkbox "Đổi mật khẩu" khi muốn thay đổi mật khẩu
- **Tự động load dữ liệu**: Form tự động load thông tin hiện tại của user
- **Validation**: Kiểm tra dữ liệu trước khi submit
- **3 nút điều khiển**:
  - **Cập nhật**: Lưu thay đổi
  - **Hủy**: Quay về danh sách
  - **Danh sách**: Quay về danh sách

#### Cách sử dụng:

1. Từ danh sách user, nhấn nút "Sửa" ở dòng user cần chỉnh sửa
2. Form sẽ tự động load thông tin hiện tại
3. Chỉnh sửa các trường cần thiết
4. Nếu muốn đổi mật khẩu, tick vào checkbox "Đổi mật khẩu" và nhập mật khẩu mới
5. Nhấn "Cập nhật" để lưu thay đổi

## Cấu trúc kỹ thuật

### Backend Services

#### 1. UserManagerService (Blazor Server)
- `CreateUserWithNavigationPropertiesAsync(CreateUserDto input)`: Tạo user mới
- `GetListWithNavigationAsync()`: Lấy danh sách users với thông tin liên quan

#### 2. RoleManagerService (Blazor Server)
- `GetListAsync()`: Lấy danh sách roles

#### 3. PositionService (Blazor Server)
- `GetListAsync()`: Lấy danh sách positions

### Frontend Components

#### 1. CreateUser.razor
- Component để tạo user mới
- Validate form tự động
- Hiển thị thông báo thành công/lỗi

#### 2. UserList.razor
- Component hiển thị danh sách users
- Hỗ trợ tìm kiếm, phân trang, sắp xếp
- Nút "Sửa" ở mỗi dòng để chỉnh sửa user

#### 3. EditUser.razor
- Component để chỉnh sửa thông tin user
- Load tự động dữ liệu hiện tại
- Validate form và hiển thị thông báo

### DTOs

#### CreateUserDto
```csharp
public class CreateUserDto
{
    public string UserName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserCode { get; set; }
    public string Password { get; set; } = "Abc@123";
    public string PasswordConfirm { get; set; } = "Abc@123";
    public Gender Gender { get; set; } = Gender.Male;
    public DateTime DOB { get; set; }
    public string PhoneNumber { get; set; } = "0000000000";
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDelete { get; set; } = false;
    public List<string> Roles { get; set; }
    public int? PositionId { get; set; }
    public string? Address { get; set; }
    // ... other properties
}
```

#### UpdateUserDto
```csharp
public class UpdateUserDto
{
    public string UserName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserCode { get; set; }
    public Gender Gender { get; set; }
    public DateTime DOB { get; set; }
    public bool IsActive { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsSetPassword { get; set; } = false;
    public string? Password { get; set; }
    public string? PasswordConfirm { get; set; }
    public string? Email { get; set; }
    public List<string> Roles { get; set; }
    public int? PositionId { get; set; }
    public string? Address { get; set; }
    // ... other properties
}
```

## API Endpoints

### 1. Tạo User
- **Endpoint**: `POST /api/user/create-user-with-roles`
- **Body**: `CreateUserDto`
- **Response**: `UserDto`

### 2. Cập nhật User
- **Endpoint**: `POST /api/user/update-user-with-roles/{id}`
- **Body**: `UpdateUserDto`
- **Response**: `UserDto`

### 3. Lấy thông tin User theo ID
- **Endpoint**: `GET /api/user/get-with-nav-properties/{id}`
- **Response**: `UserWithNavigationPropertiesDto`

### 4. Lấy danh sách Users
- **Endpoint**: `GET /api/user/get-list-with-nav`
- **Response**: `List<UserWithNavigationPropertiesDto>`

### 5. Lấy danh sách Roles
- **Endpoint**: `GET /api/role`
- **Response**: `List<RoleDto>`

### 6. Lấy danh sách Positions
- **Endpoint**: `GET /api/position`
- **Response**: `List<PositionDto>`

## Lưu ý

1. **Mật khẩu mặc định**: Tất cả user mới đều có mật khẩu là `Abc@123`. Người dùng nên đổi mật khẩu sau lần đăng nhập đầu tiên.

2. **Vai trò**: Phải chọn ít nhất một vai trò khi tạo user mới.

3. **Giới tính**: Mặc định là Nam (Gender = 1), có thể thay đổi khi tạo.

4. **Validation**: Form có validation tự động, các trường bắt buộc sẽ được kiểm tra trước khi submit.

5. **IsActive và IsDelete**: Mặc định IsActive = true và IsDelete = false để user mới có thể đăng nhập ngay.

## Cập nhật trong tương lai

- ✅ ~~Thêm chức năng sửa user~~ (Đã hoàn thành)
- ✅ ~~Thêm chức năng đổi mật khẩu~~ (Đã tích hợp vào chức năng sửa)
- Thêm chức năng xóa/khóa user
- Thêm chức năng upload avatar
- Thêm chức năng gán department cho user
- Thêm chức năng xem lịch sử thay đổi
- Thêm chức năng export danh sách users

