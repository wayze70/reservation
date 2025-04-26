using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reservation.Api.Models;

public class Reservation
{
    [Key]
    public int Id { get; private set; }

    [Required]
    public int AccountId { get; set; }

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

    [Required]
    public TimeSpan CancellationOffset { get; set; } = new TimeSpan(0, 0, 0);
    
    [Required]
    [MaxLength(100)]
    public string CustomTimeZoneId { get; set; } = string.Empty;

    [ForeignKey("AccountId")]
    public virtual Account Account { get; set; } = default!;
    
    [InverseProperty("Reservation")]
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}