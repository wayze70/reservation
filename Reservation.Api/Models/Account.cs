using System.ComponentModel.DataAnnotations;

namespace Reservation.Api.Models;

public class Account
{
    [Key]
    public int Id { get; private set; }

    [Required]
    [StringLength(50)]
    public string Organization { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Path { get; set; } = string.Empty;

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}