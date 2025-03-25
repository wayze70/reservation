using Reservation.Shared.Dtos;

namespace Reservation.Web.Client.Services;

public class ReservationService : IReservationService
{

    private readonly IHttpClientService _httpClientService;

    public ReservationService(IHttpClientService httpClientService)
    {

        _httpClientService = httpClientService;
    }
    
    public async Task<ReservationResponse> CreateAsync(ReservationCreateRequest request)
    {
        return await _httpClientService.PostAsync<ReservationCreateRequest, ReservationResponse>("/reservation", request);
    }

    public async Task<List<ReservationResponse>> GetReservationsAsync()
    {
        throw new NotImplementedException();
    }
}