using MotshwaneConsortiumGroup.Models;

namespace MotshwaneConsortiumGroup.Services.Interfaces;

public interface IStaffService
{
    Task<IReadOnlyList<Staff>> GetAllAsync();
}
