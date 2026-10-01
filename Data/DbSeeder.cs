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
                        Name = "Mobile Freezer",
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
        }
    }
}
