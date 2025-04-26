using System.Net;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Reservation.Shared.Dtos;
using Reservation.Web.Client.CustomExtensions;

namespace Reservation.Web.Client.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IHttpClientService _httpClientService;
        private readonly NavigationManager _navigationManager;
        private readonly ISnackbar _snackbar;

        public AuthService(ILocalStorageService localStorage,
            AuthenticationStateProvider authenticationStateProvider,
            IHttpClientService httpClientService,
            NavigationManager navigationManager,
            ISnackbar snackbar)
        {
            _localStorage = localStorage;
            _authenticationStateProvider = authenticationStateProvider;
            _httpClientService = httpClientService;
            _navigationManager = navigationManager;
            _snackbar = snackbar;
        }

        public async Task<HttpStatusCode> RegisterAsync(RegistrationRequest registerRequest)
        {
            var apiResponse =
                await _httpClientService.PostAsync<RegistrationRequest, AuthResponse>("auth/register", registerRequest);

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
            var apiResponse =
                await _httpClientService.PostAsync<LoginRequest, AuthResponse>("auth/login", loginRequest);

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

        public async Task LogoutAsync()
        {
            try
            {
                string? refreshToken = await _localStorage.GetItemAsync<string>(Constants.RefreshToken);
                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    await _httpClientService.PostAsync<LogoutRequest, bool>("auth/logout", new LogoutRequest
                    {
                        RefreshToken = refreshToken
                    });
                }
            }
            finally
            {
                if (_authenticationStateProvider is CustomAuthenticationStateProvider authStateProvider)
                {
                    await authStateProvider.MarkUserAsLoggedOut();
                }

                _navigationManager.NavigateTo(Constants.Routes.Login);
            }
        }

        public async Task LogoutAllDevicesAsync()
        {
            try
            {
                string? refreshToken = await _localStorage.GetItemAsync<string>(Constants.RefreshToken);
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    _snackbar.Add("Něco se nepovedlo, zkuste se znovu přihlásti a opakujte akci", Severity.Error);
                    _navigationManager.NavigateTo(Constants.Routes.Login, true);
                    return;
                }

                var response = await _httpClientService.PostAsync<LogoutRequest, bool>("auth/logout-all", new
                    LogoutRequest { RefreshToken = refreshToken });

                if (response.IsSuccess)
                {
                    _snackbar.Add("Úspěšně jste se odhlásili ze všech zařízení", Severity.Success);
                }
                else
                {
                    _snackbar.Add("Něco se nepovedlo, zkuste se znovu přihlásti a opakujte akci", Severity.Error);
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