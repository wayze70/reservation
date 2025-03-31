using Reservation.Shared.Dtos;
using Reservation.Web.Client.CustomExtensions;

namespace Reservation.Web.Client.Services;

public interface IReservationService
{
    public Task<ApiResponse<ReservationResponse>> CreateAsync(ReservationCreateRequest request);
    public Task<ApiResponse<List<ReservationResponse>>> GetReservationsAsync(string path);
}