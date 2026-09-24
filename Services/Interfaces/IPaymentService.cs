using MotshwaneConsortiumGroup.Models;

namespace MotshwaneConsortiumGroup.Services.Interfaces;

public interface IPaymentService
{
    Task<IReadOnlyList<Payment>> GetAllAsync();

    Task<Payment?> GetByBookingIdAsync(int bookingId);

    /// <summary>Finds the booking by reference, records the already-saved proof file against it,
    /// and moves both the payment and the booking's PaymentStatus to Proof Submitted.</summary>
    Task<OperationResult<Payment>> SubmitProofAsync(string bookingReference, string savedFileName);

    /// <summary>Approves the payment: Payment -> Approved, Booking.PaymentStatus -> Paid, Booking.Status -> Confirmed.</summary>
    Task<OperationResult<Payment>> ApproveAsync(int paymentId);

    /// <summary>Rejects the payment with a reason. Booking stays Pending so the customer can resubmit.</summary>
    Task<OperationResult<Payment>> RejectAsync(int paymentId, string reason);
}
