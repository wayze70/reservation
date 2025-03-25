using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reservation.Api.Models;

public class Owner
{
    [Key]
    public int Id { get; private set; }
    
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }
    
    [Required]
    [StringLength(50)]
    public string LastName { get; set; }
    
    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; set; }
    
    [Required]
    [StringLength(100)]
    public string PasswordHash { get; set; }
    [StringLength(1000)]
    public string? RefreshToken { get; set; }
    
    [StringLength(50)]
    public string? Path { get; set; }

    [InverseProperty("Owner")]
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}