using System.ComponentModel.DataAnnotations;

namespace Reservation.Api.Models;

public class Reservation
{
    public int Id { get; private set; }
    public int OwnerId { get; set; } // Cizí klíč
    public int Capacity { get; set; }
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateOnly Date { get; set; }
    public bool IsAvailable { get; set; }
    
    public List<User> SignedUsers { get; set; } = new List<User>();
    public Owner Owner { get; set; }
}