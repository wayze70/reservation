using Reservation.Shared.Dtos;

namespace Reservation.Api.Services;

public interface IAccountService
{
    Task<List<AccountInfoResponse>> GetAccountsByEmailAsync(string email);
    Task<string?> GetPathAsync(int accountId);
    Task<string> SetPathAsync(PathRequest request, int accountId);
    Task<bool> IsPathTakenAsync(string path);
    Task<AccountDescriptionResponse> GetAccountDescriptionAsync(string path);
    Task<PathResponse> GetAccountIdentifierByReservationId(int reservationId);
    Task<AccountInfoResponse> GetAccountInfoAsync(int accountId);
    Task<AccountInfoResponse> UpdateAccountInfoAsync(UpdateAccountInfoRequest request, int accountId);
    Task<bool> UpdatePasswordAsync(UpdatePasswordRequest request, int userId);
    Task<bool> DeleteAccountAsync(DeleteAccountRequest request, int accountId, int userId);
}