namespace Reservation.Api.Dtos;

public class ReservationCreateRequest
{
    public int Capacity { get; set; }
    public string? Title { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateOnly Day { get; set; }

    public bool IsAvailable { get; set; }
    // public DateOnly startDate { get; set; }
    // public int repeatCount { get; set; }
}

public class ReservationResponse
{
    public int Id { get; set; }
    public int Capacity { get; set; }
    public int CurrentCapacity { get; set; }
    public string Title { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateOnly DayOfWeek { get; set; }
    public bool IsAvailable { get; set; }
}

public class ReservationSignUpRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Mail { get; set; }
    public string Mobile { get; set; }
}

public class ReservationSignUpResponse
{
    public bool IsSuccess { get; set; }
}