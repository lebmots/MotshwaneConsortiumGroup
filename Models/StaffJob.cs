namespace MotshwaneConsortiumGroup.Models;
public class StaffJob
{
    public int Id { get; set; }
    public string Reference { get; set; } = "";
    public string Service { get; set; } = "";
    public string Location { get; set; } = "";
    public DateTime Date { get; set; }
    /// <summary>Links back to the Booking this job was created from.</summary>
    public int BookingId { get; set; }
    /// <summary>Links to Staff.Id.</summary>
    public int StaffId { get; set; }
    public string Status { get; set; } = "Assigned";
}
