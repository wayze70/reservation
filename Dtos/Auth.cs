namespace ReservationApi.Dtos;

public abstract class Auth
{
    public abstract class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public abstract class RegisterRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
    
    public abstract class AuthResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}