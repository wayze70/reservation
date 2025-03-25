using Reservation.Shared.Dtos;

namespace Reservation.Web.Client.Services;

public interface IAuthService
{
    public Task<bool> RegisterAsync(RegistrationRequest registerRequest);
    public Task<bool> LoginAsync(LoginRequest loginRequest);
    public Task<string> RefreshAsync(string refreshToken);
    public Task LogoutAsync();
}