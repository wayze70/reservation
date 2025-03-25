using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reservation.Api.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    
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
    
    [StringLength(500)]
    public string? Note { get; set; }
    
    // Unikátní kód pro zrušení rezervace – generuj např. jako GUID při vytváření záznamu Guid.NewGuid().ToString()
    [Required]
    [StringLength(100)]
    public string CancellationCode { get; set; }
    
    [Required]
    public int ReservationId { get; set; }

    [ForeignKey("ReservationId")]
    [InverseProperty("SignedUsers")]
    public Reservation Reservation { get; set; } = default!;
}