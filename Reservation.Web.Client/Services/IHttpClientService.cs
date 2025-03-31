using Reservation.Web.Client.CustomExtensions;

namespace Reservation.Web.Client.Services;

public interface IHttpClientService
{
    Task<ApiResponse<T>> GetAsync<T>(string url);
    Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest data);
    Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string url, TRequest data);
    Task<ApiResponse<T>> DeleteAsync<T>(string url);
}
