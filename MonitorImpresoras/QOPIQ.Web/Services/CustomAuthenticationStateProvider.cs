using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using QOPIQ.Web.Models;
using System.Security.Claims;
using System.Text.Json;

namespace QOPIQ.Web.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                var userInfo = await _localStorage.GetItemAsync<UserInfo>("userInfo");

                if (string.IsNullOrEmpty(token) || userInfo == null)
                {
                    return new AuthenticationState(_anonymous);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userInfo.Id.ToString()),
                    new Claim(ClaimTypes.Name, userInfo.FullName),
                    new Claim(ClaimTypes.Email, userInfo.Email),
                    new Claim("tenant_id", userInfo.TenantId),
                    new Claim("company_id", userInfo.CompanyId?.ToString() ?? ""),
                    new Claim(ClaimTypes.Role, userInfo.Role)
                };

                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        public async Task MarkUserAsAuthenticated(UserInfo userInfo)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userInfo.Id.ToString()),
                new Claim(ClaimTypes.Name, userInfo.FullName),
                new Claim(ClaimTypes.Email, userInfo.Email),
                new Claim("tenant_id", userInfo.TenantId),
                new Claim("company_id", userInfo.CompanyId?.ToString() ?? ""),
                new Claim(ClaimTypes.Role, userInfo.Role)
            };

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task MarkUserAsLoggedOut()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }
    }
}

