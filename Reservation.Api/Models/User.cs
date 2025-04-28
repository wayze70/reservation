using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Reservation.Shared.Authorization;

namespace Reservation.Api.Models
{
    public class User
    {
        [Key]
        public int Id { get; private set; }

        [Required]
        [StringLength(250)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(250)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(320)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        public Role Role { get; set; }
        
        [Required]
        public int AccountId { get; set; }
        
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; } = default!;
        
        public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
    }
}
