using Reservation.Shared.Dtos;

namespace Reservation.Api.Services;

public interface IAccountService
{
    Task<List<AccountInfoResponse>> GetAccountsByEmailAsync(string email);
    public Task<string?> GetPathAsync(int accountId);
    public Task<string> SetPathAsync(PathRequest request, int accountId);
    public Task<bool> IsPathTakenAsync(string path);
    Task<AccountDescriptionResponse> GetAccountDescriptionAsync(string path);
    public Task<AccountInfoResponse> GetAccountInfoAsync(int accountId);
    Task<AccountInfoResponse> UpdateAccountInfoAsync(UpdateAccountInfoRequest request, int accountId);
    Task<bool> UpdatePasswordAsync(UpdatePasswordRequest request, int userId);
    Task<bool> DeleteAccountAsync(DeleteAccountRequest request, int accountId, int userId);
}