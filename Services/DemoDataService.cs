using MotshwaneConsortiumGroup.Models;
namespace MotshwaneConsortiumGroup.Services;
public class DemoDataService
{
    public List<ServiceItem> Services { get; } = new()
    {
        new() { Id=1, Name="Mobile Freezer", Category="Freezer", Description="Portable cold-storage solution for events and temporary sites.", PriceFrom=750, Available=true },
        new() { Id=2, Name="Mobile Toilet", Category="Toilet", Description="Clean and convenient mobile sanitation unit.", PriceFrom=500, Available=true },
        new() { Id=3, Name="Premium Freezer", Category="Freezer", Description="Large mobile freezer for higher-volume events.", PriceFrom=1200, Available=false }
    };
    public List<Booking> Bookings { get; } = new()
    {
        new() { Id=1, Reference="MC-001", CustomerName="Naledi Mokoena", Service="Mobile Freezer", BookingDate=new DateTime(2026,9,20), Location="Pretoria East", Status="Pending", PaymentStatus="Awaiting Proof" },
        new() { Id=2, Reference="MC-002", CustomerName="Kagiso Molefe", Service="Mobile Toilet", BookingDate=new DateTime(2026,9,28), Location="Centurion", Status="Confirmed", PaymentStatus="Paid" },
        new() { Id=3, Reference="MC-003", CustomerName="Thandi Ndlovu", Service="Mobile Freezer", BookingDate=new DateTime(2026,10,3), Location="Johannesburg", Status="In Progress", PaymentStatus="Paid" }
    };
    public List<Customer> Customers { get; } = new()
    {
        new() { Id=1, Name="Naledi Mokoena", Email="naledi@example.com", Phone="071 000 0001" },
        new() { Id=2, Name="Kagiso Molefe", Email="kagiso@example.com", Phone="071 000 0002" },
        new() { Id=3, Name="Thandi Ndlovu", Email="thandi@example.com", Phone="071 000 0003" }
    };
    public List<StaffJob> Jobs { get; } = new()
    {
        new() { Id=1, Reference="MC-002", Service="Mobile Toilet", Location="Centurion", Date=new DateTime(2026,9,28), Status="Assigned" },
        new() { Id=2, Reference="MC-003", Service="Mobile Freezer", Location="Johannesburg", Date=new DateTime(2026,10,3), Status="In Progress" }
    };
}
