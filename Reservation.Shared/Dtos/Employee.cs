// DTOs/EmployeeResponse.cs

using System.ComponentModel.DataAnnotations;
using Reservation.Shared.Authorization;

namespace Reservation.Shared.Dtos;

public class EmployeeResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public Role Role { get; set; }
}

// DTOs/EmployeeCreateRequest.cs
public class EmployeeCreateRequest
{
    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public string Password { get; set; }
    
    [Required]
    public Role Role { get; set; }
}

// DTOs/EmployeeUpdateRequest.cs
public class EmployeeUpdateRequest
{
    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public Role Role { get; set; }
}

public class EmployeeUpdateWithoutRoleRequest
{
    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }
}