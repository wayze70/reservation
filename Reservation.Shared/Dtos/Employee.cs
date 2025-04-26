using System.ComponentModel.DataAnnotations;
using Reservation.Shared.Authorization;
using Reservation.Shared.Common;

namespace Reservation.Shared.Dtos;

public class EmployeeResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
}

// DTOs/EmployeeCreateRequest.cs
public class EmployeeCreateRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [RegularExpression(Utils.EmailRegexPattern)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    public Role Role { get; set; }
}

// DTOs/EmployeeUpdateRequest.cs
public class EmployeeUpdateRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [RegularExpression(Utils.EmailRegexPattern)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public Role Role { get; set; }
}

public class EmployeeUpdateWithoutRoleRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [RegularExpression(Utils.EmailRegexPattern)]
    public string Email { get; set; } = string.Empty;
}