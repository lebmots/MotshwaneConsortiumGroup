using System.ComponentModel.DataAnnotations;

namespace MotshwaneConsortiumGroup.DTOs;

public class CreateBookingDto
{
    [Required] public int CustomerId { get; set; }
    [Required] public int UnitId { get; set; }
    [Required] public DateTime BookingDate { get; set; }
    [Required] public DateTime EndDate { get; set; }
    [Required, StringLength(250)] public string Location { get; set; } = "";
    [StringLength(1000)] public string? Notes { get; set; }
}
