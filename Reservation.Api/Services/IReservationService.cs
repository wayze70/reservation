using System.Globalization;
using Reservation.Shared.Dtos;

namespace Reservation.Api.Services
{
    public interface IReservationService
    {
        // Vytváření
        Task<ReservationResponse> CreateReservationAsync(ReservationCreateRequest request, int ownerId);
        Task<List<ReservationResponse>> CreateReservationsAsync(List<ReservationCreateRequest> requests, int ownerId);

        // Načítání rezervací
        Task<List<ReservationResponse>> GetReservationsByOwnerAsync(int ownerId);
        Task<List<ReservationResponse>> GetReservationsByPathAsync(string path);
        Task<ReservationResponse> GetReservationByPathAndIdAsync(string path, int reservationId);
        Task<ReservationResponseWithUser> GetReservationWithUsersAsync(int ownerId, int reservationId);

        // Úpravy a správa rezervací
        Task<ReservationResponse> UpdateReservationAsync(ReservationCreateRequest request, int reservationId);
        Task<bool> DeleteReservationAsync(int ownerId, int reservationId);

        // Přihlášení/zrušení rezervace
        Task<ReservationResponse> SignUpForReservationAsync(int reservationId, ReservationSignUpRequest request, 
            CultureInfo cultureInfo);
        Task<ReservationResponse> CancelReservationAsync(int reservationId, string cancellationCode, CultureInfo cultureInfo);
        Task<bool> RemoveUserFromReservationAsync(int reservationId, string userEmail);
        Task<bool> OwnerOwnsReservationAsync(int ownerId, int reservationId);
    }
}