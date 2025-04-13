using System.Net;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
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
        private readonly NavigationManager _navigationManager;

        public AuthService(ILocalStorageService localStorage,
            AuthenticationStateProvider authenticationStateProvider,
            IHttpClientService httpClientService,
            IHttpClientFactory httpClientFactory,
            NavigationManager navigationManager)
        {
            _localStorage = localStorage;
            _authenticationStateProvider = authenticationStateProvider;
            _httpClientService = httpClientService;
            _httpClientFactory = httpClientFactory;
            _navigationManager = navigationManager;
        }
        
        public async Task<HttpStatusCode> RegisterAsync(RegistrationRequest registerRequest)
        {
            var apiResponse = await _httpClientService.PostAsync<RegistrationRequest, AuthResponse>("auth/register", registerRequest);

            if (!apiResponse.IsSuccess || apiResponse.Data is null 
                || string.IsNullOrEmpty(apiResponse.Data.AccessToken) 
                || string.IsNullOrEmpty(apiResponse.Data.RefreshToken))
            {
                return apiResponse.StatusCode;
            }
            
            await _localStorage.SetItemAsync(Constants.RefreshToken, apiResponse.Data.RefreshToken);

            if (_authenticationStateProvider is CustomAuthenticationStateProvider customAuthProvider)
            {
                await customAuthProvider.MarkUserAsAuthenticated(apiResponse.Data.AccessToken);
            }

            return apiResponse.StatusCode;
        }

        public async Task<HttpStatusCode> LoginAsync(LoginRequest loginRequest)
        {
            var apiResponse = await _httpClientService.PostAsync<LoginRequest, AuthResponse>("auth/login", loginRequest);

            if (!apiResponse.IsSuccess || apiResponse.Data == null 
                || string.IsNullOrEmpty(apiResponse.Data.AccessToken) 
                || string.IsNullOrEmpty(apiResponse.Data.RefreshToken))
            {
                return apiResponse.StatusCode;
            }
            
            await _localStorage.SetItemAsync(Constants.RefreshToken, apiResponse.Data.RefreshToken);

            if (_authenticationStateProvider is CustomAuthenticationStateProvider customAuthProvider)
            {
                await customAuthProvider.MarkUserAsAuthenticated(apiResponse.Data.AccessToken);
            }

            return apiResponse.StatusCode;
        }
        
        public async Task<HttpStatusCode> RefreshAsync(string refreshToken)
        {
            // Použijeme pojmenovaného HttpClientu bez připojených handlerů
            var client = _httpClientFactory.CreateClient("NoHandlerClient");
            var request = new RefreshTokenRequest { RefreshToken = refreshToken };
            var response = await client.PostAsJsonAsync("auth/refresh", request);
            
            if (!response.IsSuccessStatusCode)
            {
                return response.StatusCode;
            }

            string? newAccessToken = await response.Content.ReadFromJsonAsync<string>();
            if (string.IsNullOrEmpty(newAccessToken))
            {
                return response.StatusCode;
            }
            
            await _localStorage.SetItemAsync(Constants.AccessToken, newAccessToken);
            return response.StatusCode;
        }

        public async Task LogoutAsync()
        {
            try
            {
                string? refreshToken = await _localStorage.GetItemAsync<string>(Constants.RefreshToken);

                // Nejdřív odhlásíme na serveru
                if (refreshToken is not null)
                {
                    var client = _httpClientFactory.CreateClient("NoHandlerClient");
                    await client.PostAsJsonAsync("auth/logout", new LogoutRequest { RefreshToken = refreshToken });
                }
            }
            finally
            {
                if (_authenticationStateProvider is CustomAuthenticationStateProvider authStateProvider)
                {
                    await authStateProvider.MarkUserAsLoggedOut();
                }
                _navigationManager.NavigateTo(Constants.Routes.Login, true);
            }
        }
    }
}
