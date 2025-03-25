using Reservation.Shared.Dtos;

namespace Reservation.Api.Services;

public interface IAccountService
{
    public Task SetPathAsync(PathRequest request, int ownerId);
}