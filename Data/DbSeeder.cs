using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Models;
namespace MotshwaneConsortiumGroup.Data
{
    public class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            await context.Database.MigrateAsync();

            if (!await context.Roles.AnyAsync())
            {
                context.Roles.AddRange(
                    new Role
                    {
                        Name = "Admin"
                    },
                    new Role
                    {
                        Name = "Customer"
                    },
                    new Role
                    {
                        Name = "Staff"
                    }
                );

                await context.SaveChangesAsync();
            }

            if (!await context.Units.AnyAsync())
            {
                context.Units.AddRange
                (
                    new Unit
                    {
                        Name = "Mobile1 Freezer",
                        Category = "Freezer",
                        Description = "Portable cold-storage solution",
                        PriceFrom = 750,
                        Available = true
                    },
                    new Unit
                    {
                        Name = "Mobile Toilet",
                        Category = "Toilet",
                        Description = "Clean mobile sanitation solution",
                        PriceFrom = 500,
                        Available = true
                    },
                    new Unit
                    {
                        Name = "Premium Freezer",
                        Category = "Freezer",
                        Description = "Large mobile freezer",
                        PriceFrom = 1200,
                        Available = false
                    }
                );

                await context.SaveChangesAsync();
            }

            if (!await context.Users.AnyAsync())
            {
                var customerRole = await context.Roles
                    .FirstAsync(r => r.Name == "Customer");

                var staffRole = await context.Roles
                    .FirstAsync(r => r.Name == "Staff");

                var adminRole = await context.Roles
                    .FirstAsync(r => r.Name == "Admin");

                var users = new List<User>
                {
                    new User
                    {
                        FullName = "John Customer",
                        Email = "customer1@motshwane.co.za",
                        PasswordHash = "TEMP_PASSWORD",
                        RoleId = customerRole.Id
                    },

                    new User
                    {
                        FullName = "Jane Customer",
                        Email = "customer2@motshwane.co.za",
                        PasswordHash = "TEMP_PASSWORD",
                        RoleId = customerRole.Id
                    },

                    new User
                    {
                        FullName = "Admin User",
                        Email = "admin@motshwane.co.za",
                        PasswordHash = "TEMP_PASSWORD",
                        RoleId = adminRole.Id
                    },

                    new User
                    {
                        FullName = "Staff User",
                        Email = "staff@motshwane.co.za",
                        PasswordHash = "TEMP_PASSWORD",
                        RoleId = staffRole.Id
                    }
                };

                context.Users.AddRange(users);

                await context.SaveChangesAsync();
            }

            if (!await context.Customers.AnyAsync())
            {
                var customerUsers = await context.Users
                    .Where(u => u.Email.Contains("customer"))
                    .ToListAsync();

                context.Customers.AddRange(
                    new Customer
                    {
                        UserId = customerUsers[0].Id,
                        Phone = "071 000 0001"
                    },

                    new Customer
                    {
                        UserId = customerUsers[1].Id,
                        Phone = "071 000 0002"
                    }
                );

                await context.SaveChangesAsync();
            }

            if (!await context.Bookings.AnyAsync())
            {
                var customer = await context.Customers
                    .FirstAsync();

                var unit = await context.Units
                    .FirstAsync();

                context.Bookings.Add(
                    new Booking
                    {
                        Reference = "MC-001",
                        CustomerId = customer.Id,
                        UnitId = unit.Id,
                        BookingDate = new DateTime(2026, 10, 15),
                        Location = "Pretoria East",
                        Notes = "Test booking",
                        Status = "Pending",
                        PaymentStatus = "Awaiting Proof"
                    });

                await context.SaveChangesAsync();
            }
        }
    }
}
