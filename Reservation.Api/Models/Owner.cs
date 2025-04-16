using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Reservation.Shared.Authorization;

namespace Reservation.Api.Models
{
    public class Owner
    {
        [Key]
        public int Id { get; private set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(320)]
        public string Email { get; set; }
        
        [Required]
        [StringLength(100)]
        public string PasswordHash { get; set; }
        
        public Role Role { get; set; }
        
        [Required]
        public int AccountId { get; set; }
        
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
    }
}
