using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reservation.Api.Models;

public class Customer
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Note { get; set; }
    
    [Required]
    [StringLength(100)]
    public string CancellationCode { get; set; } = string.Empty;
    
    [Required]
    public int ReservationId { get; set; }

    [ForeignKey("ReservationId")]
    [InverseProperty("Customers")]
    public Reservation Reservation { get; set; } = default!;
}