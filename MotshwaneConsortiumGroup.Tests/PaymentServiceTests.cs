using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.EfCore;
using MotshwaneConsortiumGroup.Services.Interfaces;
using Xunit;

namespace MotshwaneConsortiumGroup.Tests;

public class PaymentServiceTests
{
    private static async Task<Booking> AddPendingBooking(Data.ApplicationDbContext db)
    {
        var booking = new Booking
        {
            Reference = $"MC-PAY{Guid.NewGuid().ToString("N")[..4]}",
            CustomerId = TestDbFactory.FirstCustomerId(db),
            UnitId = TestDbFactory.FirstUnitId(db),
            BookingDate = DateTime.Today.AddDays(3),
            EndDate = DateTime.Today.AddDays(4),
            Location = "Idutywa",
            Status = BookingStatus.Pending,
            PaymentStatus = PaymentStatus.AwaitingProof,
        };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
        return booking;
    }

    [Fact]
    public async Task SubmitProofAsync_WithAValidReference_MovesBookingToProofSubmitted()
    {
        using var db = TestDbFactory.Create();
        var payments = new EfPaymentService(db);
        var booking = await AddPendingBooking(db);

        var result = await payments.SubmitProofAsync(booking.Reference, booking.CustomerId, "somefile.pdf");

        Assert.True(result.Success);
        Assert.Equal(PaymentStatus.ProofSubmitted, result.Value!.Status);
        Assert.Equal("somefile.pdf", result.Value.ProofOfPaymentPath);

        var reloaded = db.Bookings.First(b => b.Id == booking.Id);
        Assert.Equal(PaymentStatus.ProofSubmitted, reloaded.PaymentStatus);
    }

    [Fact]
    public async Task SubmitProofAsync_WithAnUnknownReference_Fails()
    {
        using var db = TestDbFactory.Create();
        var payments = new EfPaymentService(db);
        var customerId = TestDbFactory.FirstCustomerId(db);

        var result = await payments.SubmitProofAsync("MC-DOESNOTEXIST", customerId, "somefile.pdf");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task SubmitProofAsync_ForAnotherCustomersBooking_Fails()
    {
        using var db = TestDbFactory.Create();
        var payments = new EfPaymentService(db);
        var booking = await AddPendingBooking(db);

        // Wrong customer id — proves one customer can't submit proof against someone else's booking.
        var result = await payments.SubmitProofAsync(booking.Reference, booking.CustomerId + 999, "somefile.pdf");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task SubmitProofAsync_WhenAlreadyApproved_Fails()
    {
        using var db = TestDbFactory.Create();
        var payments = new EfPaymentService(db);
        var booking = await AddPendingBooking(db);
        booking.PaymentStatus = PaymentStatus.Approved;
        await db.SaveChangesAsync();

        var result = await payments.SubmitProofAsync(booking.Reference, booking.CustomerId, "somefile.pdf");

        Assert.False(result.Success);
        Assert.Contains("already paid", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ApproveAsync_OnASubmittedPayment_ConfirmsTheBooking()
    {
        using var db = TestDbFactory.Create();
        var payments = new EfPaymentService(db);
        var booking = await AddPendingBooking(db);
        var submitted = await payments.SubmitProofAsync(booking.Reference, booking.CustomerId, "somefile.pdf");

        var result = await payments.ApproveAsync(submitted.Value!.Id);

        Assert.True(result.Success);
        Assert.Equal(PaymentStatus.Approved, result.Value!.Status);

        var reloaded = db.Bookings.First(b => b.Id == booking.Id);
        Assert.Equal(BookingStatus.Confirmed, reloaded.Status);
        Assert.Equal("Paid", reloaded.PaymentStatus);
    }

    [Fact]
    public async Task ApproveAsync_WithNoProofSubmittedYet_Fails()
    {
        using var db = TestDbFactory.Create();
        var payments = new EfPaymentService(db);

        var result = await payments.ApproveAsync(-999);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task RejectAsync_OnASubmittedPayment_KeepsBookingPendingAndStoresReason()
    {
        using var db = TestDbFactory.Create();
        var payments = new EfPaymentService(db);
        var booking = await AddPendingBooking(db);
        var submitted = await payments.SubmitProofAsync(booking.Reference, booking.CustomerId, "somefile.pdf");

        var result = await payments.RejectAsync(submitted.Value!.Id, "Amount does not match");

        Assert.True(result.Success);
        Assert.Equal(PaymentStatus.Rejected, result.Value!.Status);
        Assert.Equal("Amount does not match", result.Value.RejectionReason);

        var reloaded = db.Bookings.First(b => b.Id == booking.Id);
        Assert.Equal(BookingStatus.Pending, reloaded.Status);
        Assert.Equal(PaymentStatus.Rejected, reloaded.PaymentStatus);
    }

    [Fact]
    public async Task RejectAsync_WithNoReason_Fails()
    {
        using var db = TestDbFactory.Create();
        var payments = new EfPaymentService(db);
        var booking = await AddPendingBooking(db);
        var submitted = await payments.SubmitProofAsync(booking.Reference, booking.CustomerId, "somefile.pdf");

        var result = await payments.RejectAsync(submitted.Value!.Id, "");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task ApproveAsync_ThenRejectAsync_OnTheSamePayment_Fails()
    {
        using var db = TestDbFactory.Create();
        var payments = new EfPaymentService(db);
        var booking = await AddPendingBooking(db);
        var submitted = await payments.SubmitProofAsync(booking.Reference, booking.CustomerId, "somefile.pdf");
        await payments.ApproveAsync(submitted.Value!.Id);

        var result = await payments.RejectAsync(submitted.Value.Id, "changed my mind");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task SubmitProofAsync_SetsAmountFromTheUnitsPrice()
    {
        using var db = TestDbFactory.Create();
        var payments = new EfPaymentService(db);
        var booking = await AddPendingBooking(db);
        var expectedPrice = db.Units.First(u => u.Id == booking.UnitId).PriceFrom;

        var result = await payments.SubmitProofAsync(booking.Reference, booking.CustomerId, "somefile.pdf");

        Assert.Equal(expectedPrice, result.Value!.Amount);
    }
}
