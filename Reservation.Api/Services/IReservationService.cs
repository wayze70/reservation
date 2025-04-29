using System.Globalization;
using Reservation.Shared.Dtos;

namespace Reservation.Api.Services
{
    public interface IReservationService
    {
        Task<ReservationResponse> CreateReservationAsync(ReservationCreateRequest request, int accountId);
        Task<List<ReservationResponse>> CreateReservationsAsync(List<ReservationCreateRequest> requests, int accountId);
        Task<List<ReservationResponse>> GetReservationsByAccountAsync(int accountId);
        Task<List<ReservationResponse>> GetActiveReservationsByPathAsync(string path);
        Task<ReservationResponse> GetReservationByPathAndIdAsync(string path, int reservationId);
        Task<ReservationResponseWithCustomers> GetReservationWithUsersAsync(int accountId, int reservationId);
        Task<ReservationResponse> UpdateReservationAsync(ReservationCreateRequest request, int reservationId);
        Task<bool> DeleteReservationAsync(int accountId, int reservationId);
        Task<ReservationResponse> SignUpForReservationAsync(int reservationId, ReservationSignUpRequest request, 
            CultureInfo cultureInfo);
        Task<ReservationResponse> CancelReservationAsync(int reservationId, string cancellationCode, CultureInfo cultureInfo);
        Task<bool> RemoveUserFromReservationAsync(int reservationId, string userEmail, CultureInfo cultureInfo);
        Task<bool> AccountOwnsReservationAsync(int accountId, int reservationId);
    }
}