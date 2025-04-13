using System.ComponentModel.DataAnnotations;

namespace Reservation.Shared.Dtos;

public class PathRequest
{
        public string Path { get; set; } = string.Empty;
}

public class AccountDescriptionResponse
{
        public string Organization { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
}

public class UpdateAccountInfoRequest
{
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
}

public class AccountInfoResponse
{
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
}

public class UpdatePasswordRequest
{
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
}

public class DeleteAccountRequest
{
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
}
