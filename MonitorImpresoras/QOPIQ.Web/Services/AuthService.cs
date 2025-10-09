using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using QOPIQ.Web.Models;
using System.Text.Json;

namespace QOPIQ.Web.Services
{
    public class AuthService
    {
        private readonly ApiService _apiService;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public AuthService(
            ApiService apiService, 
            ILocalStorageService localStorage,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _apiService = apiService;
            _localStorage = localStorage;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<AuthResult> LoginAsync(LoginRequest loginRequest)
        {
            try
            {
                // Primero establecer el tenant para la llamada de login
                await _localStorage.SetItemAsync("tenantId", loginRequest.TenantId);

                var response = await _apiService.PostAsync<LoginRequest, AuthResponse>("/api/auth/login", loginRequest);
                
                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    // Guardar token y datos del usuario
                    await _localStorage.SetItemAsync("authToken", response.Token);
                    await _localStorage.SetItemAsync("userInfo", response.User);
                    await _localStorage.SetItemAsync("tenantId", response.User.TenantId);

                    // Notificar cambio de estado de autenticación
                    await ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(response.User);

                    return new AuthResult { Success = true, User = response.User };
                }
                else
                {
                    return new AuthResult { Success = false, ErrorMessage = "Credenciales inválidas" };
                }
            }
            catch (Exception ex)
            {
                return new AuthResult { Success = false, ErrorMessage = $"Error de conexión: {ex.Message}" };
            }
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("userInfo");
            await _localStorage.RemoveItemAsync("tenantId");

            await ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
        }

        public async Task<UserInfo?> GetCurrentUserAsync()
        {
            return await _localStorage.GetItemAsync<UserInfo>("userInfo");
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            return !string.IsNullOrEmpty(token);
        }

        public async Task<string?> GetTenantIdAsync()
        {
            return await _localStorage.GetItemAsync<string>("tenantId");
        }
    }
}

