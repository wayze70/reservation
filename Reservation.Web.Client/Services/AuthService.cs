using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Reservation.Shared.Dtos;
using Reservation.Web.Client.CustomExtensions;

namespace Reservation.Web.Client.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IHttpClientService _httpClientService;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthService(ILocalStorageService localStorage,
            AuthenticationStateProvider authenticationStateProvider,
            IHttpClientService httpClientService, IHttpClientFactory httpClientFactory)
        {
            _localStorage = localStorage;
            _authenticationStateProvider = authenticationStateProvider;
            _httpClientService = httpClientService;
            _httpClientFactory = httpClientFactory;
        }
        
        public async Task<bool> RegisterAsync(RegistrationRequest registerRequest)
        {
            var authResponse = await _httpClientService.PostAsync<RegistrationRequest, AuthResponse>("auth/register", registerRequest);

            if (authResponse is null || string.IsNullOrEmpty(authResponse.AccessToken) || string.IsNullOrEmpty(authResponse.RefreshToken))
                return false;
            
            await _localStorage.SetItemAsync(Constants.RefreshToken, authResponse.RefreshToken);

            if (_authenticationStateProvider is CustomAuthenticationStateProvider customAuthProvider)
            {
                await customAuthProvider.MarkUserAsAuthenticated(authResponse.AccessToken);
            }

            return true;
        }

        public async Task<bool> LoginAsync(LoginRequest loginRequest)
        {
            var authResponse = await _httpClientService.PostAsync<LoginRequest, AuthResponse>("auth/login", loginRequest);

            if (authResponse is null || string.IsNullOrEmpty(authResponse.AccessToken) || string.IsNullOrEmpty(authResponse.RefreshToken))
                return false;
            
            await _localStorage.SetItemAsync(Constants.RefreshToken, authResponse.RefreshToken);

            if (_authenticationStateProvider is CustomAuthenticationStateProvider customAuthProvider)
            {
                await customAuthProvider.MarkUserAsAuthenticated(authResponse.AccessToken);
            }

            return true;
        }
        
        public async Task<string> RefreshAsync(string refreshToken)
        {
            var client = _httpClientFactory.CreateClient("NoHandlerClient");
            var request = new RefreshTokenRequest { RefreshToken = refreshToken };
            var response = await client.PostAsJsonAsync("auth/refresh", request);
            if (!response.IsSuccessStatusCode) return null;

            string? newAccessToken = await response.Content.ReadFromJsonAsync<string>();
            if (string.IsNullOrEmpty(newAccessToken)) return null;
            
            await _localStorage.SetItemAsync(Constants.AccessToken, newAccessToken);
            return newAccessToken;
        }
        
        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync(Constants.RefreshToken);
            if (_authenticationStateProvider is CustomAuthenticationStateProvider customAuthProvider)
            {
                await customAuthProvider.MarkUserAsLoggedOut();
            }
        }
    }
}
