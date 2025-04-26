using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reservation.Api.Models;

public class Device
{
    [Key]
    public int Id { get; private set; }

    [Required]
    [StringLength(1000)]
    public string DeviceName { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string RefreshToken { get; set; } = string.Empty;
    
    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }
    public virtual User User { get; set; }= default!;

    [ForeignKey("Account")]
    public int AccountId { get; set; }
    public Account Account { get; set; }= default!;
}