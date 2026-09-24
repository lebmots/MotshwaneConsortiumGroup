namespace MotshwaneConsortiumGroup.Models;

/// <summary>Proof-of-payment record for a booking. One booking has at most one active payment.</summary>
public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string Status { get; set; } = MotshwaneConsortiumGroup.Models.PaymentStatus.AwaitingProof;
    /// <summary>Server-generated file name under the upload folder — never the customer's original name.</summary>
    public string? ProofFilePath { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? RejectionReason { get; set; }
}
