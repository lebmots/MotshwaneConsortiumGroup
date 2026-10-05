namespace MotshwaneConsortiumGroup.Models;

/// <summary>Allowed values for Booking.Status and the moves permitted between them.</summary>
public static class BookingStatus
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Cancelled = "Cancelled";
}

/// <summary>
/// Allowed values for StaffJob.Status, in the order they must happen.
/// Matches the prototype's Staff Workflow screens (Assigned -> In Progress -> Delivered/Collected -> Completed).
/// </summary>
public static class JobStatus
{
    public const string Assigned = "Assigned";
    public const string InProgress = "In Progress";
    public const string Delivered = "Delivered";
    public const string Completed = "Completed";

    private static readonly List<string> Order = new() { Assigned, InProgress, Delivered, Completed };

    /// <summary>True if a job can move from <paramref name="current"/> to <paramref name="next"/>.
    /// Only single forward steps are allowed; no skipping, no going backward.</summary>
    public static bool CanTransition(string current, string next)
    {
        var from = Order.IndexOf(current);
        var to = Order.IndexOf(next);
        if (from < 0 || to < 0) return false;
        return to == from + 1;
    }
}

/// <summary>
/// Allowed values for Payment.Status. AwaitingProof -> ProofSubmitted -> Approved/Rejected.
/// A Rejected payment can be resubmitted (back to ProofSubmitted) if the customer uploads new proof.
/// </summary>
public static class PaymentStatus
{
    public const string AwaitingProof = "Awaiting Proof";
    public const string ProofSubmitted = "Proof Submitted";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}
