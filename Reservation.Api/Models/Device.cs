using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reservation.Api.Models;

public class Device
{
    [Key]
    public int Id { get; private set; }

    // Název zařízení nebo jiný unikátní identifikátor zařízení
    [Required]
    [StringLength(1000)]
    public string DeviceName { get; set; }

    // Refresh token specifický pro dané zařízení
    [Required]
    [StringLength(1000)]
    public string RefreshToken { get; set; }

    // Cizí klíč propojující zařízení s konkrétním majitelem
    [ForeignKey("Account")]
    public int AccountId { get; set; }
    public Account Account { get; set; }
}