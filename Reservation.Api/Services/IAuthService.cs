using System.Globalization;
using Reservation.Shared.Dtos;

namespace Reservation.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(string email, string password);
    Task<AuthResponse> RegisterAsync(string firstName, string lastName, string organization, string email, string 
        password);
    Task<string> RefreshAsync(string refreshToken);
}
