using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Data;
using MotshwaneConsortiumGroup.Models;

namespace MotshwaneConsortiumGroup.Tests;

/// <summary>
/// Builds a fresh, isolated in-memory ApplicationDbContext for one test, pre-seeded with the
/// roles/units/customer that most tests need. Each test gets its own database (a unique Guid
/// name), so tests never see each other's data even though EF's InMemory provider can share
/// state across contexts with the same database name.
/// </summary>
public static class TestDbFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new ApplicationDbContext(options);

        var customerRole = new Role { Name = "Customer" };
        var staffRole = new Role { Name = "Staff" };
        db.Roles.AddRange(customerRole, staffRole);
        db.SaveChanges();

        var unit1 = new Unit { Name = "Mobile Freezer", Category = "Freezer", Description = "Cold storage", PriceFrom = 750, Available = true };
        var unit2 = new Unit { Name = "Mobile Toilet", Category = "Toilet", Description = "Sanitation", PriceFrom = 500, Available = true };
        db.Units.AddRange(unit1, unit2);
        db.SaveChanges();

        var customerUser = new User { FullName = "Test Customer", Email = "test.customer@example.com", PasswordHash = "x", RoleId = customerRole.Id };
        db.Users.Add(customerUser);
        db.SaveChanges();

        var customer = new Customer { UserId = customerUser.Id, Phone = "0710000000" };
        db.Customers.Add(customer);

        var staffUser1 = new User { FullName = "Benjamin Operator", Email = "staff1@example.com", PasswordHash = "x", RoleId = staffRole.Id };
        var staffUser2 = new User { FullName = "Grace Field", Email = "staff2@example.com", PasswordHash = "x", RoleId = staffRole.Id };
        db.Users.AddRange(staffUser1, staffUser2);
        db.SaveChanges();

        return db;
    }

    public static int FirstUnitId(ApplicationDbContext db) => db.Units.OrderBy(u => u.Id).First().Id;
    public static int SecondUnitId(ApplicationDbContext db) => db.Units.OrderBy(u => u.Id).Skip(1).First().Id;
    public static int FirstCustomerId(ApplicationDbContext db) => db.Customers.OrderBy(c => c.Id).First().Id;
    public static int FirstStaffId(ApplicationDbContext db) => db.Users.OrderBy(u => u.Id).First(u => u.Role.Name == "Staff").Id;
    public static int SecondStaffId(ApplicationDbContext db) => db.Users.OrderBy(u => u.Id).Where(u => u.Role.Name == "Staff").Skip(1).First().Id;
}
