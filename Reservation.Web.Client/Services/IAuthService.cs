using System.Net;
using Reservation.Shared.Dtos;

namespace Reservation.Web.Client.Services;

public interface IAuthService
{
    public Task<HttpStatusCode> RegisterAsync(RegistrationRequest registerRequest);
    public Task<HttpStatusCode> LoginAsync(LoginRequest loginRequest);
    public Task LogoutAsync();
    public Task LogoutAllDevicesAsync();
}