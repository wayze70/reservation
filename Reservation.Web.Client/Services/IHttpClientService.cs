namespace Reservation.Web.Client.Services;

public interface IHttpClientService
{
    Task<T> GetAsync<T>(string url);
    Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest data);
    Task<TResponse> PutAsync<TRequest, TResponse>(string url, TRequest data);
    Task DeleteAsync(string url);
}
