using Reservation.Shared.Dtos;
using Reservation.Web.Client.CustomExtensions;

namespace Reservation.Web.Client.Services;

public class AccountService : IAccountService
{
    private readonly IHttpClientService _httpClientService;

    public AccountService(IHttpClientService httpClientService)
    {
        _httpClientService = httpClientService;
    }

    public async Task<ApiResponse<string>> GetPath()
    {
        return await _httpClientService.GetAsync<string>("account/path");
    }

    public async Task<ApiResponse<string>> UpdatePath(PathRequest request)
    {
        return await _httpClientService.PostAsync<PathRequest, string>("account/path", request);
    }

    public async Task<ApiResponse<bool>> IsPathTaken(PathRequest request)
    {
        return await _httpClientService.PostAsync<PathRequest, bool>("account/path/taken", request);
    }
}