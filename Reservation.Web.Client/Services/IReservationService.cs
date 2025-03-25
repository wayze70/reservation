using Reservation.Shared.Dtos;

namespace Reservation.Web.Client.Services;

public interface IReservationService
{
    public Task<ReservationResponse> CreateAsync(ReservationCreateRequest request);
    public Task<List<ReservationResponse>> GetReservationsAsync();
}