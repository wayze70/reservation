using System.Net.Http.Json;

namespace Reservation.Web.Client.Services;

public class HttpClientService : IHttpClientService
{
    private readonly HttpClient _client;


    public HttpClientService(HttpClient client)
    {
        _client = client;

    }
    
    public async Task<T> GetAsync<T>(string url)
    {
        var response = await _client.GetAsync(url);
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest data)
    {
        var response = await _client.PostAsJsonAsync(url, data);
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public async Task<TResponse> PutAsync<TRequest, TResponse>(string url, TRequest data)
    {
        var response = await _client.PutAsJsonAsync(url, data);
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public async Task DeleteAsync(string url)
    {
        await _client.DeleteAsync(url);
    }
}