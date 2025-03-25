using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace Reservation.Web.Client.Services;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public AuthTokenHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Načtení tokenu z local storage
        string? token = await _localStorage.GetItemAsync<string>(CustomExtensions.Constants.AccessToken, cancellationToken);
        if (!string.IsNullOrWhiteSpace(token))
        {
            // Přidání tokenu do hlavičky Authorization
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Pokračování v odesílání požadavku
        return await base.SendAsync(request, cancellationToken);
    }
}