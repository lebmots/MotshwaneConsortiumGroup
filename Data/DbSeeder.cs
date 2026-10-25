using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Models;
using System.Data;

namespace MotshwaneConsortiumGroup.Data;

public class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(
                new Role { Name = "Admin" },
                new Role { Name = "Customer" },
                new Role { Name = "Staff" }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Units.AnyAsync())
        {
            context.Units.AddRange(
                new Unit { Name = "Mobile Freezer", Category = "Freezer", Description = "Portable cold-storage solution", PriceFrom = 750, Available = true },
                new Unit { Name = "Mobile Toilet", Category = "Toilet", Description = "Clean mobile sanitation solution", PriceFrom = 500, Available = true },
                new Unit { Name = "Premium Freezer", Category = "Freezer", Description = "Large mobile freezer", PriceFrom = 1200, Available = true }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync())
        {
            var customerRole = await context.Roles.FirstAsync(r => r.Name == "Customer");
            var staffRole = await context.Roles.FirstAsync(r => r.Name == "Staff");
            var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");

            // Seed passwords are properly hashed below (not stored as plain text), using the
            // same PasswordHasher the real Register/Login actions use, so these accounts can
            // actually be signed into for the demo.
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();

            var customer1 = new User { FullName = "John Customer", Email = "customer1@motshwane.co.za", RoleId = customerRole.Id };
            customer1.PasswordHash = hasher.HashPassword(customer1, "Password1!");

            var customer2 = new User { FullName = "Jane Customer", Email = "customer2@motshwane.co.za", RoleId = customerRole.Id };
            customer2.PasswordHash = hasher.HashPassword(customer2, "Password1!");

            var admin = new User { FullName = "Admin User", Email = "admin@motshwane.co.za", RoleId = adminRole.Id };
            admin.PasswordHash = hasher.HashPassword(admin, "Password1!");

            var staff = new User { FullName = "Benjamin Operator", Email = "staff@motshwane.co.za", RoleId = staffRole.Id };
            staff.PasswordHash = hasher.HashPassword(staff, "Password1!");

            var staff2 = new User { FullName = "Grace Field", Email = "staff2@motshwane.co.za", RoleId = staffRole.Id };
            staff2.PasswordHash = hasher.HashPassword(staff2, "Password1!");

            context.Users.AddRange(customer1, customer2, admin, staff, staff2);
            await context.SaveChangesAsync();
        }

        if (!await context.Customers.AnyAsync())
        {
            var customerUsers = await context.Users.Where(u => u.Email.Contains("customer")).ToListAsync();
            context.Customers.AddRange(
                new Customer { UserId = customerUsers[0].Id, Phone = "071 000 0001" },
                new Customer { UserId = customerUsers[1].Id, Phone = "071 000 0002" }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Bookings.AnyAsync())
        {
            var customer = await context.Customers.FirstAsync();
            var unit = await context.Units.FirstAsync();

            context.Bookings.Add(new Booking
            {
                Reference = "MC-001",
                CustomerId = customer.Id,
                UnitId = unit.Id,
                BookingDate = new DateTime(2026, 10, 15),
                EndDate = new DateTime(2026, 10, 16),
                Location = "Pretoria East",
                Notes = "Test booking",
                Status = "Pending",
                PaymentStatus = "Awaiting Proof"
            });
            await context.SaveChangesAsync();
        }
    }
}
