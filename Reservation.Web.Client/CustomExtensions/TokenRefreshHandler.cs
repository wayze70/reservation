using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Reservation.Shared.Dtos;

namespace Reservation.Web.Client.CustomExtensions
{
    public class TokenRefreshHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly NavigationManager _navigationManager;

        public TokenRefreshHandler(ILocalStorageService localStorage,
            IHttpClientFactory httpClientFactory,
            AuthenticationStateProvider authenticationStateProvider,
            NavigationManager navigationManager)
        {
            _localStorage = localStorage;
            _httpClientFactory = httpClientFactory;
            _authenticationStateProvider = authenticationStateProvider;
            _navigationManager = navigationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                string? refreshToken =
                    await _localStorage.GetItemAsync<string>(Constants.RefreshToken, cancellationToken);

                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    var client = _httpClientFactory.CreateClient("NoHandlerClient");
                    var refreshRequest = new RefreshTokenRequest { RefreshToken = refreshToken };
                    var refreshResponse =
                        await client.PostAsJsonAsync("auth/refresh", refreshRequest, cancellationToken);

                    if (refreshResponse.IsSuccessStatusCode)
                    {
                        string? newAccessToken =
                            await refreshResponse.Content.ReadFromJsonAsync<string>(
                                cancellationToken: cancellationToken);

                        if (!string.IsNullOrEmpty(newAccessToken))
                        {
                            await _localStorage.SetItemAsync(Constants.AccessToken, newAccessToken, cancellationToken);
                            var newRequest = await CloneHttpRequestMessageAsync(request);
                            newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);
                            response.Dispose();
                            return await base.SendAsync(newRequest, cancellationToken);
                        }
                    }
                    else
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(refreshToken))
                            {
                                await client.PostAsJsonAsync("auth/logout",
                                    new LogoutRequest { RefreshToken = refreshToken },
                                    cancellationToken: cancellationToken);
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

            return response;
        }

        private async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (request.Content != null)
            {
                byte[] contentBytes = await request.Content.ReadAsByteArrayAsync();
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