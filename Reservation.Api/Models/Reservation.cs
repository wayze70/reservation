using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Reservation.Shared.Dtos;

namespace Reservation.Api.Models;

public class Reservation
{
    [Key]
    public int Id { get; private set; }

    [Required]
    public int OwnerId { get; set; }

    [Required]
    public int Capacity { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime StartTime { get; set; } = new DateTime().ToUniversalTime();

    [Required]
    public DateTime EndTime { get; set; } = new DateTime().ToUniversalTime();

    [Required]
    public bool IsAvailable { get; set; }

    // Nové vlastnosti pro přihlašovací interval a možnost zrušení rezervace
    [Required]
    public TimeSpan CancellationOffset { get; set; } = new TimeSpan(0, 0, 0);
    
    // od -12UTC po +14UTC
    [Required]
    [MaxLength(100)]
    public string CustomTimeZoneId { get; set; } = string.Empty;

    [ForeignKey("OwnerId")]
    public Owner Owner { get; set; } = default!;
    
    [InverseProperty("Reservation")]
    public List<User> SignedUsers { get; set; } = new List<User>();
}