namespace Reservation.Shared.Dtos;

public class ReservationCreateRequest
{
    public int Capacity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool IsAvailable { get; set; }
    
    public TimeSpan CancellationOffset { get; set; }
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

public class ReservationResponseWithUser
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
    public List<UserResponse> Users { get; set; } = new List<UserResponse>();
}

public class ReservationSignUpRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class RemoveUserFromReservationRequest
{
    public int ReservationId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
}

public class ReservationSignUpResponse
{
    public bool IsSuccess { get; set; }
}
