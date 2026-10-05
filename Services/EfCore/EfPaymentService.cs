using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Data;
using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.Interfaces;

namespace MotshwaneConsortiumGroup.Services.EfCore;

public class EfPaymentService : IPaymentService
{
    private readonly ApplicationDbContext _db;
    public EfPaymentService(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<Payment>> GetAllAsync() =>
        await _db.Payments.AsNoTracking().ToListAsync();

    public async Task<Payment?> GetByBookingIdAsync(int bookingId) =>
        await _db.Payments.AsNoTracking()
            .Where(p => p.BookingId == bookingId)
            .OrderByDescending(p => p.Id)
            .FirstOrDefaultAsync();

    public async Task<OperationResult<Payment>> SubmitProofAsync(string bookingReference, int customerId, string savedFileName)
    {
        var booking = await _db.Bookings
            .Include(b => b.Unit)
            .FirstOrDefaultAsync(b => b.Reference.ToLower() == bookingReference.ToLower() && b.CustomerId == customerId);

        if (booking is null)
            return OperationResult<Payment>.Fail("No booking found with that reference on your account.");

        if (booking.PaymentStatus == PaymentStatus.Approved)
            return OperationResult<Payment>.Fail("This booking is already paid.");

        var payment = await _db.Payments
            .Where(p => p.BookingId == booking.Id)
            .OrderByDescending(p => p.Id)
            .FirstOrDefaultAsync();

        if (payment is null || payment.Status == PaymentStatus.Rejected)
        {
            payment = new Payment { BookingId = booking.Id, Amount = booking.Unit.PriceFrom };
            _db.Payments.Add(payment);
        }

        payment.ProofOfPaymentPath = savedFileName;
        payment.Status = PaymentStatus.ProofSubmitted;
        payment.PaymentDate = DateTime.UtcNow;
        payment.RejectionReason = null;

        booking.PaymentStatus = PaymentStatus.ProofSubmitted;

        await _db.SaveChangesAsync();
        return OperationResult<Payment>.Ok(payment);
    }

    public async Task<OperationResult<Payment>> ApproveAsync(int paymentId)
    {
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
        if (payment is null)
            return OperationResult<Payment>.Fail("Payment does not exist.");

        if (payment.Status != PaymentStatus.ProofSubmitted)
            return OperationResult<Payment>.Fail($"Only a submitted proof can be approved (current status: {payment.Status}).");

        payment.Status = PaymentStatus.Approved;

        var booking = await _db.Bookings.FirstAsync(b => b.Id == payment.BookingId);
        booking.PaymentStatus = "Paid";
        booking.Status = BookingStatus.Confirmed;

        await _db.SaveChangesAsync();
        return OperationResult<Payment>.Ok(payment);
    }

    public async Task<OperationResult<Payment>> RejectAsync(int paymentId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return OperationResult<Payment>.Fail("A rejection reason is required.");

        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
        if (payment is null)
            return OperationResult<Payment>.Fail("Payment does not exist.");

        if (payment.Status != PaymentStatus.ProofSubmitted)
            return OperationResult<Payment>.Fail($"Only a submitted proof can be rejected (current status: {payment.Status}).");

        payment.Status = PaymentStatus.Rejected;
        payment.RejectionReason = reason;

        var booking = await _db.Bookings.FirstAsync(b => b.Id == payment.BookingId);
        booking.PaymentStatus = PaymentStatus.Rejected;

        await _db.SaveChangesAsync();
        return OperationResult<Payment>.Ok(payment);
    }
}
