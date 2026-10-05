using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Data;
using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.Interfaces;

namespace MotshwaneConsortiumGroup.Services.EfCore;

public class EfCustomerService : ICustomerService
{
    private readonly ApplicationDbContext _db;
    public EfCustomerService(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<Customer>> GetAllAsync() =>
        await _db.Customers.Include(c => c.User).AsNoTracking().OrderBy(c => c.User.FullName).ToListAsync();
}
