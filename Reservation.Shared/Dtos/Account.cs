using System.ComponentModel.DataAnnotations;
using Reservation.Shared.Common;

namespace Reservation.Shared.Dtos;

public class AccountsByEmailRequest
{
    [Required]
    [EmailAddress]
    [RegularExpression(Utils.EmailRegexPattern)]
    public string Email { get; set; } = string.Empty;
}

public class PathRequest
{
    [Required]
    public string Path { get; set; } = string.Empty;
}

public class AccountDescriptionResponse
{
    public string Organization { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateAccountInfoRequest
{
    public string Organization { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class AccountInfoResponse
{
    public string Organization { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
}

public class UpdatePasswordRequest
{
    [Required]
    public string OldPassword { get; set; } = string.Empty;

    [Required]
    public string NewPassword { get; set; } = string.Empty;
}

public class DeleteAccountRequest
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}