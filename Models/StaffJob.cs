namespace MotshwaneConsortiumGroup.Models;
public class StaffJob
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int? StaffUserId { get; set; }
    public string Status { get; set; } = "Assigned";
    public string? Notes { get; set; }
    public Booking Booking { get; set; } = null!;
  
    
}
