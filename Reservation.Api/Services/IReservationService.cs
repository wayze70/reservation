using Reservation.Api.Dtos;

namespace Reservation.Api.Services;

public interface IReservationService
{
    public ReservationResponse Create(ReservationCreateRequest request, int ownerId);
    public List<ReservationResponse> Get(int ownerId);
    public ReservationSignUpResponse SignUp(int reservationId, ReservationSignUpRequest user);
}