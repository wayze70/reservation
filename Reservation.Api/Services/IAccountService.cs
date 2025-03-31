using Reservation.Shared.Dtos;

namespace Reservation.Api.Services;

public interface IAccountService
{
    public Task<string?> GetPathAsync(int ownerId);
    public Task<string> SetPathAsync(PathRequest request, int ownerId);
    public Task<bool> IsPathTakenAsync(string path);
}