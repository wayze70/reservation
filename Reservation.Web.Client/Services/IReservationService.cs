using Reservation.Shared.Dtos;
using Reservation.Web.Client.CustomExtensions;

namespace Reservation.Web.Client.Services;

public interface IReservationService
{
    public Task<ApiResponse<ReservationResponse>> CreateAsync(ReservationCreateRequest request);
    public Task<List<ReservationResponse>> GetReservationsAsync();
}