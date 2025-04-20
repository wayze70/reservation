using Reservation.Shared.Dtos;
using Reservation.Web.Client.CustomExtensions;

namespace Reservation.Web.Client.Services;

public interface IReservationService
{
    public Task<ApiResponse<List<ReservationResponse>>> CreateAsync(List<ReservationCreateRequest> listRequest);
    public Task<ApiResponse<List<ReservationResponse>>> GetActiveReservationsAsync(string path);
    public Task<ApiResponse<List<ReservationResponse>>> GetReservationsAsync();
    public Task<ApiResponse<ReservationResponse>> GetReservationAsync(string path, int reservationId);
    public Task<ApiResponse<ReservationSignUpResponse>> SignInToReservation(int reservationId, 
        ReservationSignUpRequest request);
    public Task<ApiResponse<ReservationResponseWithCustomers>> GetReservationWithUserAsync(int reservationId);
    public Task<ApiResponse<ReservationResponse>> UpdateReservationAsync(ReservationCreateRequest request, int reservationId);
    public Task<ApiResponse<bool>> DeleteReservationAsync(int reservationId);
    public Task<ApiResponse<bool>> RemoveUserFromReservationAsync(RemoveCustomerFromReservationRequest request);
    public Task<ApiResponse<ReservationResponse>> CancelReservationAsync(int reservationId, string cancellationCode);
}