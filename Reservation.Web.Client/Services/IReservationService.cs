using Reservation.Shared.Dtos;
using Reservation.Web.Client.CustomExtensions;

namespace Reservation.Web.Client.Services;

public interface IReservationService
{
    Task<ApiResponse<List<ReservationResponse>>> CreateAsync(List<ReservationCreateRequest> listRequest);
    Task<ApiResponse<List<ReservationResponse>>> GetActiveReservationsAsync(string path);
    Task<ApiResponse<List<ReservationResponse>>> GetReservationsAsync();
    Task<ApiResponse<ReservationResponse>> GetReservationAsync(string path, int reservationId);
    Task<ApiResponse<ReservationSignUpResponse>> SignInToReservation(int reservationId, 
        ReservationSignUpRequest request);
    Task<ApiResponse<ReservationResponseWithCustomers>> GetReservationWithUserAsync(int reservationId);
    Task<ApiResponse<ReservationResponse>> UpdateReservationAsync(ReservationCreateRequest request, int reservationId);
    Task<ApiResponse<bool>> DeleteReservationAsync(int reservationId);
    Task<ApiResponse<bool>> RemoveUserFromReservationAsync(RemoveCustomerFromReservationRequest request);
    Task<ApiResponse<ReservationResponse>> CancelReservationAsync(int reservationId, string cancellationCode);
}