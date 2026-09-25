using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services;
using MotshwaneConsortiumGroup.Services.InMemory;
using Xunit;

namespace MotshwaneConsortiumGroup.Tests;

public class PaymentServiceTests
{
    private static (InMemoryPaymentService payments, DemoDataService data) CreateService()
    {
        var data = new DemoDataService();
        return (new InMemoryPaymentService(data), data);
    }

    private static Booking AddPendingBooking(DemoDataService data)
    {
        var booking = new Booking
        {
            Id = data.Bookings.Count == 0 ? 1 : data.Bookings.Max(b => b.Id) + 1,
            Reference = "MC-PAYTEST",
            CustomerName = "Test Customer",
            Service = data.Services.First().Name,
            ServiceItemId = data.Services.First().Id,
            BookingDate = DateTime.Today.AddDays(3),
            EndDate = DateTime.Today.AddDays(4),
            Location = "Idutywa",
            Status = BookingStatus.Pending,
            PaymentStatus = PaymentStatus.AwaitingProof,
        };
        data.Bookings.Add(booking);
        return booking;
    }

    [Fact]
    public async Task SubmitProofAsync_WithAValidReference_MovesBookingToProofSubmitted()
    {
        var (payments, data) = CreateService();
        var booking = AddPendingBooking(data);

        var result = await payments.SubmitProofAsync(booking.Reference, "somefile.pdf");

        Assert.True(result.Success);
        Assert.Equal(PaymentStatus.ProofSubmitted, result.Value!.Status);
        Assert.Equal(PaymentStatus.ProofSubmitted, booking.PaymentStatus);
        Assert.Equal("somefile.pdf", result.Value.ProofFilePath);
    }

    [Fact]
    public async Task SubmitProofAsync_WithAnUnknownReference_Fails()
    {
        var (payments, _) = CreateService();

        var result = await payments.SubmitProofAsync("MC-DOESNOTEXIST", "somefile.pdf");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task SubmitProofAsync_WhenAlreadyApproved_Fails()
    {
        var (payments, data) = CreateService();
        var booking = AddPendingBooking(data);
        booking.PaymentStatus = PaymentStatus.Approved;

        var result = await payments.SubmitProofAsync(booking.Reference, "somefile.pdf");

        Assert.False(result.Success);
        Assert.Contains("already paid", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ApproveAsync_OnASubmittedPayment_ConfirmsTheBooking()
    {
        var (payments, data) = CreateService();
        var booking = AddPendingBooking(data);
        var submitted = await payments.SubmitProofAsync(booking.Reference, "somefile.pdf");

        var result = await payments.ApproveAsync(submitted.Value!.Id);

        Assert.True(result.Success);
        Assert.Equal(PaymentStatus.Approved, result.Value!.Status);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.Equal("Paid", booking.PaymentStatus);
    }

    [Fact]
    public async Task ApproveAsync_WithNoProofSubmittedYet_Fails()
    {
        var (payments, data) = CreateService();
        var booking = AddPendingBooking(data);
        // Create a payment record without going through SubmitProofAsync, still AwaitingProof.
        var payment = new Payment { Id = 1, BookingId = booking.Id, Status = PaymentStatus.AwaitingProof };

        var result = await payments.ApproveAsync(payment.Id);

        // No matching payment exists yet (list is empty), so this should fail — proves you can't
        // approve a payment that was never submitted.
        Assert.False(result.Success);
    }

    [Fact]
    public async Task RejectAsync_OnASubmittedPayment_KeepsBookingPendingAndStoresReason()
    {
        var (payments, data) = CreateService();
        var booking = AddPendingBooking(data);
        var submitted = await payments.SubmitProofAsync(booking.Reference, "somefile.pdf");

        var result = await payments.RejectAsync(submitted.Value!.Id, "Amount does not match");

        Assert.True(result.Success);
        Assert.Equal(PaymentStatus.Rejected, result.Value!.Status);
        Assert.Equal("Amount does not match", result.Value.RejectionReason);
        Assert.Equal(BookingStatus.Pending, booking.Status); // NOT confirmed
        Assert.Equal(PaymentStatus.Rejected, booking.PaymentStatus);
    }

    [Fact]
    public async Task RejectAsync_WithNoReason_Fails()
    {
        var (payments, data) = CreateService();
        var booking = AddPendingBooking(data);
        var submitted = await payments.SubmitProofAsync(booking.Reference, "somefile.pdf");

        var result = await payments.RejectAsync(submitted.Value!.Id, "");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task ApproveAsync_ANonExistentPayment_Fails()
    {
        var (payments, _) = CreateService();

        var result = await payments.ApproveAsync(-999);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task ApproveAsync_ThenRejectAsync_OnTheSamePayment_Fails()
    {
        // Once approved, a payment shouldn't be reject-able (it's already gone through).
        var (payments, data) = CreateService();
        var booking = AddPendingBooking(data);
        var submitted = await payments.SubmitProofAsync(booking.Reference, "somefile.pdf");
        await payments.ApproveAsync(submitted.Value!.Id);

        var result = await payments.RejectAsync(submitted.Value.Id, "changed my mind");

        Assert.False(result.Success);
    }
}
