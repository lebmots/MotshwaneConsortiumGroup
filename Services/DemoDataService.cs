using MotshwaneConsortiumGroup.Models;
namespace MotshwaneConsortiumGroup.Services;
public class DemoDataService
{
    public List<Unit> Units { get; } = new()
    {
        new() { Id=1, Name="Mobile Freezer", Category="Freezer", Description="Portable cold-storage solution for events and temporary sites.", PriceFrom=750, Available=true },
        new() { Id=2, Name="Mobile Toilet", Category="Toilet", Description="Clean and convenient mobile sanitation unit.", PriceFrom=500, Available=true },
        new() { Id=3, Name="Premium Freezer", Category="Freezer", Description="Large mobile freezer for higher-volume events.", PriceFrom=1200, Available=false }
    };
    public List<Booking> Bookings { get; } = new()
    {
        new() { Id=1, Reference="MC-001", CustomerId=1, UnitId=1, BookingDate=new DateTime(2026,9,20), Location="Pretoria East", Status="Pending", PaymentStatus="Awaiting Proof" },
        new() { Id=2, Reference="MC-002", CustomerId=2, UnitId=2, BookingDate=new DateTime(2026,9,28), Location="Centurion", Status="Confirmed", PaymentStatus="Paid" },
        new () { Id=3, Reference="MC-003", CustomerId=3, UnitId=3, BookingDate=new DateTime(2026,10,3), Location="Johannesburg",Notes="Placeholder" ,Status="In Progress", PaymentStatus="Paid" }
        
    };
    public List<Customer> Customers { get; } = new()
    {
        new() { Id=1, UserId=1, Phone="071 000 0001" },
        new() { Id=2, UserId=2, Phone="071 000 0002" },
        new() { Id=3, UserId=3, Phone="071 000 0003" }
    };
    public List<StaffJob> Jobs { get; } = new()
    {
        new() { Id=1, BookingId= 1,StaffUserId=1,Status="Assigned", Notes="Initial assignment" },
        new() { Id=2, BookingId= 2,StaffUserId=2,Status="In Progress", Notes="Working on the job" },
        new () { Id=3, BookingId= 3, StaffUserId=3, Status="In Progress", Notes="Needs attention" }
    };
}
