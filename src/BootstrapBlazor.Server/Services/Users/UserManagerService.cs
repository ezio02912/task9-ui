using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using BootstrapBlazor.Server.Http;
using BootstrapBlazor.Server.Identity;

namespace BootstrapBlazor.Server.Services
{
    public interface IUserManagerService
    {
        Task<List<UserWithNavigationPropertiesDto>> GetListWithNavigationAsync();
        Task<List<UserBasicInfoDto>> GetBasicInfoUsersWithNavigationAsync(FilterBaseData filter);
        Task<UserWithNavigationPropertiesDto> GetWithNavigationProperties(int id);
        Task<List<UserIdentityDto>> GetBasicUserInfosAsync(string role = "");

        Task<List<UserDto>> GetListAsync();

        Task<List<UserIdentityDto>> GetBasicUserInfoAsync(string role = "");

        Task<UserDto> CreateUserWithNavigationPropertiesAsync(CreateUserDto input);

        Task<TokenDto?> SignInAsync(UserModel input);

        Task<bool> SetNewPasswordAsync(NewUserPasswordDto input);

        Task<TokenDto?> RefreshTokenAsync(TokenModel token);

        void Logout();

        Task<List<UserDto>> GetListByRoles(string roleName);

        Task<List<UserDto>> GetListByRoles(int createBy, string roleName);

        Task<ApiResponseBase<List<UserDto>>> GetListByFilterAsync(UserFilterPagingModel filter);
        Task<List<UserIdentityDto>> GetBasicSeoUserInfoAsync(int? userId = null, string? role = null);
        
        Task<UserDto> UpdateUserWithNavigationPropertiesAsync(UpdateUserDto input, int id);
    }
    public class UserManagerService : IUserManagerService
    {
        private IUserManagerService userManagerService;
        private ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IJSRuntime _jsRuntime;

        public UserManagerService(
            ILocalStorageService localStorage,
            IJSRuntime jsRuntime,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _localStorage = localStorage;
            _authenticationStateProvider = authenticationStateProvider;
            _jsRuntime = jsRuntime;
        }


        public async Task<List<UserWithNavigationPropertiesDto>> GetListWithNavigationAsync()
        {
            return await RequestClient.GetAPIAsync<List<UserWithNavigationPropertiesDto>>("user/get-list-with-nav");
        }

        public async Task<List<UserBasicInfoDto>> GetBasicInfoUsersWithNavigationAsync(FilterBaseData filter)
        {
            return await RequestClient.PostAPIAsync<List<UserBasicInfoDto>>("user/get-basic-info-users-with-navigation-properties", filter);
        }

        public async Task<UserWithNavigationPropertiesDto> GetWithNavigationProperties(int id)
        {
            return await RequestClient.GetAPIAsync<UserWithNavigationPropertiesDto>($"user/get-with-nav-properties/{id}");
        }


        public async Task<List<UserIdentityDto>> GetBasicUserInfosAsync(string role = "")
        {
            return await RequestClient.GetAPIAsync<List<UserIdentityDto>>($"user/get-basic-user-info?role={role}");
        }

        public async Task<List<UserDto>> GetListAsync()
        {
            return await RequestClient.GetAPIAsync<List<UserDto>>("user");
        }

        public async Task<List<UserIdentityDto>> GetBasicUserInfoAsync(string role = "")
        {
            return await RequestClient.GetAPIAsync<List<UserIdentityDto>>("user/get-basic-user-info");
        }

        public async Task<List<UserIdentityDto>> GetBasicSeoUserInfoAsync(int? userId = null, string? role = null)
        {
            var queryParams = new List<string>();
            if (userId.HasValue)
                queryParams.Add($"userId={userId.Value}");
            if (!string.IsNullOrEmpty(role))
                queryParams.Add($"role={role}");
            
            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            return await RequestClient.GetAPIAsync<List<UserIdentityDto>>($"user/get-basic-seo-user-info{queryString}");
        }


        public async Task<TokenDto?> SignInAsync(UserModel input)
        {
            try
            {
                var response = await RequestClient.PostAPIAsync<TokenDto>("user/sign-in", input);

                // Check if response is valid
                if (response == null || string.IsNullOrEmpty(response.AccessToken))
                {
                    return null;
                }

                RequestClient.AttachToken(response.AccessToken);
                RequestClient.InjectServices(_localStorage);
                await _localStorage.SetItemAsync("my-access-token", response.AccessToken);
                await _localStorage.SetItemAsync("my-refresh-token", response.RefreshToken);
                ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(input.UserName);

                return response;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Console.WriteLine($"SignIn Error: {ex.Message}");
                System.Console.WriteLine($"StackTrace: {ex.StackTrace}");

                // Return null to indicate sign-in failure
                return null;
            }
        }
        public async Task<bool> SetNewPasswordAsync(NewUserPasswordDto input)
        {
            return await RequestClient.PostAPIAsync<bool>("user/set-password", input);
        }


        public async Task<TokenDto?> RefreshTokenAsync(TokenModel token)
        {
            try
            {
                // TODO: Implement refresh token logic
                var response = await RequestClient.PostAPIAsync<TokenDto>("user/refresh-token", token);

                if (response != null && !string.IsNullOrEmpty(response.AccessToken))
                {
                    RequestClient.AttachToken(response.AccessToken);
                    await _localStorage.SetItemAsync("my-access-token", response.AccessToken);
                    await _localStorage.SetItemAsync("my-refresh-token", response.RefreshToken);
                    return response;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"RefreshToken Error: {ex.Message}");
                return null;
            }
        }

        public async void Logout()
        {
            await ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
        }

        public async Task<List<UserDto>> GetListByRoles(string roleName)
        {
            return await RequestClient.GetAPIAsync<List<UserDto>>($"user/get-with-roles/{roleName}");
        }

        public async Task<List<UserDto>> GetListByRoles(int createBy, string roleName)
        {
            return await RequestClient.GetAPIAsync<List<UserDto>>($"user/get-with-roles/{createBy}/{roleName}");
        }

        public async Task<ApiResponseBase<List<UserDto>>> GetListByFilterAsync(UserFilterPagingModel filter)
        {
            return await RequestClient.PostAPIAsync<ApiResponseBase<List<UserDto>>>($"user/get-with-filters", filter);
        }

        public async Task<UserDto> CreateUserWithNavigationPropertiesAsync(CreateUserDto input)
        {
            return await RequestClient.PostAPIAsync<UserDto>("user/create-user-with-roles", input);
        }
        
        public async Task<UserDto> UpdateUserWithNavigationPropertiesAsync(UpdateUserDto input, int id)
        {
            return await RequestClient.PostAPIAsync<UserDto>($"user/update-user-with-roles/{id}", input);
        }
       
    }
}
