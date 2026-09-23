using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.Interfaces;

namespace MotshwaneConsortiumGroup.Services.InMemory;

public class InMemoryCustomerService : ICustomerService
{
    private readonly DemoDataService _data;
    public InMemoryCustomerService(DemoDataService data) => _data = data;

    public Task<IReadOnlyList<Customer>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Customer>>(_data.Customers);
}
