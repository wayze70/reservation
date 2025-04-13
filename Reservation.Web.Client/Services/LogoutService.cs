using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Reservation.Shared.Dtos;
using Reservation.Web.Client.CustomExtensions;

namespace Reservation.Web.Client.Services;

public class LogoutService : ILogoutService
{
    private readonly ILocalStorageService _localStorage;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly NavigationManager _navigationManager;

    public LogoutService(ILocalStorageService localStorage,
        IHttpClientFactory httpClientFactory,
        AuthenticationStateProvider authenticationStateProvider,
        NavigationManager navigationManager)
    {
        _localStorage = localStorage;
        _httpClientFactory = httpClientFactory;
        _authenticationStateProvider = authenticationStateProvider;
        _navigationManager = navigationManager;
    }
    
    public async Task LogoutAsync()
    {
        try
        {
            string? refreshToken = await _localStorage.GetItemAsync<string>(Constants.RefreshToken);
            if (!string.IsNullOrWhiteSpace(refreshToken))
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