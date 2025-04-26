using System.Net.Http.Json;
using Reservation.Web.Client.CustomExtensions;

namespace Reservation.Web.Client.Services;

public class HttpClientService : IHttpClientService
{
    private readonly HttpClient _client;

    public HttpClientService(HttpClient client)
    {
        _client = client;
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string url) => await GetResponse<T>(await _client.GetAsync(url));

    public async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest data) =>
        await GetResponse<TResponse>(await _client.PostAsJsonAsync(url, data));

    public async Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string url, TRequest data) =>
        await GetResponse<TResponse>(await _client.PutAsJsonAsync(url, data));

    public async Task<ApiResponse<T>> DeleteAsync<T>(string url) => await GetResponse<T>(await _client.DeleteAsync(url));

    private static async Task<ApiResponse<T>> GetResponse<T>(HttpResponseMessage response)
    {
        var apiResponse = new ApiResponse<T>
        {
            IsSuccess = response.IsSuccessStatusCode,
            StatusCode = response.StatusCode
        };

        if (response.IsSuccessStatusCode)
        {
            if (response.Content.Headers.ContentLength > 0)
            {
                apiResponse.Data = await response.Content.ReadFromJsonAsync<T>();
            }
        }
        else
        {
            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
            {
                try
                {
                    apiResponse.Data = await response.Content.ReadFromJsonAsync<T>();
                }
                catch
                {
                    // ignored - not all error responses will have a body
                }
                
                apiResponse.ErrorMessage = await response.Content.ReadAsStringAsync();
            }
            else
            {
                apiResponse.ErrorMessage = "Omlouváme se, ale chyba je na naší stráně. Zkuste to prosím později.";
            }
        }

        return apiResponse;
    }
}