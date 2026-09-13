namespace MotshwaneConsortiumGroup.Models;
public class Booking
{
    public int Id { get; set; }
    public string Reference { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Service { get; set; } = "";
    public DateTime BookingDate { get; set; }
    public string Location { get; set; } = "";
    public string Status { get; set; } = "Pending";
    public string PaymentStatus { get; set; } = "Awaiting Proof";
}
