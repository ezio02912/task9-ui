using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using BootstrapBlazor.Server.Http;
namespace BootstrapBlazor.Server.Identity
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private Task<AuthenticationState>? _authenticationStateTask = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

        private bool IsAuth { get; set; }

        public ApiAuthenticationStateProvider(ILocalStorageService localStorage
            )
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var savedToken = "";
            AuthenticationState state = await _authenticationStateTask!;
            state = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            try
            {
                var isAuth = state.User.Identity?.IsAuthenticated ?? false;
                if (!isAuth)
                {
                    InjectServiceForHttpClient();
                    savedToken = await _localStorage.GetItemAsync<string>("my-access-token");

                    if (string.IsNullOrWhiteSpace(savedToken))
                    {
                        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                    }
                    RequestClient.AttachToken(savedToken);

                    var claims = ParseClaimsFromJwt1(savedToken);
                    if (!CheckExpiredToken(claims))
                    {
                        await _localStorage.RemoveItemAsync("my-access-token");
                        await _localStorage.RemoveItemAsync("my-refresh-token");
                        state = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                        Thread.Sleep(4000);
                    }else{
                        state = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
                    }
                    _authenticationStateTask = Task.FromResult(state);
                    NotifyAuthenticationStateChanged(_authenticationStateTask);
                }
            }
            catch (Exception e)
            {
            }
            return state;
        }

        public void SetAuthenticationState(Task<AuthenticationState> authenticationStateTask)
        {
            _authenticationStateTask = authenticationStateTask ?? throw new ArgumentNullException(nameof(authenticationStateTask));
            NotifyAuthenticationStateChanged(_authenticationStateTask);
        }


        public void Logout()
        {
            IsAuth = false;
            _authenticationStateTask = Task.FromResult(new AuthenticationState(new ClaimsPrincipal()));
            NotifyAuthenticationStateChanged(_authenticationStateTask);
        }

        public bool CheckExpiredToken(IEnumerable<Claim> claims)
        {
            var expiredClaim = claims.FirstOrDefault(x => x.Type == "exp");
            var epochTime = long.Parse(expiredClaim.Value);
            DateTime tokenTime = DateTime.UnixEpoch.AddSeconds(epochTime);
            if (tokenTime.ToLocalTime() < DateTime.Now)
            {
                return false;
            }

            return true;
        }


        private void InjectServiceForHttpClient()
        {
            RequestClient.InjectServices(_localStorage);
        }

        public void MarkUserAsAuthenticated(string userName)
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userName) }, "apiauth"));
            _authenticationStateTask = Task.FromResult(new AuthenticationState(authenticatedUser));
            IsAuth = true;
            NotifyAuthenticationStateChanged(_authenticationStateTask);
        }

        public async Task MarkUserAsLoggedOut()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            await _localStorage.RemoveItemAsync("my-access-token");
            await _localStorage.RemoveItemAsync("my-refresh-token");
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));

            NotifyAuthenticationStateChanged(authState);
        }

        public async Task<string> GetCurrentUserId()
        {
            var savedToken = await _localStorage.GetItemAsync<string>("my-access-token");
            var claims = ParseClaimsFromJwt1(savedToken);
            var userId = claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid)?.Value;
            return userId;
        }


        public async Task<string> GetUserRolesAsync()
        {
            var savedToken = await _localStorage.GetItemAsync<string>("my-access-token");
            var claims = ParseClaimsFromJwt1(savedToken);
            var roleClaims = claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
            return roleClaims.Value;
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles);

            if (roles != null)
            {
                if (roles.ToString().Trim().StartsWith("["))
                {
                    var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());

                    foreach (var parsedRole in parsedRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
                }

                keyValuePairs.Remove(ClaimTypes.Role);
            }

            claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())));

            return claims;
        }

        private IEnumerable<Claim> ParseClaimsFromJwt1(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();

            var decodedValue = handler.ReadJwtToken(jwt);
            return decodedValue.Claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
