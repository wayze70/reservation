using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reservation.Api.Models;

public class Account
{
    [Key]
    public int Id { get; private set; }

    [Required]
    [StringLength(50)]
    public string Organization { get; set; }

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? Path { get; set; }

    // Vztah 1:n s entitou Device – každý Owner může mít více zařízení
    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public virtual ICollection<Owner> Owners { get; set; } = new List<Owner>();
}