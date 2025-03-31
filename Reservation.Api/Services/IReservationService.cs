using Reservation.Shared.Dtos;

namespace Reservation.Api.Services;

public interface IReservationService
{
    public Task<ReservationResponse> CreateAsync(ReservationCreateRequest request, int ownerId);
    public Task<List<ReservationResponse>> GetAsync(int ownerId);
    public Task<List<ReservationResponse>> GetAsync(string path);
    public Task<ReservationSignUpResponse> SignUpAsync(int reservationId, ReservationSignUpRequest user);
}