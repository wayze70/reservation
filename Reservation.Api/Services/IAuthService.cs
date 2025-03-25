using Reservation.Shared.Dtos;

namespace Reservation.Api.Services;

public interface IAuthService
{
    public AuthResponse Login(string email, string password);
    public AuthResponse Register(string firstName, string lastName, string email, string password);
    public Task<string> RefreshAsync(string refreshToken);
}