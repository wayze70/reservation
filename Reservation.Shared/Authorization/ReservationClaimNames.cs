using System.IdentityModel.Tokens.Jwt;

namespace Reservation.Shared.Authorization;

public struct ReservationClaimNames
{
    public const string Sub = JwtRegisteredClaimNames.Sub;
    public const string Email = JwtRegisteredClaimNames.Email;
    public const string GivenName = JwtRegisteredClaimNames.GivenName;
    public const string FamilyName = JwtRegisteredClaimNames.FamilyName;
    
    public struct Custom
    {
        public const string GeneratedNumber = "gen_num";
        public const string DeviceName = "device_name";
        public const string AccountId = "aid";
        public const string Role = "role";
    }
}