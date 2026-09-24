using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.Interfaces;

namespace MotshwaneConsortiumGroup.Services.InMemory;

public class InMemoryPaymentService : IPaymentService
{
    // Kept separately from DemoDataService (Lebone's file) so we don't need to edit it.
    // Swap for an EF Core-backed (or Firebase-backed) implementation later behind the same interface.
    private static readonly List<Payment> _payments = new();
    private readonly DemoDataService _data;

    public InMemoryPaymentService(DemoDataService data) => _data = data;

    public Task<IReadOnlyList<Payment>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Payment>>(_payments);

    public Task<Payment?> GetByBookingIdAsync(int bookingId) =>
        Task.FromResult(_payments.FirstOrDefault(p => p.BookingId == bookingId));

    public Task<OperationResult<Payment>> SubmitProofAsync(string bookingReference, string savedFileName)
    {
        var booking = _data.Bookings.FirstOrDefault(b =>
            b.Reference.Equals(bookingReference, StringComparison.OrdinalIgnoreCase));
        if (booking is null)
            return Task.FromResult(OperationResult<Payment>.Fail("No booking found with that reference."));

        if (booking.PaymentStatus == PaymentStatus.Approved)
            return Task.FromResult(OperationResult<Payment>.Fail("This booking is already paid."));

        var payment = _payments.FirstOrDefault(p => p.BookingId == booking.Id);
        if (payment is null)
        {
            payment = new Payment
            {
                Id = _payments.Count == 0 ? 1 : _payments.Max(p => p.Id) + 1,
                BookingId = booking.Id,
            };
            _payments.Add(payment);
        }

        payment.ProofFilePath = savedFileName;
        payment.Status = PaymentStatus.ProofSubmitted;
        payment.SubmittedAt = DateTime.UtcNow;
        payment.RejectionReason = null;

        booking.PaymentStatus = PaymentStatus.ProofSubmitted;

        return Task.FromResult(OperationResult<Payment>.Ok(payment));
    }

    public Task<OperationResult<Payment>> ApproveAsync(int paymentId)
    {
        var payment = _payments.FirstOrDefault(p => p.Id == paymentId);
        if (payment is null)
            return Task.FromResult(OperationResult<Payment>.Fail("Payment does not exist."));

        if (payment.Status != PaymentStatus.ProofSubmitted)
            return Task.FromResult(OperationResult<Payment>.Fail(
                $"Only a submitted proof can be approved (current status: {payment.Status})."));

        payment.Status = PaymentStatus.Approved;

        var booking = _data.Bookings.First(b => b.Id == payment.BookingId);
        booking.PaymentStatus = "Paid";
        booking.Status = BookingStatus.Confirmed;

        return Task.FromResult(OperationResult<Payment>.Ok(payment));
    }

    public Task<OperationResult<Payment>> RejectAsync(int paymentId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Task.FromResult(OperationResult<Payment>.Fail("A rejection reason is required."));

        var payment = _payments.FirstOrDefault(p => p.Id == paymentId);
        if (payment is null)
            return Task.FromResult(OperationResult<Payment>.Fail("Payment does not exist."));

        if (payment.Status != PaymentStatus.ProofSubmitted)
            return Task.FromResult(OperationResult<Payment>.Fail(
                $"Only a submitted proof can be rejected (current status: {payment.Status})."));

        payment.Status = PaymentStatus.Rejected;
        payment.RejectionReason = reason;

        var booking = _data.Bookings.First(b => b.Id == payment.BookingId);
        booking.PaymentStatus = PaymentStatus.Rejected;
        // Booking.Status stays Pending — customer can upload new proof and resubmit.

        return Task.FromResult(OperationResult<Payment>.Ok(payment));
    }
}
