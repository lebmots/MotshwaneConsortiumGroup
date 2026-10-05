using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Data;
using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.Interfaces;

namespace MotshwaneConsortiumGroup.Services.EfCore;

/// <summary>Staff are Users with Role = "Staff" — there's no separate Staff table.</summary>
public class EfStaffService : IStaffService
{
    private readonly ApplicationDbContext _db;
    public EfStaffService(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<Staff>> GetAllAsync()
    {
        var staffUsers = await _db.Users.AsNoTracking()
            .Where(u => u.Role.Name == "Staff")
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return staffUsers.Select(u => new Staff { Id = u.Id, Name = u.FullName, Email = u.Email }).ToList();
    }
}
