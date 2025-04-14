using System.Globalization;
using Reservation.Shared.Dtos;

namespace Reservation.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(string email, string password, string idetifier, string devinceName);
    Task<AuthResponse> RegisterAsync(string firstName, string lastName, string organization, string idetifier, string email, 
        string password, string deviceName);
    Task<string> RefreshAsync(string refreshToken);
    Task<bool> LogoutAsync(string refreshToken);
    Task CleanupInvalidRefreshTokensAsync();
    Task<bool> LogoutAllDevicesAsync(string refreshToken);
}
