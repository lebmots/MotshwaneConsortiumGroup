using System.ComponentModel.DataAnnotations.Schema;

namespace MotshwaneConsortiumGroup.Models;

public class Booking
{
    public int Id { get; set; }
    public string Reference { get; set; } = "";
    public int CustomerId { get; set; }
    public int UnitId { get; set; }
    public DateTime BookingDate { get; set; }
    /// <summary>Rental end date, added on top of Thato's schema so the Week 3 date-range
    /// availability check (not just same-day) keeps working. See the AddBookingEndDate migration.</summary>
    public DateTime EndDate { get; set; }
    public string Location { get; set; } = "";
    public string? Notes { get; set; }
    public string Status { get; set; } = BookingStatus.Pending;
    public string PaymentStatus { get; set; } = Models.PaymentStatus.AwaitingProof;
    public Customer Customer { get; set; } = null!;
    public Unit Unit { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    // Convenience accessors so the Week 1-4 controllers/views (written against a flatter,
    // pre-database model) keep working unchanged against the real relational schema.
    [NotMapped] public string CustomerName => Customer?.Name ?? "";
    [NotMapped] public string Service => Unit?.Name ?? "";
    [NotMapped] public int ServiceItemId => UnitId;
}
