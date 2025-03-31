using Reservation.Shared.Dtos;

namespace Reservation.Api.Services;

public interface IReservationService
{
    public Task<ReservationResponse> CreateAsync(ReservationCreateRequest request, int ownerId);
    public Task<List<ReservationResponse>> GetAsync(int ownerId);
    public Task<List<ReservationResponse>> GetAsync(string path);
    public Task<ReservationResponse> GetAsync(string path, int reservationId);
    public Task<ReservationResponse> SignUpAsync(int reservationId, ReservationSignUpRequest user);
    public Task<ReservationResponse> CancelReservationAsync(int reservationId, string cancelationCode);
    public Task<ReservationResponseWithUser> GetWithUserAsync(int ownerId, int reservationId);
    public Task<ReservationResponse> UpdateAsync(ReservationCreateRequest request, int reservationId);
    public Task<bool> DeleteAsync(int ownerId, int reservationId);
    public Task<bool> RemoveUserFromReservationAsync(int reservation, string userEmail);
    
    public Task<bool> OwnerOwnsReservation(int ownerId, int reservationId);
}