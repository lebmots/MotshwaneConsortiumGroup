using System.ComponentModel.DataAnnotations;

namespace MotshwaneConsortiumGroup.DTOs;

public class UpdateBookingDto
{
    [Required] public DateTime BookingDate { get; set; }
    [Required] public DateTime EndDate { get; set; }
    [Required, StringLength(250)] public string Location { get; set; } = "";
    [StringLength(1000)] public string? Notes { get; set; }
    [Required] public string Status { get; set; } = "";
    [Required] public string PaymentStatus { get; set; } = "";
}
