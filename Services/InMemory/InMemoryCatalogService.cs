using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.Interfaces;

namespace MotshwaneConsortiumGroup.Services.InMemory;

public class InMemoryCatalogService : ICatalogService
{
    private readonly DemoDataService _data;
    public InMemoryCatalogService(DemoDataService data) => _data = data;

    public Task<IReadOnlyList<ServiceItem>> GetServicesAsync(string? category = null)
    {
        IReadOnlyList<ServiceItem> result = string.IsNullOrWhiteSpace(category)
            ? _data.Services
            : _data.Services
                .Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        return Task.FromResult(result);
    }
}
