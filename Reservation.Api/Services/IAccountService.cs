using Reservation.Shared.Dtos;

namespace Reservation.Api.Services;

public interface IAccountService
{
    public Task<string?> GetPathAsync(int ownerId);
    public Task<string> SetPathAsync(PathRequest request, int ownerId);
    public Task<bool> IsPathTakenAsync(string path);
    Task<AccountDescriptionResponse> GetAccountDescriptionAsync(string path);
    public Task<AccountInfoResponse> GetAccountInfoAsync(int ownerId);
    Task<AccountInfoResponse> UpdateAccountInfoAsync(UpdateAccountInfoRequest request, int ownerId);
    Task<bool> UpdatePasswordAsync(UpdatePasswordRequest request, int ownerId);
    Task<bool> DeleteAccountAsync(DeleteAccountRequest request, int ownerId);
}