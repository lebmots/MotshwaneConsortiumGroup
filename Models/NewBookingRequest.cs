namespace MotshwaneConsortiumGroup.Models;

/// <summary>Input for IBookingService.CreateAsync — what a customer submits on the booking form.</summary>
public class NewBookingRequest
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    /// <summary>The Unit being booked (named ServiceItemId for historical reasons — predates
    /// the Unit entity — but always maps to Unit.Id).</summary>
    public int ServiceItemId { get; set; }
    public string Location { get; set; } = "";
    public string? Notes { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
