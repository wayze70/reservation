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
    
    public async Task<ApiResponse<List<AccountInfoResponse>>> GetAccountsByEmail(AccountsByEmailRequest request)
    {
        return await _httpClientService.PostAsync<AccountsByEmailRequest ,List<AccountInfoResponse>>($"account/accounts-by-email", request);
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

    public async Task<ApiResponse<AccountInfoResponse>> GetAccountInfo()
    {
        return await _httpClientService.GetAsync<AccountInfoResponse>("account/account-info");
    }

    public async Task<ApiResponse<AccountInfoResponse>> UpdateAccountInfo(UpdateAccountInfoRequest request)
    {
        return await _httpClientService.PutAsync<UpdateAccountInfoRequest, AccountInfoResponse>
            ("account/account-info", request);
    }

    public async Task<ApiResponse<bool>> UpdatePassword(UpdatePasswordRequest request)
    {
        return await _httpClientService.PutAsync<UpdatePasswordRequest, bool>("account/update-password", request);
    }

    public async Task<ApiResponse<AccountDescriptionResponse>> GetAccountDescription(string path)
    {
        return await _httpClientService.GetAsync<AccountDescriptionResponse>("account/" + path);
    }

    public async Task<ApiResponse<bool>> DeleteAccount(DeleteAccountRequest request)
    {
        return await _httpClientService.PostAsync<DeleteAccountRequest, bool>("account/delete", request);
    }
    
    public async Task<ApiResponse<PathResponse>> GetAccountIdentifierByReservationId(int reservationId)
    {
        return await _httpClientService.GetAsync<PathResponse>($"account/get-account-path/{reservationId}");
    }
}