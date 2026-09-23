namespace MotshwaneConsortiumGroup.Models;

/// <summary>Input for IBookingService.CreateAsync — what a customer submits on the booking form.</summary>
public class NewBookingRequest
{
    public string CustomerName { get; set; } = "";
    public int ServiceItemId { get; set; }
    public string Location { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
