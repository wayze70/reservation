using System.ComponentModel.DataAnnotations;
using Reservation.Shared.Common;

namespace Reservation.Shared.Dtos;

public class ReservationCreateRequest
{
    [Required]
    public int Capacity { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Required]
    public DateTime Start { get; set; }
    [Required]
    public DateTime End { get; set; }
    [Required]
    public bool IsAvailable { get; set; }
    [Required]
    public TimeSpan CancellationOffset { get; set; }
    [Required]
    public string CustomTimeZoneId { get; set; } = string.Empty;
}

public class ReservationResponse
{
    public int Id { get; set; }
    public int Capacity { get; set; }
    public int CurrentCapacity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    // Poznámka: pokud se jedná o datum rezervace, doporučuji název změnit na "Date" nebo "ReservationDate"
    public bool IsAvailable { get; set; }
    
    // Nové vlastnosti
    public TimeSpan CancellationOffset { get; set; }
    public string CustomTimeZoneId { get; set; } = string.Empty;
}

public class ReservationResponseWithCustomers
{
    public int Id { get; set; }
    public int Capacity { get; set; }
    public int CurrentCapacity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    // Poznámka: pokud se jedná o datum rezervace, doporučuji název změnit na "Date" nebo "ReservationDate"
    public bool IsAvailable { get; set; }
    
    // Nové vlastnosti
    public TimeSpan CancellationOffset { get; set; }
    public string CustomTimeZoneId { get; set; } = string.Empty;
    public List<CustomerResponse> Users { get; set; } = [];
}

public class ReservationSignUpRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    [RegularExpression(Utils.EmailRegexPattern)]
    public string Email { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}

public class RemoveCustomerFromReservationRequest
{
    [Required]
    public int ReservationId { get; set; }
    [Required]
    [EmailAddress]
    [RegularExpression(Utils.EmailRegexPattern)]
    public string UserEmail { get; set; } = string.Empty;
}

public class ReservationSignUpResponse
{
    public bool IsSuccess { get; set; }
}
