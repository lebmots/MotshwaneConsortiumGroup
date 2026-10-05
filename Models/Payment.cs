namespace MotshwaneConsortiumGroup.Models;

public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = Models.PaymentStatus.AwaitingProof;
    public string? ProofOfPaymentPath { get; set; }
    public DateTime? PaymentDate { get; set; }
    /// <summary>Added on top of Thato's schema so the Week 4 reject flow can record why.</summary>
    public string? RejectionReason { get; set; }
    public Booking Booking { get; set; } = null!;
}
