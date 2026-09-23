namespace MotshwaneConsortiumGroup.Models;
public class Booking
{
    public int Id { get; set; }
    public string Reference { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Service { get; set; } = "";
    public DateTime BookingDate { get; set; }
    /// <summary>Rental end date. Needed for the availability check; defaults to BookingDate for old seed data.</summary>
    public DateTime EndDate { get; set; }
    /// <summary>Links to ServiceItem.Id. Placeholder until Thato's Unit/Inventory entity exists.</summary>
    public int ServiceItemId { get; set; }
    public string Location { get; set; } = "";
    public string Status { get; set; } = "Pending";
    public string PaymentStatus { get; set; } = "Awaiting Proof";
}
