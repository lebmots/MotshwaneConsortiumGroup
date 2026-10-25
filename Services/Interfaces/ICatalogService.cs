using MotshwaneConsortiumGroup.Models;

namespace MotshwaneConsortiumGroup.Services.Interfaces;

/// <summary>The list of rentable units (mobile freezers, mobile toilets, ...).</summary>
public interface ICatalogService
{
    /// <param name="category">Optional filter, e.g. "Freezer" or "Toilet". Case-insensitive.</param>
    Task<IReadOnlyList<Unit>> GetServicesAsync(string? category = null);
}
