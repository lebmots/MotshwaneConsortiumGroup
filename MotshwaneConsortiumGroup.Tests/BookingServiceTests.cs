using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.EfCore;
using MotshwaneConsortiumGroup.Services.Interfaces;
using Xunit;

namespace MotshwaneConsortiumGroup.Tests;

public class BookingServiceTests
{
    private static NewBookingRequest ValidRequest(int customerId, int unitId, DateTime? start = null, DateTime? end = null) => new()
    {
        CustomerId = customerId,
        ServiceItemId = unitId,
        Location = "Idutywa",
        StartDate = start ?? DateTime.Today.AddDays(5),
        EndDate = end ?? DateTime.Today.AddDays(6),
    };

    [Fact]
    public async Task CreateAsync_WithValidRequest_SavesBookingAsPending()
    {
        using var db = TestDbFactory.Create();
        var service = new EfBookingService(db);
        var customerId = TestDbFactory.FirstCustomerId(db);
        var unitId = TestDbFactory.FirstUnitId(db);

        var result = await service.CreateAsync(ValidRequest(customerId, unitId));

        Assert.True(result.Success);
        Assert.Equal(BookingStatus.Pending, result.Value!.Status);
        Assert.Contains(db.Bookings, b => b.Id == result.Value.Id);
    }

    [Fact]
    public async Task CreateAsync_GeneratesAUniqueReference()
    {
        using var db = TestDbFactory.Create();
        var service = new EfBookingService(db);
        var customerId = TestDbFactory.FirstCustomerId(db);
        var unitId = TestDbFactory.FirstUnitId(db);

        var first = await service.CreateAsync(ValidRequest(customerId, unitId, DateTime.Today.AddDays(1), DateTime.Today.AddDays(2)));
        var second = await service.CreateAsync(ValidRequest(customerId, unitId, DateTime.Today.AddDays(10), DateTime.Today.AddDays(11)));

        Assert.NotEqual(first.Value!.Reference, second.Value!.Reference);
    }

    [Fact]
    public async Task CreateAsync_WithMissingLocation_Fails()
    {
        using var db = TestDbFactory.Create();
        var service = new EfBookingService(db);
        var request = ValidRequest(TestDbFactory.FirstCustomerId(db), TestDbFactory.FirstUnitId(db));
        request.Location = "";

        var result = await service.CreateAsync(request);

        Assert.False(result.Success);
        Assert.Contains("location", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_WithPastStartDate_Fails()
    {
        using var db = TestDbFactory.Create();
        var service = new EfBookingService(db);
        var request = ValidRequest(TestDbFactory.FirstCustomerId(db), TestDbFactory.FirstUnitId(db),
            DateTime.Today.AddDays(-1), DateTime.Today);

        var result = await service.CreateAsync(request);

        Assert.False(result.Success);
        Assert.Contains("past", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_WithEndDateNotAfterStartDate_Fails()
    {
        using var db = TestDbFactory.Create();
        var service = new EfBookingService(db);
        var sameDay = DateTime.Today.AddDays(3);
        var request = ValidRequest(TestDbFactory.FirstCustomerId(db), TestDbFactory.FirstUnitId(db), sameDay, sameDay);

        var result = await service.CreateAsync(request);

        Assert.False(result.Success);
        Assert.Contains("end date", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_WithUnknownUnit_Fails()
    {
        using var db = TestDbFactory.Create();
        var service = new EfBookingService(db);
        var request = ValidRequest(TestDbFactory.FirstCustomerId(db), unitId: -999);

        var result = await service.CreateAsync(request);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateAsync_WithUnknownCustomer_Fails()
    {
        using var db = TestDbFactory.Create();
        var service = new EfBookingService(db);
        var request = ValidRequest(customerId: -999, unitId: TestDbFactory.FirstUnitId(db));

        var result = await service.CreateAsync(request);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateAsync_WhenDatesOverlapAnExistingBooking_Fails()
    {
        using var db = TestDbFactory.Create();
        var service = new EfBookingService(db);
        var customerId = TestDbFactory.FirstCustomerId(db);
        var unitId = TestDbFactory.FirstUnitId(db);
        var start = DateTime.Today.AddDays(5);
        var end = DateTime.Today.AddDays(8);

        var first = await service.CreateAsync(ValidRequest(customerId, unitId, start, end));
        Assert.True(first.Success);

        var overlapping = ValidRequest(customerId, unitId, DateTime.Today.AddDays(6), DateTime.Today.AddDays(7));
        var second = await service.CreateAsync(overlapping);

        Assert.False(second.Success);
        Assert.Contains("not available", second.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_WhenDatesDoNotOverlap_Succeeds()
    {
        using var db = TestDbFactory.Create();
        var service = new EfBookingService(db);
        var customerId = TestDbFactory.FirstCustomerId(db);
        var unitId = TestDbFactory.FirstUnitId(db);

        var first = await service.CreateAsync(ValidRequest(customerId, unitId, DateTime.Today.AddDays(1), DateTime.Today.AddDays(2)));
        Assert.True(first.Success);

        // Starts exactly when the first booking ends — back-to-back, not overlapping.
        var backToBack = ValidRequest(customerId, unitId, DateTime.Today.AddDays(2), DateTime.Today.AddDays(3));
        var second = await service.CreateAsync(backToBack);

        Assert.True(second.Success);
    }

    [Fact]
    public async Task CreateAsync_ForADifferentUnit_IgnoresOtherUnitsBookings()
    {
        using var db = TestDbFactory.Create();
        var service = new EfBookingService(db);
        var customerId = TestDbFactory.FirstCustomerId(db);
        var start = DateTime.Today.AddDays(5);
        var end = DateTime.Today.AddDays(8);

        var first = await service.CreateAsync(ValidRequest(customerId, TestDbFactory.FirstUnitId(db), start, end));
        Assert.True(first.Success);

        // Same dates, but a different unit — should not be blocked by the first booking.
        var second = await service.CreateAsync(ValidRequest(customerId, TestDbFactory.SecondUnitId(db), start, end));

        Assert.True(second.Success);
    }

    [Fact]
    public async Task IsAvailableAsync_ExcludingABookingsOwnId_IgnoresItself()
    {
        using var db = TestDbFactory.Create();
        var service = new EfBookingService(db);
        var customerId = TestDbFactory.FirstCustomerId(db);
        var unitId = TestDbFactory.FirstUnitId(db);

        var created = await service.CreateAsync(ValidRequest(customerId, unitId, DateTime.Today.AddDays(5), DateTime.Today.AddDays(8)));

        var available = await service.IsAvailableAsync(
            unitId, DateTime.Today.AddDays(5), DateTime.Today.AddDays(8), created.Value!.Id);

        Assert.True(available);
    }
}
