using System.ComponentModel.DataAnnotations;

namespace Reservation.Api.Models;

public class ReservationReminder
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    [StringLength(320)]
    public string CustomerEmail { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}