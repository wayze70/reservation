using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using System.Text.Json;
using System.Text;
using Reservation.Shared.Dtos;  // předpokládaná umístění RefreshTokenRequest
using System.Threading.Tasks;

namespace Reservation.Web.Client.Services
{
    public class TokenRefreshHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IHttpClientFactory _httpClientFactory;

        public TokenRefreshHandler(ILocalStorageService localStorage, IHttpClientFactory httpClientFactory)
        {
            _localStorage = localStorage;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Odeslání původního požadavku
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Načtení refresh tokenu z local storage
                string? refreshToken = await _localStorage.GetItemAsync<string>("refreshToken", cancellationToken);
                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    // Vytvoříme HttpClient bez připojených handlerů
                    var client = _httpClientFactory.CreateClient("NoHandlerClient");

                    // Vytvoříme refresh požadavek
                    var refreshRequest = new RefreshTokenRequest { RefreshToken = refreshToken };

                    var refreshResponse = await client.PostAsJsonAsync("auth/refresh", refreshRequest, cancellationToken);
                    if (refreshResponse.IsSuccessStatusCode)
                    {
                        var newAccessToken = await refreshResponse.Content.ReadFromJsonAsync<string>(cancellationToken: cancellationToken);
                        if (!string.IsNullOrEmpty(newAccessToken))
                        {
                            // Uložíme nový access token do local storage
                            await _localStorage.SetItemAsync("accessToken", newAccessToken, cancellationToken);

                            // Klonujeme původní požadavek, nastavíme nový header a znovu odešleme
                            var newRequest = await CloneHttpRequestMessageAsync(request);
                            newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);

                            response.Dispose();
                            return await base.SendAsync(newRequest, cancellationToken);
                        }
                    }
                }
            }

            return response;
        }

        // Metoda pro klonování HttpRequestMessage, protože původní požadavek již nelze znovu použít
        private async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            // Zkopírujeme hlavičky
            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (request.Content != null)
            {
                var contentBytes = await request.Content.ReadAsByteArrayAsync();
                clone.Content = new ByteArrayContent(contentBytes);
                foreach (var header in request.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
            clone.Version = request.Version;
            return clone;
        }
    }
}
