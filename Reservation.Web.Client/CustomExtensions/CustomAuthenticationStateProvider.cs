using System.Security.Claims;
using Blazored.LocalStorage;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Components.Authorization;
using Reservation.Web.Client.CustomExtensions;

namespace Reservation.Web.Client.Services;

public class CustomAuthenticationStateProvider : 
    AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;

    public CustomAuthenticationStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Načtení tokenu z local storage
        string? token = await _localStorage.GetItemAsync<string>(Constants.AccessToken);

        ClaimsIdentity identity;

        if (!string.IsNullOrWhiteSpace(token))
        {
            // Token lze zpracovat – např. pomocí metody, která z tokenu vytáhne claimy
            identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
        }
        else
        {
            identity = new ClaimsIdentity();
        }

        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);
    }
    
    public static IEnumerable<Claim>? ParseClaimsFromJwt(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
        return securityToken?.Claims;
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        await _localStorage.SetItemAsync(Constants.AccessToken, token);
        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _localStorage.RemoveItemAsync(Constants.AccessToken);
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }
}