using Reservation.Shared.Dtos;
using Reservation.Web.Client.CustomExtensions;

namespace Reservation.Web.Client.Services;

public class ReservationService : IReservationService
{

    private readonly IHttpClientService _httpClientService;

    public ReservationService(IHttpClientService httpClientService)
    {
        _httpClientService = httpClientService;
    }

    public async Task<ApiResponse<List<ReservationResponse>>> CreateAsync(List<ReservationCreateRequest> listRequest)
    {
        return await _httpClientService.PostAsync<List<ReservationCreateRequest>, List<ReservationResponse>>("/reservation", listRequest);
    }

    public async Task<ApiResponse<List<ReservationResponse>>> GetActiveReservationsAsync(string path)
    {
        return await _httpClientService.GetAsync<List<ReservationResponse>>($"/reservation/public/{path}");
    }

    public async Task<ApiResponse<List<ReservationResponse>>> GetReservationsAsync()
    {
        return await _httpClientService.GetAsync<List<ReservationResponse>>($"/reservation/owner");
    }

    public async Task<ApiResponse<ReservationResponse>> GetReservationAsync(string path, int reservationId)
    {
        return await _httpClientService.GetAsync<ReservationResponse>($"/reservation/public/{path}/{reservationId}");
    }
    
    public async Task<ApiResponse<ReservationSignUpResponse>> SignInToReservation(int reservationId, ReservationSignUpRequest request)
    {
        return await _httpClientService.PostAsync<ReservationSignUpRequest, ReservationSignUpResponse>
            ($"/reservation/signup/{reservationId}", request);
    }

    public async Task<ApiResponse<ReservationResponseWithCustomers>> GetReservationWithUserAsync(int reservationId)
    {
        return await _httpClientService.GetAsync<ReservationResponseWithCustomers>($"/reservation/{reservationId}");
    }
    
    public async Task<ApiResponse<ReservationResponse>> UpdateReservationAsync(ReservationCreateRequest request, int reservationId)
    {
        return await _httpClientService.PutAsync<ReservationCreateRequest, ReservationResponse>($"/reservation/{reservationId}", request);
    }

    public async Task<ApiResponse<bool>> DeleteReservationAsync(int reservationId)
    {
        return await _httpClientService.DeleteAsync<bool>($"/reservation/{reservationId}");
    }

    public async Task<ApiResponse<bool>> RemoveUserFromReservationAsync(RemoveCustomerFromReservationRequest request)
    {
        return await _httpClientService.PostAsync<RemoveCustomerFromReservationRequest, bool>($"/reservation/remove-user", request);
    }

    public async Task<ApiResponse<ReservationResponse>> CancelReservationAsync(int reservationId, string cancellationCode)
    {
        return await _httpClientService.PostAsync<string, ReservationResponse>($"/reservation/cancel/{reservationId}", 
            cancellationCode);
    }
}