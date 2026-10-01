namespace MotshwaneConsortiumGroup.Models;
public class Booking
{
    public int Id { get; set; }
    public string Reference { get; set; } = "";
    public int CustomerId { get; set; }// Foreign key to the Customer model
    public int UnitId { get; set; } // Foreign key to the Unit model

    //public string CustomerName { get; set; } = ""; use Customer foreign key instead
    //public string Service { get; set; } = ""; use Unit foreign key instead
    public DateTime BookingDate { get; set; }
    public string Location { get; set; } = "";
    public string? Notes { get; set; }
    public string Status { get; set; } = "Pending";
    public string PaymentStatus { get; set; } = "Awaiting Proof";
    public Customer Customer { get; set; } = null!;
    public Unit Unit { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
