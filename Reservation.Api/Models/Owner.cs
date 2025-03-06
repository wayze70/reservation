using System.ComponentModel.DataAnnotations;

namespace Reservation.Api.Models;

public class Owner
{
    [Key]
    public int Id { get; private set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string? RefreshToken { get; set; }

    // Navigační vlastnost pro vztah 1:N
    public ICollection<Reservation> Reservation { get; set; } = new List<Reservation>();
}
