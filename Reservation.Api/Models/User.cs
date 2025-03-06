using System.ComponentModel.DataAnnotations;

namespace Reservation.Api.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Mail { get; set; }
    public string Mobile { get; set; }
    public string Note { get; set; }
    
    public int ReservationId { get; set; } // Cizí klíč
    public global::Reservation.Api.Models.Reservation Reservation { get; set; }
}