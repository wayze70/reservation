using Reservation.Shared.Dtos;
using Reservation.Web.Client.CustomExtensions;

namespace Reservation.Web.Client.Services;

public interface IAccountService
{
    public Task<ApiResponse<List<AccountInfoResponse>>> GetAccountsByEmail(AccountsByEmailRequest email);
    public Task<ApiResponse<string>> GetPath();
    public Task<ApiResponse<string>> UpdatePath(PathRequest request);
    public Task<ApiResponse<bool>> IsPathTaken(PathRequest request);
    public Task<ApiResponse<AccountInfoResponse>> GetAccountInfo();
    public Task<ApiResponse<AccountInfoResponse>> UpdateAccountInfo(UpdateAccountInfoRequest request);
    public Task<ApiResponse<bool>> UpdatePassword(UpdatePasswordRequest request);
    public Task<ApiResponse<AccountDescriptionResponse>> GetAccountDescription(string path);
    public Task<ApiResponse<bool>> DeleteAccount(DeleteAccountRequest request);
}