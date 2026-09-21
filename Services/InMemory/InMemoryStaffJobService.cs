using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.Interfaces;

namespace MotshwaneConsortiumGroup.Services.InMemory;

public class InMemoryStaffJobService : IStaffJobService
{
    private readonly DemoDataService _data;
    public InMemoryStaffJobService(DemoDataService data) => _data = data;

    public Task<IReadOnlyList<StaffJob>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<StaffJob>>(_data.Jobs);

    public Task<StaffJob?> GetByIdAsync(int id) =>
        Task.FromResult(_data.Jobs.FirstOrDefault(j => j.Id == id));

    public Task<bool> UpdateStatusAsync(int id, string status)
    {
        var job = _data.Jobs.FirstOrDefault(j => j.Id == id);
        if (job is null || string.IsNullOrWhiteSpace(status)) return Task.FromResult(false);
        job.Status = status;
        return Task.FromResult(true);
    }
}
