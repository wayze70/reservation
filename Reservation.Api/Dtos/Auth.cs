using System.ComponentModel.DataAnnotations;

namespace Reservation.Api.Dtos;

public class LoginRequest
{
    [Required] [EmailAddress] public string Email { get; set; }
    [Required] public string Password { get; set; }
}

public class RegistrationRequest
{
    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }
    [Required] [EmailAddress] public string Email { get; set; }
    [Required] public string Password { get; set; }
}

public class AuthResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; }
}