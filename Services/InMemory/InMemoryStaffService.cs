using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.Interfaces;

namespace MotshwaneConsortiumGroup.Services.InMemory;

/// <summary>Temporary seed data until Thato's Staff/User table exists.</summary>
public class InMemoryStaffService : IStaffService
{
    private static readonly List<Staff> _staff = new()
    {
        new() { Id = 1, Name = "Benjamin Operator", Email = "benjamin@motshwane.co.za" },
        new() { Id = 2, Name = "Grace Field", Email = "grace@motshwane.co.za" },
    };

    public Task<IReadOnlyList<Staff>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Staff>>(_staff);
}
