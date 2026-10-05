using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Data;
using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.Interfaces;

namespace MotshwaneConsortiumGroup.Services.EfCore;

public class EfCatalogService : ICatalogService
{
    private readonly ApplicationDbContext _db;
    public EfCatalogService(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<Unit>> GetServicesAsync(string? category = null)
    {
        var query = _db.Units.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(u => u.Category.ToLower() == category.ToLower());
        return await query.OrderBy(u => u.Name).ToListAsync();
    }
}
