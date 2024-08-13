using Microsoft.EntityFrameworkCore;
using User = ReservationApi.Dtos.User;

namespace ReservationApi.Services;

public class AuthService : IAuthService
{
    private readonly DataContext _context;

    public AuthService(DataContext context)
    {
        _context = context;
    }

    public User Login(string email, string password)
    {
        var user = _context.User.FirstOrDefaultAsync(user => user.Email == email && user.PasswordHash ==
            password).Result;

        if (user is null)
        {
            throw new Exception("User not found");
        }

        return new User
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };
    }

    public User Register(string firstName, string lastName, string email, string password)
    {
        if (_context.User.Any(user => user.Email == email))
        {
            throw new Exception("Email already exists");
        }

        var createdUser = _context.User.Add(new Models.User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = password
        }).Entity;
        
        _context.SaveChanges();
        
        return new User
        {
            Id = createdUser.Id,
            FirstName = createdUser.FirstName,
            LastName = createdUser.LastName,
            Email = createdUser.Email
        };
    }
}