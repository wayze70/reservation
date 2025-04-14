using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Reservation.Shared.Authorization;

namespace Reservation.Web.Client.CustomExtensions;

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
        string? token = await _localStorage.GetItemAsync<string>(Constants.AccessToken);

        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var claims = ParseClaimsFromJwt(token);
        if (claims == null)
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        // Create new identity with authentication type "jwt"
        var identity = new ClaimsIdentity(claims, "jwt");

        // Get role from claims and add it as a new Role claim
        var roleClaim = claims.FirstOrDefault(c => c.Type == ReservationClaimNames.Custom.Role);
        if (roleClaim != null && Enum.TryParse<Role>(roleClaim.Value, out var role))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role.ToString()));
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
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
        await _localStorage.RemoveItemAsync(Constants.RefreshToken);
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }
}